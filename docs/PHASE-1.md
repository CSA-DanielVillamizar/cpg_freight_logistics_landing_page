# Phase 1 — Análisis y Estructuración · Reporte de cierre

Estado: **completado**. Esqueleto arquitectónico compilando limpio, dependencias en contenedores
operativas, migración inicial aplicada. Cero lógica de negocio (solo contratos, tipos, middlewares
FDE y rieles de infraestructura), según lo acordado.

## Condición de éxito — verificación

| Check | Resultado |
| --- | --- |
| `dotnet build backend/CPG.sln` | ✅ 0 warnings / 0 errors (`TreatWarningsAsErrors`, `<Nullable>enable</Nullable>`) |
| `dotnet test backend/CPG.sln` | ✅ 11 passed, 5 skipped (`@ignore` BDD hasta cada slice) |
| `npm run build` (`frontend/`) | ✅ `tsc --noEmit` + Vite build OK |
| `npm run lint` (`frontend/`) | ✅ 0 problemas (`no-explicit-any: error`) |
| `docker compose up -d` | ✅ postgres (healthy) · rabbitmq (healthy) · azurite · jaeger |
| `dotnet ef database update` | ✅ `InitialSchema` aplicada — 9 tablas |
| API boot + probes | ✅ `/health` 200 · `/swagger` 200 · RBAC 401/403 · idempotencia · rate stub 501 |

## Decisiones de arranque (confirmadas por el usuario)

1. **Fuente de verdad UI**: densidad funcional de los PNG desktop, adaptada mobile-first.
2. **Layout repo**: split `backend/` + `frontend/`; Clean Architecture dentro de `backend/src`.
3. **Blob storage**: `IBlobStorage` en Application; `AzureBlobStorageService` (Azurite en dev /
   Azure Blob en prod) + `LocalFileSystemBlobStorage` de respaldo.
4. **Alcance**: Fase 1 completa, revisión antes de implementar US.

## Estructura entregada

### Backend (`backend/`)
- `Directory.Build.props` — net8.0, `Nullable=enable`, `TreatWarningsAsErrors`, `AnalysisLevel=latest-minimum`.
- `Directory.Packages.props` — Central Package Management.
- **CPG.Domain** — `Entity` / `AggregateRoot` / `IAuditableEntity` / `IHasRowVersion`, `DomainEvent`,
  `DomainException`; enums `UserRole`, `ServiceType`, `LeadStatus`, `ComplianceStatus`,
  `ComplianceDocumentType`, `LoadStatus`; entidades `User`, `RefreshToken`, `Carrier`,
  `ComplianceDocument`, `Lead`, `Load`, `AuditLogEntry`.
- **CPG.Application** — `AddApplication()`; pipeline MediatR: `UnhandledException` → `Logging` →
  `Validation` (FluentValidation) → `Performance` (presupuesto 500 ms US-02). Puertos:
  `IApplicationDbContext`, `IBlobStorage`, `IEventBus`, `IJwtTokenService`, `ICurrentUser`,
  `IDateTimeProvider`, `IIdempotencyService`, `IRateCalculator`. `Result`/`Result<T>`. Excepciones
  `ValidationException` / `NotFoundException` / `ForbiddenAccessException`. Eventos de integración
  `LeadCreatedIntegrationEvent`, `ComplianceDocumentUploadedIntegrationEvent`. DTOs de tarificación.
- **CPG.Infrastructure** — `AddInfrastructure(config)`: `ApplicationDbContext` (Npgsql),
  configuraciones EF con `xmin` como token de concurrencia, `AuditableEntityInterceptor`,
  `IdempotencyService`, `IDesignTimeDbContextFactory`; `AzureBlobStorageService` +
  `LocalFileSystemBlobStorage`; `MassTransitEventBus` (RabbitMQ); `JwtTokenService`;
  `DateTimeProvider`. Migración `Persistence/Migrations/InitialSchema`.
- **CPG.Api** — Serilog; `AddApplication` + `AddInfrastructure`; controllers stub
  (`Auth`, `Admin`, `Rates`, `Compliance`, `Leads`, `Loads`) con atributos RBAC y contratos
  `ProducesResponseType`; `GlobalExceptionHandler` (ProblemDetails, 403 "Access denied");
  `IdempotencyKeyMiddleware` + `[RequireIdempotencyKey]`; `HttpContextCurrentUser`;
  políticas `AdminOnly`/`CarrierOnly`/`ShipperOnly`; JWT bearer; OpenTelemetry (`AddCpgObservability`);
  Swagger con esquema Bearer; health checks (`/health`, `/health/ready`); CORS dev.
- **tests/** — Reqnroll + Testcontainers (`ContainerEnvironment`, `CpgApiFactory`); 4 `.feature`
  transcritos verbatim de `SPEC.md` §3 (con `@ignore`); pruebas unitarias de dominio y de
  composición de la capa de aplicación; `ScaffoldSanityTests`.

### Frontend (`frontend/`)
- Vite + React 18 + TS **strict** (`noUncheckedIndexedAccess`, `exactOptionalPropertyTypes`,
  `noImplicitAny`), ESLint `no-explicit-any: error`.
- `tailwind.config.ts` sincronizado con el design system *Industrial Fleet & Logistics*
  (Chivo / Inter / JetBrains Mono; navy `#0B192C`, hazard-orange `#EA580C`, safety-amber).
- `shared/ui` — `Button`, `Card`, `Badge`, `Input`. `shared/api` — cliente HTTP tipado con
  soporte `Idempotency-Key` + tipos espejo de los DTOs. `shared/lib/cn`.
- `features/landing` (home + páginas verticales US-04), `features/rates` (calculadora conectada
  al contrato, sin lógica de pricing), `features/leads` (`LeadCaptureForm`).

### Infraestructura local
- `docker-compose.yml`: PostgreSQL 16, RabbitMQ 3.13 (+ mgmt UI :15672), Azurite (:10000-2),
  Jaeger all-in-one (UI :16686, OTLP :4317/:4318).
- `.env.example`, `.editorconfig`, `.gitignore`, `global.json`.

## Desviaciones respecto al plan (todas menores, documentadas)

1. **`Microsoft.Extensions.*` 9.0.0** en lugar de 8.0.x: el ecosistema OpenTelemetry actual las
   arrastra transitivamente. Son compatibles con `net8.0`. EF Core / ASP.NET permanecen en 8.0.x.
2. **Exportador OTLP diferido**: todas las versiones publicadas de
   `OpenTelemetry.Exporter.OpenTelemetryProtocol` tienen la advisory `GHSA-4625-4j76-fww9`
   (moderate) y `NU1902` es error. Se usa el exportador de consola; la propagación `traceparent`
   —el requisito real de `SPEC.md` §2— no depende del exportador. Jaeger ya está en compose para
   cuando se reactive.
3. **`AnalysisLevel=latest-minimum`**: mantiene analizadores de correctitud + nullable como
   error, sin convertir cada sugerencia de estilo/perf en romper el build sobre andamiaje.

## Qué desbloquea cada fase siguiente

| Slice | Quita `@ignore` de | Implementa |
| --- | --- | --- |
| US-01 | `Authentication.feature` | `AuthController` login/refresh, hashing BCrypt, `JwtTokenService` en uso, seed de usuarios, 403 "Access denied" verificado |
| US-02 | `RateCalculation.feature` | `IRateCalculator` (matriz de lanes en memoria, <500 ms), handler CQRS + validator, `RatesController`, formulario React funcional |
| US-03 | `CarrierCompliance.feature` | `ComplianceController` upload → `IBlobStorage`, transición `PendingCompliance → UnderReview`, `AuditLogEntry` en PostgreSQL, evento RabbitMQ |
| US-04 | `LeadGeneration.feature` | `LeadsController` + handler + validator, persistencia status `New`, publicación `LeadCreatedIntegrationEvent`, páginas verticales conectadas |
| Load board | (nueva feature) | `LoadsController` create con idempotencia real end-to-end + concurrencia optimista en asignación |
| Observabilidad | — | Reactivar exportador OTLP → Jaeger cuando haya versión sin advisory |
