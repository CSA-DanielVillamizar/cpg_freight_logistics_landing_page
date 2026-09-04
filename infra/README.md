# CPG Enterprises — Production Infrastructure (Azure, Bicep)

Infrastructure-as-Code that provisions the CPG Enterprises Logistics Platform in **Azure,
region `centralus`**, optimised for cost (scale-to-zero, burstable DB) and zero-trust
(managed identity + Key Vault everywhere).

```
infra/
├── main.bicep                 # subscription-scoped orchestrator (creates the RG + wires modules)
├── main.parameters.json       # non-secret parameters
├── deploy.ps1 / deploy.sh     # context → lint → what-if → (optional) deploy
└── modules/
    ├── identity.bicep         # 1 user-assigned managed identity, shared by both apps
    ├── keyvault.bicep         # Key Vault (RBAC) + jwt-signing-key secret + Secrets User grant
    ├── storage.bicep          # Storage (shared-key OFF) + compliance-documents container + lifecycle + Blob Data Contributor grant
    ├── messaging.bicep        # Service Bus Standard + SAS→Key Vault + Service Bus Data Owner grant
    ├── db.bicep               # PostgreSQL Flexible Server (Burstable B1ms) + DB + connection-string→Key Vault
    └── apps.bicep             # Log Analytics + App Insights + ACR (Basic) + ACA env + api/web Container Apps
```

## Resources & naming (CAF `<type>-cpgorlando-prd-cus-01`)

| Resource | Name | SKU / tier | FinOps / security note |
| --- | --- | --- | --- |
| Resource Group | `rg-cpgorlando-prd-cus-01` | — | all resources tagged `Project=CPGOrlando`, `Environment=Production`, `CostCenter=LogisticsPlatform` |
| Managed Identity | `id-cpgorlando-prd-cus-01` | user-assigned | one workload identity for API + web |
| Key Vault | `kv-cpgorlando-prd-cus-01` | Standard, **RBAC** | soft-delete 90d; purge-protection off (greenfield) |
| Storage Account | `stcpgorlandoprdcus01` | Standard_LRS, StorageV2 | `allowSharedKeyAccess: false` → MI only; lifecycle: `compliance-documents` → **Cool @ 90d, Archive @ 180d** |
| Container Registry | `crcpgorlandoprdcus01` | **Basic** | admin user disabled; pull via `AcrPull` on the MI |
| Service Bus | `sb-cpgorlando-prd-cus-01` | **Standard** | replaces local RabbitMQ; SAS connection string in Key Vault **and** MI `Data Owner` for a passwordless path |
| PostgreSQL Flexible | `psql-cpgorlando-prd-cus-01` | **Burstable `Standard_B1ms`**, PG 16 | 32 GB auto-grow, backup 7d, no geo-redundancy, no HA; `require_secure_transport = ON` |
| Log Analytics | `log-cpgorlando-prd-cus-01` | PerGB2018, 30d | |
| Application Insights | `appi-cpgorlando-prd-cus-01` | workspace-based | |
| Container Apps env | `cae-cpgorlando-prd-cus-01` | **Consumption** | |
| Container App — API | `ca-cpgorlando-api-prd-cus-01` | 0.5 vCPU / 1 GiB | **minReplicas = 0** (scale-to-zero), max 10, HTTP scale @ 20 concurrent |
| Container App — Web | `ca-cpgorlando-web-prd-cus-01` | 0.25 vCPU / 0.5 GiB | **minReplicas = 0**, max 5 |

> Key Vault / Storage / ACR / Service Bus / PostgreSQL names are **globally scoped**. If a name
> is already taken, change `instance` (or `namePrefix`) in `main.parameters.json`.

## Identity & secrets flow (zero-trust)

```
                    ┌─────────────── Key Vault (RBAC) ───────────────┐
                    │  jwt-signing-key                               │
                    │  postgres-connection-string                    │
                    │  servicebus-connection-string                  │
                    └───────────────▲───────────────────────────────┘
                                    │ Key Vault reference (resolved with the MI)
   ┌── id-cpgorlando-prd-cus-01 ────┼──────────────────────────────────────────┐
   │  Key Vault Secrets User        │                                          │
   │  Storage Blob Data Contributor ─────────► Blob (compliance-documents)     │
   │  Azure Service Bus Data Owner  ─────────► Service Bus                     │
   │  AcrPull                       ─────────► Container Registry              │
   └───────────────────────────────────────────────────────────────────────────┘
```

No secret **value** is ever placed in a Container App environment variable — only Key Vault
references (`secretRef`) resolved at runtime by the managed identity.

## Environment variables injected into the Container Apps

Mapped from the current app config surface (`backend/src/CPG.Api/appsettings.json`, `.env.example`,
`frontend/.env.example`). ASP.NET Core reads `Section__Key` → `Section:Key`.

### API — `ca-cpgorlando-api-prd-cus-01`

| Variable | Source | Kind |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` | literal |
| `ASPNETCORE_HTTP_PORTS` | `8080` (= ingress `targetPort`) | literal |
| `ConnectionStrings__Postgres` | Key Vault `postgres-connection-string` | **secretRef** |
| `ConnectionStrings__ServiceBus` | Key Vault `servicebus-connection-string` | **secretRef** |
| `ServiceBus__FullyQualifiedNamespace` | `sb-cpgorlando-prd-cus-01.servicebus.windows.net` | literal (for the MI path) |
| `Jwt__SigningKey` | Key Vault `jwt-signing-key` | **secretRef** |
| `Jwt__Issuer` / `Jwt__Audience` | parameters | literal |
| `BlobStorage__Provider` | `AzureManagedIdentity` | literal |
| `BlobStorage__ServiceUri` | storage blob endpoint | literal |
| `AZURE_CLIENT_ID` | MI client id (selects the identity for `DefaultAzureCredential`) | literal |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | App Insights | literal |
| `Cors__AllowedOrigins` | `https://<web fqdn>` | literal |
| `AllowedHosts` | `*` | literal |

### Web — `ca-cpgorlando-web-prd-cus-01`

| Variable | Source |
| --- | --- |
| `API_BASE_URL` / `VITE_API_BASE_URL` | `https://<api fqdn>/api` |

## Application ↔ infrastructure alignment (done)

The four adaptations the app needed for this infrastructure are **implemented** (commit after
`c219ec5`). They are configuration-switched, so local `docker compose` + `dotnet test` keep
working unchanged:

1. **Messaging transport** — `CPG.Infrastructure/DependencyInjection.cs` picks
   `UsingAzureServiceBus` when `ServiceBus:FullyQualifiedNamespace` **or**
   `ConnectionStrings:ServiceBus` is set (managed identity via `AZURE_CLIENT_ID`, else the
   Key Vault connection string); otherwise `UsingRabbitMq` (local dev / tests). Package
   `MassTransit.Azure.ServiceBus.Core`.
2. **Blob via managed identity** — `AzureBlobStorageService` builds
   `new BlobServiceClient(new Uri(ServiceUri), new DefaultAzureCredential(...))` when
   `BlobStorage:Provider == "AzureManagedIdentity"`; connection-string mode otherwise. Package
   `Azure.Identity`.
3. **Dynamic CORS** — `Program.cs` reads `Cors:AllowedOrigins` (array or `,`/`;` list) and
   falls back to the local Vite origins; `UseCors` now runs in every environment.
4. **Frontend runtime config** — `index.html` loads `/config.js`; the web image's nginx
   entrypoint (`frontend/docker-entrypoint.sh`) regenerates it from `API_BASE_URL` /
   `VITE_API_BASE_URL` on every start; `src/shared/config/runtime.ts` resolves
   `window.__CPG_CONFIG__` → Vite build env → `/api`.

**Container images.** `backend/Dockerfile` (SDK build → chiseled `aspnet:8.0`, port 8080) and
`frontend/Dockerfile` (node build → `nginx:1.27-alpine`, port 80). `main.parameters.json` points
`apiContainerImage` / `webContainerImage` at a public placeholder so `what-if` runs; build and
push real images to `crcpgorlandoprdcus01.azurecr.io` and pass them as parameters:

```bash
az acr login -n crcpgorlandoprdcus01
docker build -t crcpgorlandoprdcus01.azurecr.io/cpg-api:$(git rev-parse --short HEAD) ./backend
docker build -t crcpgorlandoprdcus01.azurecr.io/cpg-web:$(git rev-parse --short HEAD) ./frontend
docker push crcpgorlandoprdcus01.azurecr.io/cpg-api:...
docker push crcpgorlandoprdcus01.azurecr.io/cpg-web:...
./infra/deploy.ps1 -Deploy   # pass apiContainerImage=... webContainerImage=... via main.parameters.json
```

## Usage

```powershell
az login
az account set --subscription "<sub>"

# 1. context + lint + what-if (no changes made)
./infra/deploy.ps1

# 2. apply (generates + prints the two secrets if not supplied)
$env:CPG_PG_ADMIN_PASSWORD = "<strong>"
$env:CPG_JWT_SIGNING_KEY   = "<48+ bytes base64>"
./infra/deploy.ps1 -Deploy
```

`deploy.sh` is the bash equivalent (`./infra/deploy.sh` / `./infra/deploy.sh --deploy`).

## Estimated monthly cost (idle, USD, list price — indicative)

| Component | Idle assumption | ~ USD/mo |
| --- | --- | --- |
| Container Apps (Consumption) | scale-to-zero, near-zero requests | **~$0** |
| PostgreSQL Burstable B1ms | 1 vCore burstable + 32 GB | ~$13 + ~$4 storage |
| Service Bus Standard | base charge | ~$10 |
| Storage Standard_LRS | a few GB, mostly Cool/Archive after 90/180d | < $1 |
| Log Analytics + App Insights | low ingest, 30d | ~$3–8 (usage-based) |
| Container Registry Basic | | ~$5 |
| Key Vault | operations-based | < $1 |
| **Total (idle)** | | **~$45–55/mo**, rising only with real dispatch traffic |
