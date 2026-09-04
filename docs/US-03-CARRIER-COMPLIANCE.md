# US-03 — Carrier Document Compliance & Verification · Reporte de cierre

Estado: **100% en verde**. Pasarela segura de carga de documentos con Blob Storage (Azurite),
auditoría transaccional en PostgreSQL y mensajería asíncrona (RabbitMQ/MassTransit).

## Condición de éxito — verificación

| Check | Resultado |
| --- | --- |
| `dotnet build backend/CPG.sln` | ✅ 0 warnings / 0 errors |
| `dotnet test backend/CPG.sln` | ✅ **US-03 en verde** — `Successfully uploading a Certificate of Insurance (COI)` passed [11 s] |
| `npm run build` / `npm run lint` / `tsc --noEmit` | ✅ los tres limpios |
| El archivo se guarda en el contenedor de Azurite | ✅ verificado (BDD descarga el blob de Azurite efímero y valida el tamaño) |
| El log de auditoría se persiste en PostgreSQL | ✅ `ComplianceDocumentUploaded` con `UserId` + `TimestampUtc` (BDD lo consulta) |
| Estado del transportista → `Under Review` | ✅ dentro de la misma transacción que el audit log |
| Evento publicado en RabbitMQ y consumido | ✅ el consumer escribe `CommercialTeamNotified`; el BDD hace *poll* hasta verlo |
| Validación de archivo (tipo/tamaño) → `400` | ✅ `.txt` → 400 (FileName+ContentType); 6 MB → 400 |
| RBAC: rol incorrecto → 403 "Access denied"; anónimo → 401 | ✅ |
| Frontend gestiona la carga (drag&drop, progreso, toast, refresh) | ✅ verificado en navegador |

## Backend

### Dominio
- `Carrier` es ahora un agregado con comportamiento: `SubmitComplianceDocument(...)` añade el
  `ComplianceDocument`, transiciona `ComplianceStatus → UnderReview` y **levanta**
  `ComplianceDocumentUploadedDomainEvent`. `ComplianceDocuments` pasó a colección de solo lectura
  con backing field (`_complianceDocuments`).

### CQRS (`Features/Compliance/`)
- `UploadComplianceDocumentCommand` (+ validator + handler) — **el `CarrierId` se resuelve de
  `ICurrentUser.UserId`, nunca del payload**. Validación estricta: extensión ∈ {.pdf,.jpg,.jpeg},
  MIME ∈ {application/pdf, image/jpeg}, tamaño 1 B … **5 MB**.
- `GetComplianceStatusQuery` — snapshot del transportista para el portal.
- `ComplianceDocumentUploadedDomainEventHandler` — puentea el evento de dominio (post-commit) a
  `ComplianceDocumentUploadedIntegrationEvent` vía `IEventBus` (MassTransit).

### Handler — flujo transaccional (FDE)
1. **Blob primero**: sube a `IBlobStorage` (`compliance-documents/{carrierId}/{guid}{ext}`).
2. **Una transacción** (`SaveChangesAsync`): `carrier.SubmitComplianceDocument(...)` +
   `ComplianceDocuments.Add(document)` + `AuditLogEntry` (`Action`, `EntityId`, `UserId`,
   `TimestampUtc`, `TraceId`, `DataJson` con metadatos del archivo).
3. Si la transacción falla → **best-effort delete** del blob y re-throw.
4. Tras el commit, `DispatchDomainEventsInterceptor` (en `SavedChangesAsync`) publica el evento
   de dominio por MediatR → RabbitMQ.

### Infraestructura
- `DispatchDomainEventsInterceptor : SaveChangesInterceptor` — publica los domain events de los
  agregados **después** de que la transacción commitea.
- `ComplianceNotificationConsumer : IConsumer<ComplianceDocumentUploadedIntegrationEvent>` —
  consume de RabbitMQ, registra `CommercialTeamNotified` en `audit_log` (hace observable el
  camino publish → broker → consume).
- `AzureBlobStorageService` conectado a **Azurite** (`UseDevelopmentStorage=true` en dev, connection
  string del contenedor en tests). Crea el contenedor `compliance-documents` on-demand.
- Seed: `ApplicationDbContextInitialiser` ahora crea un `Carrier` ligado a `carrier@cpgorlando.com`.

### API
- `POST /api/compliance/upload` (`multipart/form-data`: `file` + `documentType`), política
  `CarrierOnly`, `RequestSizeLimit`/`RequestFormLimits` = 5 MB + 256 KB → 202 `Accepted` con
  `{ carrierId, documentId, status, blobUri }`.
- `GET /api/compliance` → snapshot (`CompanyName`, `Status`, `Documents[]`).

## Frontend (`src/features/carrier-portal/`)
- `complianceApi.ts` — `validateFile` (tipo + 5 MB en cliente para ahorrar ancho de banda),
  `upload()` con **`XMLHttpRequest`** para eventos de progreso (fetch no los tiene) e inyección
  del `Authorization: Bearer` vía `currentAccessToken()`.
- `useComplianceUpload.ts` — máquina de estados `idle→uploading→done|error`, toasts (`sonner`),
  refetch del status tras éxito.
- `ComplianceDropzone.tsx` — drag & drop + selector de tipo de documento + barra de progreso.
- `CarrierPortalPage.tsx` — badge de estado del transportista, lista de documentos filtrados,
  actualización visual a "Under Review" al terminar. Ruta `/carrier` protegida con
  `<RequireRole role="Carrier">`; link en el header.
- `sonner` `<Toaster />` montado en `main.tsx`. Material Symbols añadido a `index.html`.

## BDD (`Features/CarrierCompliance.feature` — `@ignore` retirado)
`StepDefinitions/CarrierComplianceStepDefinitions.cs` + `Support/TestScope.cs` (acceso al
`IServiceProvider` del host de test, helper `EventuallyAsync` para *polling*):
- **Given**: login como carrier seed, y resetea el carrier a `PendingCompliance` + limpia sus
  documentos vía SQL directo (determinista).
- **When**: construye un PDF de ~2.4 MB, lo sube por `multipart/form-data`, exige `202` y captura
  `documentId` + `blobUri`.
- **Then**: descarga el blob de **Azurite** y valida > 2 MB; el `Carrier.ComplianceStatus` es
  `UnderReview`; existe `AuditLogEntry` (`ComplianceDocumentUploaded`, `UserId`, timestamp
  reciente); y *poll* hasta 20 s a que aparezca `CommercialTeamNotified` (evento consumido de
  **RabbitMQ** efímero).

Testcontainers por corrida: `postgres:16-alpine` + `rabbitmq:3.13-management-alpine` +
`azurite:3.33.0`.

## Suite completa
17 unit + integration passed, 1 skipped (US-04), 0 failed.

## Siguiente
- US-04 (Landing-page leads) — quita `@ignore` de `LeadGeneration.feature`.
