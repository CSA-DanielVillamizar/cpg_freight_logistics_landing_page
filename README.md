# CPG Enterprises Logistics Platform

Digital platform for **CPG Enterprises of Orlando, Inc.** — heavy haul, flatbed, cold chain and
FDOT concrete logistics. Built to `SPEC.md` (Clean Architecture · .NET 8 · React · PostgreSQL ·
Spec-Driven Development · BDD).

## Repository layout

```
.
├── SPEC.md                       # Master FDE & SSD execution specification
├── docker-compose.yml            # PostgreSQL 16 · RabbitMQ · Azurite · Jaeger
├── backend/                      # .NET 8 solution (Clean Architecture)
│   ├── src/
│   │   ├── CPG.Domain/           # Entities, value objects, enums, domain exceptions
│   │   ├── CPG.Application/      # CQRS (MediatR), FluentValidation, pipeline behaviours, ports
│   │   ├── CPG.Infrastructure/   # EF Core + PostgreSQL, MassTransit/RabbitMQ, blob storage, JWT
│   │   └── CPG.Api/              # ASP.NET Core Web API, RBAC, OpenAPI, FDE middleware
│   └── tests/
│       ├── CPG.Domain.UnitTests/
│       ├── CPG.Application.UnitTests/
│       └── CPG.Api.IntegrationTests/   # Reqnroll (Gherkin) + Testcontainers
├── frontend/                     # React 18 + Vite + TypeScript (strict) + Tailwind
│   └── src/
│       ├── app/                  # Router
│       ├── features/             # Feature-first: landing, rates, leads
│       └── shared/               # ui/ (design-system primitives), api/ (typed client), lib/
└── cpg_freight_logistics_landing_page/   # Design system + vertical page prototypes
```

## Prerequisites

- .NET SDK 8.0.x (pinned in `global.json`)
- Node.js 20+ / npm
- Docker (for the dependency stack)

## Getting started

```bash
# 1. Bring up dependencies (PostgreSQL, RabbitMQ, Azurite, Jaeger)
docker compose up -d

# 2. Backend
cd backend
dotnet build
dotnet tool install --global dotnet-ef --version 8.*   # once
dotnet ef database update --project src/CPG.Infrastructure --startup-project src/CPG.Api
dotnet run --project src/CPG.Api --urls http://localhost:5080
#   Swagger UI:  http://localhost:5080/swagger
#   Health:      http://localhost:5080/health

# 3. Frontend (separate terminal)
cd frontend
npm install
npm run dev      # http://localhost:5173  (proxies /api -> http://localhost:5080)
```

## Quality gates

| Command | Scope |
| --- | --- |
| `dotnet build backend/CPG.sln` | Strict nullable + `TreatWarningsAsErrors` (SSD) |
| `dotnet test backend/CPG.sln` | Unit tests + Reqnroll BDD suite (`@ignore` until each slice lands) |
| `npm run build` (in `frontend/`) | `tsc --noEmit` + Vite production build |
| `npm run lint` (in `frontend/`) | ESLint, `@typescript-eslint/no-explicit-any: error` |

## Seed users (non-production)

`ApplicationDbContextInitialiser` seeds three RBAC accounts on startup — password `Passw0rd!`:

| Email | Role |
| --- | --- |
| `admin@cpgorlando.com` | Admin |
| `carrier@cpgorlando.com` | Carrier |
| `shipper@cpgorlando.com` | Shipper |

## Slice status

| Slice | State | Doc |
| --- | --- | --- |
| Phase 1 — scaffold + FDE rails | ✅ done | `docs/PHASE-1.md` |
| US-01 — RBAC & authentication | ✅ done | `docs/US-01-RBAC.md` |
| US-02 — rate calculator | ✅ done | `docs/US-02-RATE-CALCULATOR.md` |
| US-03 — carrier compliance | ☐ pending | — |
| US-04 — landing-page leads | ☐ pending | — |

## FDE rails already wired (Phase 1)

- **Idempotency** — `IdempotencyKeyMiddleware` enforces `Idempotency-Key: <UUID>` on endpoints
  marked `[RequireIdempotencyKey]` and replays stored responses (`idempotency_records` table).
- **Optimistic concurrency** — `Carrier` and `Load` map their concurrency token onto the
  PostgreSQL system column `xmin` (no user column emitted).
- **Distributed tracing** — OpenTelemetry tracing with W3C `traceparent` propagation
  (ASP.NET Core + `HttpClient`). OTLP export to Jaeger is added in a later phase once a
  non-vulnerable exporter package aligns with the .NET 8 stack.
- **RBAC** — JWT bearer auth + `AdminOnly` / `CarrierOnly` / `ShipperOnly` policies; the global
  exception handler returns `403` with `"Access denied"` (SPEC.md US-01).
- **Auditing** — `AuditableEntityInterceptor` stamps create/update columns; `audit_log` table
  ready for US-03.

See `docs/PHASE-1.md` for the full status report and what each later phase unblocks.
