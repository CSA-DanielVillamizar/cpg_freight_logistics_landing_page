# CPG Enterprises Logistics Platform — Reporte de cierre del proyecto

Plataforma digital para **CPG Enterprises of Orlando, Inc.** construida de forma iterativa
sobre `SPEC.md` (Clean Architecture · .NET 8 · React · PostgreSQL · Spec-Driven Development · BDD).

## Estado final

| Fase / Slice | Estado | Doc |
| --- | --- | --- |
| Fase 1 — Esqueleto + rieles FDE | ✅ | `docs/PHASE-1.md` |
| US-01 — RBAC & autenticación | ✅ | `docs/US-01-RBAC.md` |
| US-02 — Calculadora de tarifas | ✅ | `docs/US-02-RATE-CALCULATOR.md` |
| US-03 — Cumplimiento de transportistas | ✅ | `docs/US-03-CARRIER-COMPLIANCE.md` |
| US-04 — Generación de leads | ✅ | `docs/US-04-LEAD-GENERATION.md` |

### Suite de pruebas

```
CPG.Domain.UnitTests        4 passed
CPG.Application.UnitTests   12 passed
CPG.Api.IntegrationTests   10 passed   (4 escenarios BDD + 6 sanity)
------------------------------------------------
TOTAL                      26 passed · 0 skipped · 0 failed
```

Los **4 escenarios Gherkin de `SPEC.md` §3** están implementados y en verde, sin `@ignore`:

1. `RBAC and Secure Authentication` → login 200 + JWT/refresh · Carrier→admin = **403 "Access denied"**
2. `Dynamic Rate Calculation` → Cold Chain Miami→Orlando 35000 lb −20 °C, **< 500 ms**, desglose base/cold-chain/fuel
3. `Carrier Document Compliance` → PDF 2.4 MB → **Azurite** · carrier → `Under Review` · audit en **PostgreSQL** con userId · evento **RabbitMQ** consumido
4. `Corporate Lead Generation` → `POST /api/leads` 200 · lead `New` en **PostgreSQL** · evento **RabbitMQ** consumido

### Quality gates

| Comando | Resultado |
| --- | --- |
| `dotnet build backend/CPG.sln` | ✅ 0 warnings / 0 errors (`TreatWarningsAsErrors`, `<Nullable>enable</Nullable>`) |
| `dotnet test backend/CPG.sln` | ✅ 26/0/0 |
| `cd frontend && npm run build` | ✅ `tsc --noEmit` + Vite |
| `cd frontend && npm run lint` | ✅ 0 problemas (`@typescript-eslint/no-explicit-any: error`) |
| `docker compose up -d` + `dotnet ef database update` | ✅ 4 migraciones aplicadas |

## Arquitectura entregada

### Backend — `backend/` (Clean Architecture, .NET 8)

| Capa | Contenido |
| --- | --- |
| **Domain** | Agregados `User`, `Carrier` (comportamiento: `SubmitComplianceDocument`), `Lead` (`RegisterFromLandingPage`), `Load`; `RefreshToken`, `ComplianceDocument`, `AuditLogEntry`; `AggregateRoot` + domain events; 6 enums; `IHasRowVersion` (xmin) |
| **Application** | CQRS (MediatR) + pipeline `UnhandledException → Logging → Validation → Performance`; features `Authentication`, `Rates` (motor Strategy + Chain of Responsibility), `Compliance`, `Leads`; puertos (`IApplicationDbContext`, `IBlobStorage`, `IEventBus`, `IJwtTokenService`, `IPasswordHasher`, `ICurrentUser`, `IDateTimeProvider`, `IIdempotencyService`); `Result<T>`; excepciones tipadas; eventos de integración |
| **Infrastructure** | EF Core + Npgsql; `xmin` como token de concurrencia optimista; `AuditableEntityInterceptor` + `DispatchDomainEventsInterceptor` (publica domain events post-commit); `AzureBlobStorageService` (Azurite/Azure) + `LocalFileSystemBlobStorage`; MassTransit/RabbitMQ + 2 consumers; `BCryptPasswordHasher`; `JwtTokenService`; `IdempotencyService`; seeding (3 usuarios RBAC + 1 carrier) |
| **Api** | Serilog; controllers `Auth`/`Rates`/`Compliance`/`Leads`/`Admin`/`Loads`; `GlobalExceptionHandler` (RFC 7807); `CpgAuthorizationResultHandler` (403 "Access denied"); `IdempotencyKeyMiddleware`; políticas `AdminOnly`/`CarrierOnly`/`ShipperOnly`; OpenTelemetry (`traceparent`); Swagger con Bearer; health checks; warm-up de startup |

### Rieles FDE (`SPEC.md` §2)
- **Idempotencia** — `Idempotency-Key` header + `IdempotencyKeyMiddleware` + tabla `idempotency_records`.
- **Concurrencia optimista** — `Carrier` y `Load` mapean su token a la columna de sistema `xmin` de PostgreSQL (sin columna de usuario).
- **Trazabilidad distribuida** — OpenTelemetry con propagación W3C `traceparent`; `traceId` en cada `AuditLogEntry` y `ProblemDetails`.
- **Patrón transaccional** — blob primero → una transacción EF (entidad + estado + audit) → best-effort compensación → evento a RabbitMQ post-commit.

### Frontend — `frontend/` (React 18 + Vite + TS estricto + Tailwind)
- Feature-first: `features/{auth,rates,carrier-portal,leads,landing,admin}`, `shared/{ui,api,lib}`.
- TS estricto: `exactOptionalPropertyTypes`, `noUncheckedIndexedAccess`, `noImplicitAny`; ESLint `no-explicit-any: error`.
- `tailwind.config.ts` sincronizado con el design system *Industrial Fleet & Logistics* (Chivo/Inter/JetBrains Mono; navy `#0B192C`, hazard-orange `#EA580C`, safety-amber).
- Cliente HTTP tipado: inyección de `Bearer`, flujo **401 → refresh → retry**, subida XHR con progreso.
- Auth store (Context API + `localStorage`), guards `RequireRole`, toasts `sonner`.
- **4 landing pages verticales** conectadas a `POST /api/leads`, portal de transportistas con dropzone, calculadora de tarifas, login.

### Infra local — `docker-compose.yml`
PostgreSQL 16 · RabbitMQ 3.13 · Azurite 3.33 · Jaeger. Testcontainers efímeros por corrida BDD.

## Desviaciones documentadas
1. `Microsoft.Extensions.*` 9.0.0 (arrastradas por OpenTelemetry; compatibles con net8.0).
2. Exportador OTLP diferido (advisory `GHSA-4625-4j76-fww9` + `NU1902` como error) → exportador de consola; `traceparent` no depende de él; Jaeger listo en compose.
3. `AnalysisLevel=latest-minimum` (correctitud + nullable como error, sin romper el build por estilo).
4. EF Core: entidad hija con clave `Guid` de cliente vía navegación → `dbContext.Set<T>().Add()` explícito.
5. Testcontainers en Docker Desktop → reintento de *bring-up* en `TestApp`.

## Repo
`https://github.com/CSA-DanielVillamizar/cpg_freight_logistics_landing_page.git` · rama `main`.

| Commit | Slice |
| --- | --- |
| `8962f64` | Fase 1 + US-01 |
| `1553457` | US-02 |
| `2bd1262` | US-03 |
| `<final>` | US-04 + cierre |

## Cómo arrancar

```bash
docker compose up -d
cd backend && dotnet ef database update --project src/CPG.Infrastructure --startup-project src/CPG.Api
dotnet run --project src/CPG.Api --urls http://localhost:5080     # Swagger: /swagger
cd ../frontend && npm install && npm run dev                       # http://localhost:5173
```

Cuentas seed (no producción, password `Passw0rd!`): `admin@` / `carrier@` / `shipper@cpgorlando.com`.
