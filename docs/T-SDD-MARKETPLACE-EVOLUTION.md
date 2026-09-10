# T-SDD: Evolución Comercial CPG Enterprises → SaaS-Enabled Marketplace

**Autores (equipo virtual):** Arquitecto Cloud Azure · Arquitecto .NET Backend · Lead Frontend React · Ingeniero QA/DevOps
**Basado en:** SDD Comercial (5 épicas) + estado actual del repositorio (`backend/src/CPG.*`, `frontend/src`, `SPEC.md`)
**Stack confirmado en código:** .NET 8 / Clean Architecture / DDD / CQRS (MediatR) / EF Core 8 · React 18 + TS + Tailwind · PostgreSQL · MassTransit sobre Azure Service Bus · Azure Container Apps · GitHub Actions OIDC

> **Nota de alineación con el código existente.** El MVP ya implementa `User` (con `UserRole.Admin|Carrier|Shipper`), `Carrier` (con `ComplianceDocument` embebido y máquina de estados `ComplianceStatus`), `Load` (aggregate con `Accept/MarkInTransit/MarkDelivered` y eventos de dominio), `Invoice`, `Lead`, un motor de tarifas determinista (`Features/Rates/Engine` — Strategy + Chain of Responsibility), un `MockStripePaymentService` y un `FleetTelemetrySimulator`. Este T-SDD **extiende** esas piezas en lugar de reemplazarlas; los mocks (`MockStripePaymentService`, `FleetTelemetrySimulator`) se sustituyen por implementaciones reales detrás de las mismas interfaces (`IPaymentService`, patrón `IXxxService` en `CPG.Application`), preservando el principio de inversión de dependencias ya vigente.

---

## 0. Decisiones arquitectónicas transversales (ADR resumido)

| # | Decisión | Justificación |
|---|---|---|
| ADR-01 | `UserRole` gana el valor `Agent = 4`. No se crea un sistema de roles paralelo. | Reutiliza el pipeline de RBAC/JWT de US-01 sin romper autorización existente. |
| ADR-02 | Cada perfil de negocio (`Carrier`, `Agent`) sigue siendo un **aggregate root propio** 1:1 con `User` (no herencia TPH). `Shipper` **no** obtiene aggregate propio en v1; se añaden campos opcionales (`CompanyName`, `PhoneNumber`) a `User`. | Consistente con el modelo actual (`Carrier.UserId`); evita joins TPH costosos; Shipper no requiere compliance ni payouts todavía. |
| ADR-03 | Todo webhook externo (ELD, Stripe) entra por un endpoint **anónimo pero firmado**, verificado por middleware dedicado antes de tocar MediatR. Nunca se confía en el payload sin verificar HMAC/firma. | Superficie de ataque pública — mitigación OWASP A08 (Software & Data Integrity). |
| ADR-04 | La actualización de mapa en tiempo real usa **Azure SignalR Service** (modo *serverless*) alimentado por un consumer de Service Bus, no polling HTTP. | Azure Container Apps es *scale-to-zero*; SignalR Service externaliza el estado de conexión WebSocket fuera de las réplicas. |
| ADR-05 | La dispersión de fondos (`PaymentDisbursement`) es una entidad de **ledger append-only** separada de `Invoice`. Nunca se sobrescribe un registro de pago; correcciones = nuevo registro de reversión. | Auditoría financiera B2B, reconciliación con Stripe, cumplimiento contable. |
| ADR-06 | El "pricing dinámico" del Épica 5 es una **agregación estadística determinista** sobre PostgreSQL (percentiles por carril/estacionalidad), no un modelo ML en v1. | Time-to-value; evita infraestructura MLOps prematura; deja la puerta abierta a Azure ML como v2 sin romper el contrato de API (`SuggestedRateUsd`). |
| ADR-07 | Toda nueva integración cruza el límite de bounded context exclusivamente vía **Integration Events** en Azure Service Bus (MassTransit), nunca por llamada síncrona entre features. | Ya es el patrón de `LoadAcceptedDomainEvent` → `LoadAcceptedNotificationConsumer`; se mantiene consistencia. |
| ADR-08 | Secrets (webhook secrets ELD, Stripe API keys, Stripe webhook signing secret) viven en **Azure Key Vault**, inyectados a Container Apps vía referencias de secreto (no env vars planas). | Seguridad B2B / rotación sin redeploy. |

---

## Épica 1 — Onboarding Automatizado (Tri-Sign-Up)

### 1.1 Modelado de Dominio y Base de Datos

```csharp
// CPG.Domain/Enums/UserRole.cs (modificado)
public enum UserRole
{
    Admin = 1,
    Carrier = 2,
    Shipper = 3,
    Agent = 4, // NUEVO
}

// CPG.Domain/Entities/User.cs (modificado — campos opcionales para Shipper/Agent sin aggregate propio)
public class User : AggregateRoot, IAuditableEntity
{
    // ...existing members...
    public string? CompanyName { get; set; }     // NUEVO — usado por Shipper
    public string? PhoneNumber { get; set; }      // NUEVO
}

// CPG.Domain/Entities/Agent.cs (NUEVO)
public class Agent : AggregateRoot, IAuditableEntity, IHasRowVersion, IHasStripeConnectAccount
{
    public required string CompanyName { get; set; }
    public required Guid UserId { get; set; }
    public required string CpgLicenseReference { get; set; } // referencia al DOT/MC de CPG bajo el cual opera
    public decimal CommissionRatePercent { get; set; } = 10.0m; // configurable por Admin
    public AgentStatus Status { get; private set; } = AgentStatus.PendingActivation;
    public string? StripeConnectAccountId { get; set; }
    public uint RowVersion { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }

    public void Activate() // tras completar checklist de compliance de agencia
    {
        if (Status != AgentStatus.PendingActivation)
            throw new DomainException($"Agent {Id} cannot be activated from status {Status}.");
        Status = AgentStatus.Active;
        RaiseDomainEvent(new AgentActivatedDomainEvent(Id, UserId));
    }
}

// CPG.Domain/Enums/AgentStatus.cs (NUEVO)
public enum AgentStatus { PendingActivation = 1, Active = 2, Suspended = 3 }

// CPG.Domain/Events/AgentActivatedDomainEvent.cs (NUEVO)
public sealed record AgentActivatedDomainEvent(Guid AgentId, Guid UserId) : IDomainEvent;

// CPG.Domain/Events/UserRegisteredDomainEvent.cs (NUEVO)
public sealed record UserRegisteredDomainEvent(Guid UserId, string Email, UserRole Role) : IDomainEvent;
```

**Migración EF Core:** `AddUserRoleAgent_AgentEntity_UserProfileFields`
- Nueva tabla `Agents` (FK `UserId` único → `Users.Id`, índice único).
- Constraint `CHECK` a nivel DB no es viable para enums de EF (se valida en dominio); se documenta en `SPEC.md` la invariante "un `User` tiene a lo sumo un perfil `Carrier` **o** `Agent`, nunca ambos".
- Columnas nuevas nullable en `Users`: `CompanyName varchar(200)`, `PhoneNumber varchar(20)`.

### 1.2 API y CQRS

| Tipo | Nombre | Notas |
|---|---|---|
| Command | `RegisterUserCommand` | `Email, Password, FullName, Role, CompanyName?, PhoneNumber?, DotNumber?, McNumber?` (los dos últimos solo si `Role == Carrier`). Transacción única: crea `User` + perfil (`Carrier`/`Agent`) + emite `UserRegisteredDomainEvent`. |
| Validator | `RegisterUserCommandValidator` | Password ≥ 12 chars con complejidad (OWASP ASVS 2.1), email único (`EmailMustBeUniqueAsync` contra `Users`), `Role != Admin` (Admin no se autoregistra). |
| Command | `ActivateAgentCommand` | Solo `Admin`; dispara `Agent.Activate()`. |
| Query | `GetMyProfileQuery` | Reemplaza necesidad de round-trip extra tras login: devuelve `User` + perfil de rol + estado de checklist. |
| IntegrationEvent | `UserRegisteredIntegrationEvent` | Publicado tras commit exitoso vía Service Bus → consumido por `WelcomeEmailConsumer` (nuevo) y por analítica (Épica 5 no depende de esto en v1). |

**Contrato REST:**
```
POST /api/auth/register          [AllowAnonymous]  → 201 Created (+ Location: /api/users/{id}) | 409 Conflict (email duplicado) | 400 Bad Request
POST /api/agents/{id}/activate   [Authorize(Roles="Admin")] → 200 OK | 404 | 409 (ya activo)
GET  /api/me                     [Authorize] → 200 OK
```

No hay webhooks externos en esta épica.

### 1.3 Arquitectura Frontend

```
frontend/src/features/auth/
  routes:
    /signup                       → RoleSelectionPage (Shipper | Carrier | Agent)
    /signup/shipper                → ShipperSignUpForm
    /signup/carrier                → CarrierSignUpForm
    /signup/agent                  → AgentSignUpForm
  components:
    RoleCard.tsx, SignUpFormShell.tsx, PasswordStrengthMeter.tsx
  state:
    useRegister() (TanStack Query mutation) → en éxito, guarda JWT (mismo store de auth/ actual)
                                              → router.navigate según Role:
                                                 Carrier → /carrier-portal (checklist 5 docs "Falta")
                                                 Shipper → /shipper-portal
                                                 Agent   → /agent-portal (estado "Pendiente de activación")
```

Reutiliza `shared/api/httpClient.ts` y el `AuthContext` ya existentes en `features/auth`; **no** se introduce Redux — se mantiene el patrón local (React Query + Context) ya usado en `carrier-portal`/`shipper-portal`.

### 1.4 Criterios de Aceptación (Gherkin BDD — API First)

```gherkin
Feature: Tri-Sign-Up

  Scenario: Registro exitoso de un Carrier
    Given no existe ningún usuario con email "ops@newcarrier.test"
    When envío POST /api/auth/register con { role: "Carrier", email: "ops@newcarrier.test", password: "Str0ngP@ssphrase!", fullName: "Ops Lead", dotNumber: "1234567" }
    Then la respuesta HTTP es 201 Created
    And el cuerpo contiene un JWT con claim role="Carrier"
    And existe una fila en "Carriers" con ComplianceStatus = "PendingCompliance"

  Scenario: Registro duplicado
    Given ya existe un usuario con email "ops@newcarrier.test"
    When envío POST /api/auth/register con ese mismo email
    Then la respuesta HTTP es 409 Conflict

  Scenario: Registro con password débil
    When envío POST /api/auth/register con password "12345"
    Then la respuesta HTTP es 400 Bad Request
    And el problem+json incluye el campo "password" en errors

  Scenario: Un Agente no puede activarse a sí mismo
    Given un Agent autenticado con status "PendingActivation"
    When envía POST /api/agents/{suPropioId}/activate
    Then la respuesta HTTP es 401 Unauthorized
```

### 1.5 Notas de Roadmap
Ver §Roadmap General — **Sprint 1**, sin dependencias previas salvo la migración de esquema del Sprint 0.

---

## Épica 2A — Telemetría ELD (GPS en tiempo real)

### 2A.1 Modelado de Dominio y Base de Datos

```csharp
// CPG.Domain/Enums/TelemetryProvider.cs (NUEVO)
public enum TelemetryProvider { Samsara = 1, Motive = 2, KeepTruckin = 3 }

// CPG.Domain/Entities/TelemetryDevice.cs (NUEVO)
public class TelemetryDevice : Entity, IAuditableEntity
{
    public required Guid CarrierId { get; set; }
    public required TelemetryProvider Provider { get; set; }
    public required string ExternalDeviceId { get; set; }     // id del vehículo/ELD en el proveedor
    public required string WebhookSecretHash { get; set; }     // HMAC secret, hasheado en reposo
    public DateTimeOffset? LastSeenAtUtc { get; set; }
    // IAuditableEntity members...
}

// CPG.Domain/Entities/TelemetryLog.cs (NUEVO) — append-only, alto volumen
public class TelemetryLog : Entity
{
    public required Guid LoadId { get; set; }
    public required Guid TelemetryDeviceId { get; set; }
    public required decimal Latitude { get; set; }
    public required decimal Longitude { get; set; }
    public decimal? SpeedMph { get; set; }
    public decimal? HeadingDegrees { get; set; }
    public required DateTimeOffset RecordedAtUtc { get; set; }  // timestamp del hardware
    public DateTimeOffset ReceivedAtUtc { get; set; }            // timestamp de ingesta CPG
}

// CPG.Domain/Entities/Load.cs (modificado — proyección de última posición para lecturas rápidas)
public class Load : AggregateRoot, IAuditableEntity, IHasRowVersion, ISoftDelete
{
    // ...existing members...
    public decimal? LastKnownLatitude { get; set; }
    public decimal? LastKnownLongitude { get; set; }
    public DateTimeOffset? LastTelemetryAtUtc { get; set; }

    public void UpdateTelemetry(decimal lat, decimal lng, DateTimeOffset recordedAtUtc) // NUEVO método de dominio
    {
        if (Status != LoadStatus.InTransit && Status != LoadStatus.Dispatched)
            throw new DomainException($"Load {Reference} cannot receive telemetry in status {Status}.");
        if (LastTelemetryAtUtc is { } last && recordedAtUtc <= last) return; // idempotencia / fuera de orden

        LastKnownLatitude = lat; LastKnownLongitude = lng; LastTelemetryAtUtc = recordedAtUtc;
        RaiseDomainEvent(new LoadGpsLocationUpdatedDomainEvent(Id, Reference, lat, lng, recordedAtUtc));
    }
}
```

**Migración EF Core:** `AddTelemetryDeviceAndLog_LoadLastKnownPosition`
- Índice `IX_TelemetryLog_LoadId_RecordedAtUtc` (consultas de historial ordenado).
- Considerar particionado por mes (tabla `telemetry_log` con `pg_partman` o partición nativa PG 15) si el volumen ELD supera ~5M filas/mes — documentado como *fast-follow*, no bloqueante para MVP de esta épica.

### 2A.2 API y CQRS

| Tipo | Nombre | Notas |
|---|---|---|
| Command | `IngestTelemetryWebhookCommand` | `Provider, RawPayload (JSON), SignatureHeader`. Ejecutado **después** de que el middleware de firma valide el HMAC — el comando ya recibe el payload como confiable. |
| Command | `RegisterTelemetryDeviceCommand` | Admin/Carrier vincula un `ExternalDeviceId` a un `Carrier` y genera/rota el `WebhookSecretHash`. |
| Query | `GetLoadCurrentLocationQuery` | Devuelve `LastKnownLatitude/Longitude/LastTelemetryAtUtc` — usado en carga inicial del mapa (antes de que llegue el primer push realtime). |
| Query | `GetLoadTelemetryHistoryQuery` | Paginado, para reconstrucción de ruta (replay). |
| Domain Event | `LoadGpsLocationUpdatedDomainEvent` | Disparado por `Load.UpdateTelemetry`. |
| **IntegrationEvent (Service Bus)** | `LoadGpsLocationUpdatedIntegrationEvent(LoadId, Reference, ShipperUserId, Lat, Lng, RecordedAtUtc)` | Publicado por `LoadGpsLocationUpdatedDomainEvent` handler (mismo patrón que `LoadDeliveredNotificationConsumer`). |
| Consumer | `LoadGpsLocationUpdatedConsumer` | Suscrito al tópico; reenvía a **Azure SignalR Service** al grupo `load-{loadId}` (solo si `ShipperUserId` coincide con la conexión autenticada). |

**Estrategia de verificación de firma (webhook ELD):**
1. Middleware `TelemetryWebhookSignatureMiddleware` intercepta `POST /api/webhooks/telemetry/{provider}` **antes** de MediatR.
2. Resuelve el `TelemetryDevice` por `ExternalDeviceId` (extraído del payload sin confiar en él para lógica, solo para lookup) → obtiene `WebhookSecretHash`.
3. Recalcula HMAC-SHA256 sobre el *raw body* con el secreto y compara contra el header (`X-Samsara-Signature`, `X-Motive-Signature`, etc., vía `ITelemetrySignatureVerifier` con una implementación por proveedor — Strategy pattern, igual que `IServiceRateStrategy`).
4. Comparación con `CryptographicOperations.FixedTimeEquals` (mitiga *timing attacks*).
5. Firma inválida → **401 Unauthorized** inmediato, no se llega a MediatR, se audita el intento (`AuditLogEntry`).
6. Replay protection: rechaza timestamps del payload con *drift* > 5 minutos (mitiga *replay attacks*).

**Contrato REST:**
```
POST /api/webhooks/telemetry/{provider}   [AllowAnonymous + firma]  → 202 Accepted | 401 Unauthorized | 400 Bad Request (payload malformado)
POST /api/carriers/{id}/telemetry-devices [Authorize(Roles="Carrier,Admin")] → 201 Created
GET  /api/loads/{id}/location             [Authorize] → 200 OK | 404
GET  /api/loads/{id}/telemetry-history    [Authorize] → 200 OK
```
`202 Accepted` (no `200`) para el webhook: la ingesta es asíncrona respecto a la propagación SignalR.

### 2A.3 Arquitectura Frontend

```
frontend/src/features/telemetry/
  routes: (se monta dentro de shipper-portal, no ruta top-level)
    /shipper-portal/loads/:loadId/tracking → LoadTrackingPage
  components:
    LiveMap.tsx (MapLibre GL / Leaflet — evaluar licencia, evitar Google Maps de pago)
    TruckMarker.tsx, RouteReplay.tsx
  state:
    useSignalRConnection(loadId) — hook que abre conexión a Azure SignalR (negotiate endpoint /api/signalr/negotiate),
      se une al grupo `load-{loadId}`, expone `latestPosition` reactivo.
    useLoadLocationQuery(loadId) — carga inicial (React Query) antes de que llegue el primer evento realtime.
```

### 2A.4 Criterios de Aceptación (Gherkin BDD)

```gherkin
Feature: Telemetría ELD en tiempo real

  Scenario: Webhook con firma válida actualiza la posición
    Given una carga "LOAD-1001" en estado "InTransit" con un TelemetryDevice registrado
    When el proveedor ELD envía POST /api/webhooks/telemetry/samsara con coordenadas válidas y firma HMAC correcta
    Then la respuesta HTTP es 202 Accepted
    And se persiste una fila en TelemetryLog
    And Load.LastKnownLatitude/Longitude se actualizan
    And se publica LoadGpsLocationUpdatedIntegrationEvent en Azure Service Bus

  Scenario: Webhook con firma inválida es rechazado
    When el proveedor ELD envía POST /api/webhooks/telemetry/samsara con firma incorrecta
    Then la respuesta HTTP es 401 Unauthorized
    And no se crea ninguna fila en TelemetryLog

  Scenario: El Shipper ve la actualización en el mapa
    Given el Shipper tiene una conexión SignalR activa al grupo "load-LOAD-1001"
    When se publica un LoadGpsLocationUpdatedIntegrationEvent para esa carga
    Then el cliente recibe el mensaje "loadLocationUpdated" en < 2 segundos (p95)
```

---

## Épica 2B — Automatización Financiera (Quick Pay / Stripe Connect)

### 2B.1 Modelado de Dominio y Base de Datos

```csharp
// CPG.Domain/Entities/Carrier.cs (modificado)
public class Carrier : AggregateRoot, IAuditableEntity, IHasRowVersion, IHasStripeConnectAccount
{
    // ...existing members...
    public string? StripeConnectAccountId { get; set; }          // NUEVO
    public StripeOnboardingStatus StripeOnboardingStatus { get; set; } = StripeOnboardingStatus.NotStarted; // NUEVO
}

// CPG.Domain/Enums/StripeOnboardingStatus.cs (NUEVO — compartido por Carrier y Agent vía IHasStripeConnectAccount)
public enum StripeOnboardingStatus { NotStarted = 1, PendingVerification = 2, Active = 3, Restricted = 4 }

// CPG.Domain/Entities/PaymentDisbursement.cs (NUEVO — ledger append-only)
public class PaymentDisbursement : AggregateRoot, IAuditableEntity
{
    public required Guid LoadId { get; set; }
    public required Guid InvoiceId { get; set; }
    public required Guid CarrierId { get; set; }
    public Guid? AgentId { get; set; }                      // NULL si no hay agente involucrado (Épica 4)
    public required decimal GrossAmountUsd { get; set; }
    public required decimal CpgMarginAmountUsd { get; set; }
    public decimal AgentCommissionAmountUsd { get; set; }    // 0 si no aplica
    public decimal QuickPayFeeAmountUsd { get; set; }        // 0 si el Carrier no solicitó Quick Pay
    public required decimal CarrierNetAmountUsd { get; set; }
    public bool QuickPayRequested { get; set; }
    public DisbursementStatus Status { get; private set; } = DisbursementStatus.Pending;
    public string? StripeTransferId { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public string? FailureReason { get; set; }

    public void MarkCompleted(string stripeTransferId, DateTimeOffset processedAtUtc) { /* invariantes + RaiseDomainEvent(new PaymentDisbursementCompletedDomainEvent(...)) */ }
    public void MarkFailed(string reason) { /* Status = Failed; ... */ }
}

// CPG.Domain/Enums/DisbursementStatus.cs (NUEVO)
public enum DisbursementStatus { Pending = 1, Processing = 2, Completed = 3, Failed = 4 }
```

**Migración EF Core:** `AddStripeConnect_PaymentDisbursement`
- `Carriers.StripeConnectAccountId` (nullable, índice único parcial `WHERE NOT NULL`).
- Tabla `PaymentDisbursements` con FKs a `Loads`, `Invoices`, `Carriers`, `Agents` (nullable).
- **No** se borra ni actualiza in-place un `PaymentDisbursement` completado; correcciones = nueva fila con referencia (`CorrectionOfDisbursementId`, *fast-follow*).

### 2B.2 API y CQRS

| Tipo | Nombre | Notas |
|---|---|---|
| Command | `CreateStripeConnectAccountCommand` | Carrier/Agent inicia onboarding → crea cuenta Express en Stripe, devuelve `AccountLinkUrl` (hosted onboarding, evita que CPG maneje KYC directamente — reduce alcance PCI/regulatorio). |
| Command | `RequestQuickPayCommand` | Carrier, sobre una `Invoice` en estado pagable, marca `QuickPayRequested = true`. |
| Command | `ProcessLoadDisbursementCommand` | Disparado automáticamente por el handler de `LoadDeliveredDomainEvent` **una vez el POD está aprobado** (reutiliza el flujo existente de `Invoice`); calcula márgenes vía `IDisbursementCalculator` y llama a `IStripeConnectService.CreateTransferAsync`. |
| Command | `HandleStripeWebhookCommand` | Procesa eventos `account.updated`, `transfer.paid`, `transfer.failed` desde Stripe. |
| Query | `GetCarrierPayoutHistoryQuery` | Historial paginado de `PaymentDisbursement` por Carrier. |
| Query | `GetPendingDisbursementsQuery` | Vista operativa para Admin/Finance. |
| **IntegrationEvent** | `QuickPayRequestedIntegrationEvent` | |
| **IntegrationEvent** | `QuickPayProcessedIntegrationEvent(LoadId, CarrierId, NetAmountUsd, StripeTransferId)` | Consumida por notificaciones (email/SMS al Carrier) y por Épica 5 (dataset de rentabilidad real). |
| **IntegrationEvent** | `PaymentDisbursementFailedIntegrationEvent` | Alerta a Admin (Service Bus → `AdminAlertConsumer`). |

**Cálculo de la dispersión (`IDisbursementCalculator`, determinista y unit-testeable):**
```
CpgMarginAmountUsd      = GrossAmountUsd * CpgMarginRate            (default 5%, configurable por ServiceType)
AgentCommissionAmountUsd= (GrossAmountUsd - CpgMarginAmountUsd) * Agent.CommissionRatePercent   (si Load.AgentId != null)
QuickPayFeeAmountUsd    = (GrossAmountUsd - CpgMarginAmountUsd - AgentCommissionAmountUsd) * QuickPayFeeRate  (default 3%, solo si QuickPayRequested)
CarrierNetAmountUsd     = GrossAmountUsd - CpgMarginAmountUsd - AgentCommissionAmountUsd - QuickPayFeeAmountUsd
```

**Estrategia de verificación de webhooks Stripe:**
- `POST /api/webhooks/stripe` anónimo; usa `Stripe.net`'s `EventUtility.ConstructEvent(payload, header, webhookSigningSecret)`, que valida la firma `Stripe-Signature` (timestamp + HMAC-SHA256) — **nunca** deserializar el body antes de esta validación.
- `webhookSigningSecret` en Azure Key Vault, distinto por ambiente (test/live).
- Idempotencia: Stripe puede reintentar el mismo evento → tabla `ProcessedStripeEventIds` (unique constraint sobre `stripe_event_id`) para descartar duplicados antes de procesar.

**Contrato REST:**
```
POST /api/carriers/{id}/stripe-connect         [Authorize(Roles="Carrier")] → 201 Created (AccountLinkUrl) | 409 (ya conectado)
POST /api/invoices/{id}/quick-pay              [Authorize(Roles="Carrier")] → 202 Accepted | 400 (Invoice no elegible) | 404
POST /api/webhooks/stripe                      [AllowAnonymous + firma]     → 200 OK | 400 (firma inválida)
GET  /api/carriers/{id}/payouts                [Authorize(Roles="Carrier,Admin")] → 200 OK
```

### 2B.3 Arquitectura Frontend

```
frontend/src/features/carrier-portal/billing/  (extiende carrier-portal existente)
  routes:
    /carrier-portal/payouts                 → PayoutHistoryPage
    /carrier-portal/settings/stripe-connect  → StripeConnectOnboardingPage (redirige a AccountLinkUrl de Stripe)
  components:
    QuickPayToggle.tsx (en la vista de detalle de Invoice — "Recibir en 24h con 3% de descuento")
    PayoutStatusBadge.tsx
  state:
    useRequestQuickPay() mutation, useCarrierPayouts() query
```

### 2B.4 Criterios de Aceptación (Gherkin BDD)

```gherkin
Feature: Quick Pay y dispersión automática

  Scenario: Dispersión estándar sin Quick Pay
    Given una carga "LOAD-2001" en estado "Delivered" con POD aprobado y GrossAmountUsd = 1000.00
    And el Carrier tiene un StripeConnectAccountId activo
    When el sistema procesa ProcessLoadDisbursementCommand
    Then se crea un PaymentDisbursement con CpgMarginAmountUsd = 50.00 y CarrierNetAmountUsd = 950.00
    And el estado final es "Completed"
    And se publica QuickPayProcessedIntegrationEvent

  Scenario: Carrier solicita Quick Pay
    Given una Invoice pagable para "LOAD-2002"
    When el Carrier envía POST /api/invoices/{id}/quick-pay
    Then la respuesta HTTP es 202 Accepted
    And el disbursement subsecuente descuenta adicionalmente el 3% de QuickPayFeeAmountUsd

  Scenario: Webhook de Stripe con firma inválida
    When llega POST /api/webhooks/stripe con un header Stripe-Signature manipulado
    Then la respuesta HTTP es 400 Bad Request
    And no se actualiza ningún PaymentDisbursement

  Scenario: Carrier sin cuenta Stripe conectada no puede recibir Quick Pay
    Given un Carrier con StripeConnectAccountId = null
    When se intenta ProcessLoadDisbursementCommand para su carga entregada
    Then el PaymentDisbursement queda en estado "Failed" con FailureReason = "StripeAccountNotConnected"
    And se publica PaymentDisbursementFailedIntegrationEvent
```

---

## Épica 3 — Garantía de Red y Prueba Social (Marketing B2B)

### 3.1 Modelado de Dominio y Base de Datos

```csharp
// CPG.Domain/Entities/CaseStudy.cs (NUEVO — contenido gestionado, no transaccional core)
public class CaseStudy : AggregateRoot, IAuditableEntity
{
    public required string Title { get; set; }
    public required string Slug { get; set; }              // único, usado en URL
    public required ServiceType ServiceType { get; set; }
    public required string SummaryMarkdown { get; set; }
    public required string BodyMarkdown { get; set; }
    public string? HeroImageBlobUri { get; set; }
    public bool IsPublished { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; }
}
```

**Migración EF Core:** `AddCaseStudy`. Tabla ligera, sin relación con el core transaccional (bounded context de "Marketing Content", intencionalmente desacoplado).

### 3.2 API y CQRS

| Tipo | Nombre | Notas |
|---|---|---|
| Command | `CreateCaseStudyCommand` | Admin. |
| Command | `PublishCaseStudyCommand` | Admin; setea `IsPublished = true`, `PublishedAtUtc`. |
| Query | `GetPublishedCaseStudiesQuery` | `[AllowAnonymous]`, filtrable por `ServiceType`, usado por landing page. |
| Query | `GetCaseStudyBySlugQuery` | `[AllowAnonymous]`. |

No hay eventos de integración (contenido editorial, no dispara side-effects de dominio) ni webhooks externos.

**Contrato REST:**
```
GET  /api/case-studies                 [AllowAnonymous] → 200 OK
GET  /api/case-studies/{slug}          [AllowAnonymous] → 200 OK | 404
POST /api/case-studies                 [Authorize(Roles="Admin")] → 201 Created
POST /api/case-studies/{id}/publish    [Authorize(Roles="Admin")] → 200 OK | 409 (ya publicado)
```

### 3.3 Arquitectura Frontend

```
frontend/src/features/landing/network-guarantee/  (NUEVO, integrado en páginas de servicio existentes)
  components:
    NetworkGuaranteeSection.tsx  — infografía interactiva del proceso de verificación (reutiliza iconografía de compliance)
    CaseStudyCarousel.tsx        — carrusel accesible (teclado + swipe), lazy-loaded (code splitting)
    CaseStudyCard.tsx
  routes:
    /case-studies/:slug → CaseStudyDetailPage (SSR/prerender opcional vía Vite SSG plugin si SEO es prioridad)
  state:
    usePublishedCaseStudies(serviceType?) — React Query, cache larga (staleTime alto, contenido editorial cambia poco)
```

### 3.4 Criterios de Aceptación (Gherkin BDD)

```gherkin
Feature: Garantía de Red Certificada

  Scenario: Listado público de casos de estudio publicados
    Given existen 3 CaseStudy con IsPublished = true y 1 con IsPublished = false
    When un visitante anónimo envía GET /api/case-studies?serviceType=HeavyHaul
    Then la respuesta HTTP es 200 OK
    And el cuerpo contiene únicamente los registros publicados que coincidan con HeavyHaul

  Scenario: Un caso de estudio no publicado no es accesible
    Given un CaseStudy con slug "megaload-orlando" y IsPublished = false
    When un visitante anónimo envía GET /api/case-studies/megaload-orlando
    Then la respuesta HTTP es 404 Not Found

  Scenario: Solo Admin puede publicar
    Given un usuario autenticado con rol "Carrier"
    When envía POST /api/case-studies/{id}/publish
    Then la respuesta HTTP es 403 Forbidden
```

---

## Épica 4 — Portal de Agencias (El Ecosistema CPG)

### 4.1 Modelado de Dominio y Base de Datos

```csharp
// CPG.Domain/Entities/Load.cs (modificado)
public class Load : AggregateRoot, IAuditableEntity, IHasRowVersion, ISoftDelete
{
    // ...existing members...
    public Guid? AgentId { get; set; }                       // NUEVO — atribución de la carga a un Agente
    public decimal? ProjectedAgentCommissionUsd { get; set; } // NUEVO — pre-cálculo mostrado en el dashboard

    public void AttributeToAgent(Guid agentId, decimal projectedCommissionUsd) // NUEVO
    {
        if (Status != LoadStatus.Available)
            throw new DomainException("Solo se puede atribuir una carga recién publicada.");
        AgentId = agentId;
        ProjectedAgentCommissionUsd = projectedCommissionUsd;
        RaiseDomainEvent(new LoadPublishedByAgentDomainEvent(Id, agentId, projectedCommissionUsd));
    }
}

// CPG.Domain/Entities/AgentClientInvitation.cs (NUEVO)
public class AgentClientInvitation : Entity, IAuditableEntity
{
    public required Guid AgentId { get; set; }
    public required string InvitedEmail { get; set; }
    public Guid? AcceptedByUserId { get; set; }
    public required string Token { get; set; }               // opaco, un solo uso
    public InvitationStatus Status { get; set; } = InvitationStatus.Sent;
    public required DateTimeOffset ExpiresAtUtc { get; set; }
}

// CPG.Domain/Enums/InvitationStatus.cs (NUEVO)
public enum InvitationStatus { Sent = 1, Accepted = 2, Expired = 3, Revoked = 4 }

// CPG.Domain/Events/LoadPublishedByAgentDomainEvent.cs (NUEVO)
public sealed record LoadPublishedByAgentDomainEvent(Guid LoadId, Guid AgentId, decimal ProjectedCommissionUsd) : IDomainEvent;
```

**Migración EF Core:** `AddAgentEcosystem_LoadAttribution_ClientInvitations`
- `Loads.AgentId` (FK nullable → `Agents.Id`, índice).
- Tabla `AgentClientInvitations` (índice único sobre `Token`, índice sobre `AgentId`).

### 4.2 API y CQRS

| Tipo | Nombre | Notas |
|---|---|---|
| Command | `InviteClientCommand` | Agent invita por email; genera `Token` criptográficamente aleatorio (`RandomNumberGenerator`), expira en 7 días. |
| Command | `AcceptClientInvitationCommand` | El invitado (nuevo o existente) acepta → vincula `ShipperUserId`, marca `Accepted`. |
| Command | `PublishAgentLoadCommand` | Extiende el `CreateLoadCommand` existente: si el solicitante es `Agent`, calcula `ProjectedAgentCommissionUsd` vía `IDisbursementCalculator` (reutilizado de Épica 2B) y llama a `Load.AttributeToAgent`. |
| Query | `GetAgentDashboardQuery` | Cargas publicadas, comisiones proyectadas (pendientes) vs. reales (desde `PaymentDisbursement.AgentCommissionAmountUsd` — Épica 2B). |
| Query | `GetAgentClientsQuery` | Lista de invitaciones + clientes activos. |
| **IntegrationEvent** | `AgentCommissionAccruedIntegrationEvent(AgentId, LoadId, AmountUsd)` | Publicado cuando `PaymentDisbursement` se completa con `AgentId != null` (extiende el consumer de Épica 2B, no crea uno nuevo). |
| **IntegrationEvent** | `ClientInvitationSentIntegrationEvent` | Consumida por `EmailNotificationConsumer` (envío del correo de invitación). |

**Contrato REST:**
```
POST /api/agents/{id}/clients/invite        [Authorize(Roles="Agent")]                    → 201 Created | 409 (invitación duplicada pendiente)
POST /api/agents/invitations/{token}/accept [AllowAnonymous o Authorize — ver nota abajo]  → 200 OK | 410 Gone (expirada) | 404
POST /api/loads                             [Authorize(Roles="Agent,Shipper,Admin")]       → 201 Created  (comando existente, extendido)
GET  /api/agents/{id}/dashboard             [Authorize(Roles="Agent")]                     → 200 OK | 403 (no es su propio dashboard)
```
> Nota de seguridad: `accept` permite anónimo únicamente si el invitado no tiene cuenta aún (flujo combina aceptación + registro); si ya existe cuenta, se exige `Authorize` y se valida que el `email` del JWT coincida con `InvitedEmail` (evita *account takeover* por token filtrado).

### 4.3 Arquitectura Frontend

```
frontend/src/features/agent-portal/  (NUEVO, hermano de carrier-portal/shipper-portal)
  routes:
    /agent-portal                    → AgentDashboardPage (KPI: comisiones acumuladas, cargas activas)
    /agent-portal/loads/new          → PublishAgentLoadForm (reutiliza componentes de load-board donde aplique)
    /agent-portal/clients            → ClientListPage
    /agent-portal/clients/invite     → InviteClientModal
  components:
    CommissionSummaryCard.tsx, AgentLoadTable.tsx
  state:
    useAgentDashboard(), useInviteClient() mutation
```

### 4.4 Criterios de Aceptación (Gherkin BDD)

```gherkin
Feature: Portal de Agencias

  Scenario: Atribución de carga y comisión proyectada
    Given un Agent autenticado y activo con CommissionRatePercent = 10%
    When publica una carga con GrossAmountUsd proyectado de 1000.00 y CpgMarginAmountUsd de 50.00
    Then la respuesta HTTP es 201 Created
    And Load.AgentId corresponde al Agent autenticado
    And Load.ProjectedAgentCommissionUsd = 95.00

  Scenario: Invitación de cliente exitosa
    Given un Agent activo
    When envía POST /api/agents/{id}/clients/invite con email "cliente@empresa.com"
    Then la respuesta HTTP es 201 Created
    And se publica ClientInvitationSentIntegrationEvent

  Scenario: Aceptación de invitación expirada
    Given una AgentClientInvitation con ExpiresAtUtc en el pasado
    When se envía POST /api/agents/invitations/{token}/accept
    Then la respuesta HTTP es 410 Gone

  Scenario: Un Agente no puede ver el dashboard de otro Agente
    Given dos Agentes A y B autenticados
    When A envía GET /api/agents/{idDeB}/dashboard
    Then la respuesta HTTP es 403 Forbidden
```

---

## Épica 5 — Inteligencia Predictiva y Pricing Dinámico

### 5.1 Modelado de Dominio y Base de Datos

```csharp
// CPG.Domain/Entities/LaneRateStatistic.cs (NUEVO — read-model, recalculado por job, no es aggregate transaccional)
public class LaneRateStatistic : Entity
{
    public required string OriginZip3 { get; set; }
    public required string DestinationZip3 { get; set; }
    public required ServiceType ServiceType { get; set; }
    public required int Month { get; set; }               // 1-12, estacionalidad
    public required decimal AvgRatePerMileUsd { get; set; }
    public required decimal P25RatePerMileUsd { get; set; }
    public required decimal P75RatePerMileUsd { get; set; }
    public required int SampleSize { get; set; }
    public required DateTimeOffset ComputedAtUtc { get; set; }
}
```

**Migración EF Core:** `AddLaneRateStatistic`
- Índice único compuesto `(OriginZip3, DestinationZip3, ServiceType, Month)`.
- Tabla poblada exclusivamente por un **job batch nocturno** (no por tráfico de request), evitando contención con el path caliente de `POST /api/rates/calculate`.

### 5.2 API y CQRS

| Tipo | Nombre | Notas |
|---|---|---|
| Query | `CalculateRateQuery` (existente, **extendida**) | Se añade `SuggestedRateUsd`, `SuggestedRateConfidence` (enum `Low/Medium/High` según `SampleSize`) a `CalculateRateResponse`. |
| Job (no HTTP) | `RecomputeLaneRateStatisticsJob` | Ejecutado como **Azure Container Apps Job** (cron, no un endpoint) — agrega `Loads` con `Status = Delivered` de los últimos 18 meses, agrupando por `(OriginZip3, DestinationZip3, ServiceType, Month)`, calcula avg/percentiles vía SQL (`percentile_cont` nativo de PostgreSQL). |
| Query | `GetLaneRateStatisticsQuery` | Admin/Agent — transparencia sobre de dónde sale la sugerencia. |

**Extensión del motor determinista existente (Chain of Responsibility, sin romper el contrato de 500ms de US-02):**
```csharp
// CPG.Application/Features/Rates/Engine/HistoricalMarginAdjustmentHandler.cs (NUEVO)
// Se añade AL FINAL de la cadena existente: Base → ColdChainSurcharge → FuelSurcharge → HistoricalMarginAdjustment
public sealed class HistoricalMarginAdjustmentHandler : SurchargeHandler
{
    // Lee LaneRateStatistic (ya materializada, lookup O(1) por índice compuesto — no hay agregación en el request path)
    // Si no hay estadística (SampleSize == 0) → SuggestedRateUsd = BaseTotal (fallback sin ajuste), Confidence = "Low"
}
```
> Decisión clave de rendimiento: el cálculo pesado (percentiles sobre histórico) **nunca** ocurre en el request de cotización; el request solo hace un `SELECT` indexado sobre la tabla pre-agregada. Esto preserva el SLA de <500ms ya validado en US-02.

No hay `IntegrationEvent` nuevo (proceso de lectura/batch interno); no hay webhook externo.

**Contrato REST:**
```
POST /api/rates/calculate                  [AllowAnonymous]              → 200 OK  (respuesta extendida con SuggestedRateUsd)
GET  /api/admin/lane-rate-statistics       [Authorize(Roles="Admin")]    → 200 OK
```

### 5.3 Arquitectura Frontend

```
frontend/src/features/rates/  (extiende la calculadora existente, US-02)
  components:
    RateBreakdown.tsx (MODIFICADO) — nueva fila "Tarifa Sugerida" con badge de confianza (Low/Medium/High)
    SuggestedRateTooltip.tsx (NUEVO) — explica brevemente "basado en N operaciones similares"
  state:
    useRateCalculator (existente) — el DTO de respuesta se extiende, sin cambios de forma en el hook
```

### 5.4 Criterios de Aceptación (Gherkin BDD)

```gherkin
Feature: Tarifa Sugerida por Inteligencia de Mercado

  Scenario: Sugerencia con historial suficiente
    Given existen 50 Loads entregadas en la ruta Orlando(32801)-Miami(33101) para ServiceType "Flatbed" en el mes actual
    And el job RecomputeLaneRateStatisticsJob ya corrió
    When se envía POST /api/rates/calculate para esa ruta y equipo
    Then la respuesta HTTP es 200 OK en menos de 500 ms
    And el cuerpo incluye SuggestedRateUsd distinto de null
    And SuggestedRateConfidence es "High" (SampleSize >= 30)

  Scenario: Sin historial disponible, fallback seguro
    Given no existe LaneRateStatistic para la ruta consultada
    When se envía POST /api/rates/calculate
    Then la respuesta HTTP es 200 OK
    And SuggestedRateUsd == BaseTotal (sin ajuste)
    And SuggestedRateConfidence es "Low"

  Scenario: Solo Admin ve el detalle estadístico crudo
    Given un usuario autenticado con rol "Carrier"
    When envía GET /api/admin/lane-rate-statistics
    Then la respuesta HTTP es 403 Forbidden
```

---

## Roadmap de Ejecución (Sprints)

Principio rector: **Dominio/EF Core → API/CQRS → Eventos → UI**, y *"lo que bloquea a más gente va primero"*. `Agent`, `UserRole.Agent` y `StripeConnectAccountId` son dependencias transversales, por eso el esquema completo se cierra en el Sprint 0.

| Sprint | Contenido | Depende de | Desbloquea |
|---|---|---|---|
| **Sprint 0** | Todas las migraciones EF Core de las 5 épicas en una sola PR de esquema (`UserRole.Agent`, `Agent`, `TelemetryDevice/Log`, `Load` (campos GPS + AgentId), `PaymentDisbursement`, `CaseStudy`, `LaneRateStatistic`). ADRs §0 documentados y aprobados. Actualizar `docker-compose.yml`/`Directory.Packages.props` si se añaden paquetes (`Stripe.net`, `Microsoft.Azure.SignalR`). | — | Todos los sprints siguientes |
| **Sprint 1** | **Épica 1** completa (dominio → `RegisterUserCommand` → `UserRegisteredIntegrationEvent` → UI de signup). Paralelizable: **Épica 3** (frontend-only, sin dependencia de Sprint 0 salvo `CaseStudy`) puede correr en paralelo con un dev de frontend dedicado. | Sprint 0 | Sprints 2, 3, 4 (necesitan roles/Agent) |
| **Sprint 2** | **Épica 2A** (Telemetría): `TelemetryDevice/Log` → webhook + verificación de firma → `LoadGpsLocationUpdatedIntegrationEvent` → Azure SignalR → mapa React. | Sprint 0 | Nada crítico, pero valida el patrón de webhook firmado reutilizado en Sprint 3 |
| **Sprint 3** | **Épica 2B** (Quick Pay): Stripe Connect onboarding → `PaymentDisbursement` → `IDisbursementCalculator` → webhook Stripe → UI de payouts. | Sprint 0, patrón de webhook de Sprint 2 | Sprint 4 (split de comisión de Agente reutiliza `IDisbursementCalculator`) |
| **Sprint 4** | **Épica 4** (Portal de Agencias): atribución de carga, invitaciones, dashboard, extensión de `IDisbursementCalculator` con `AgentCommissionAmountUsd`. | Sprint 1 (Agent), Sprint 3 (`PaymentDisbursement`) | — |
| **Sprint 5** | **Épica 5** (Pricing dinámico): `LaneRateStatistic`, job nocturno, extensión del `RateEngine`, UI de tarifa sugerida. | Requiere volumen mínimo de `Loads.Delivered` histórico — puede iniciar en paralelo desde Sprint 2 en cuanto a *código*, pero el job necesita datos reales antes de tener valor en producción. | — |

**Regla de no-bloqueo para QA/DevOps:** cada sprint entrega su propio archivo `.feature` con `@ignore` removido solo cuando el escenario pasa en CI (mismo patrón que `US-02-RATE-CALCULATOR.md`), y cada PR de esquema (Sprint 0) debe incluir *rollback script* probado en un entorno efímero de Container Apps antes de mergear a `main`.

### Requisitos no funcionales críticos por sprint

| Sprint | NFR | Verificación |
|---|---|---|
| 0 | Migraciones reversibles, sin downtime (`ALTER TABLE ... ADD COLUMN NULL`, nunca `NOT NULL` sin default en tablas con datos). | `dotnet ef migrations script` revisado en PR. |
| 2 | Webhook ELD soporta ≥ 50 req/s por Container App replica sin degradar `POST /api/rates/calculate` (aislamiento de recursos — considerar *scale rule* independiente por revisión). | Load test con `mcp_azure_mcp_ser_loadtesting` / Azure Load Testing. |
| 3 | Ningún log de aplicación contiene números de cuenta bancaria, tokens Stripe completos o `WebhookSecretHash` en claro. | Revisión de `ILogger` scopes + regla de `security-reviewer`. |
| 4 | Autorización por recurso (un Agent no lee datos de otro) verificada con tests de integración, no solo unit tests de handler. | `CPG.Api.IntegrationTests`. |
| 5 | `POST /api/rates/calculate` mantiene p95 < 500 ms tras añadir `HistoricalMarginAdjustmentHandler`. | Reutilizar el benchmark existente de US-02 (`RateEngineTests`). |

---

## Resumen de nuevas dependencias de paquete (a añadir en `Directory.Packages.props`)

| Paquete | Uso |
|---|---|
| `Stripe.net` | Stripe Connect, verificación de webhooks (`EventUtility.ConstructEvent`). |
| `Microsoft.Azure.SignalR` | Realtime GPS hacia el Shipper (modo serverless, sin gestionar WebSockets en la propia Container App). |
| (Frontend) `@microsoft/signalr` | Cliente SignalR en React. |
| (Frontend) `maplibre-gl` o `react-leaflet` | Mapa interactivo sin licenciamiento de Google Maps. |
