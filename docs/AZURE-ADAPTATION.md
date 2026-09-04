# Azure adaptation — application code aligned with `infra/`

The four gaps flagged in `infra/README.md` are implemented. Every switch is
**configuration-driven**, so `docker compose` + `dotnet test` keep working unchanged and the
same binaries run on Azure Container Apps.

## 1. MassTransit — RabbitMQ (dev) ↔ Azure Service Bus (prod)

`backend/src/CPG.Infrastructure/DependencyInjection.cs · AddMessaging`

| Condition | Transport |
| --- | --- |
| `ServiceBus:FullyQualifiedNamespace` **or** `ConnectionStrings:ServiceBus` present | `UsingAzureServiceBus` |
| neither | `UsingRabbitMq` (local dev / integration tests) |

- **Managed identity path** (`ServiceBus:FullyQualifiedNamespace` set): `cfg.Host("sb://<ns>", h => h.TokenCredential = new DefaultAzureCredential(...))`. `AZURE_CLIENT_ID` selects the user-assigned identity.
- **Connection-string path**: `cfg.Host(connectionString)` from the Key Vault secret `servicebus-connection-string`.
- Package added: `MassTransit.Azure.ServiceBus.Core` 8.3.0.
- The two consumers (`ComplianceNotificationConsumer`, `LeadNotificationConsumer`) are unchanged — MassTransit maps them onto Service Bus topics/subscriptions automatically.

## 2. Blob Storage — connection string (dev) ↔ managed identity (prod)

`backend/src/CPG.Infrastructure/Storage/AzureBlobStorageService.cs`

| `BlobStorage:Provider` | Client |
| --- | --- |
| `Azure` | `new BlobServiceClient(connectionString)` (Azurite / account key) |
| `AzureManagedIdentity` | `new BlobServiceClient(new Uri(ServiceUri), new DefaultAzureCredential(...))` — **no secret** |
| `Local` | `LocalFileSystemBlobStorage` |

- New `BlobStorageOptions` members: `ServiceUri`, `ManagedIdentityClientId`.
- Package added: `Azure.Identity` 1.13.2.

## 3. Dynamic CORS

`backend/src/CPG.Api/Program.cs`

- Origins come from `Cors:AllowedOrigins` — a JSON array (`Cors__AllowedOrigins__0`, …) **or** a
  `,` / `;`-separated string. Empty → falls back to `http://localhost:5173` / `:4173`.
- `app.UseCors("cpg-frontend")` now runs in **every** environment (was Development-only), placed
  before authentication.
- `appsettings.json` gains `"Cors": { "AllowedOrigins": [] }`.

## 4. Frontend runtime configuration

Vite bakes `import.meta.env.*` at build time, so a single image is parameterised at **container
start** instead:

- `frontend/index.html` loads `<script src="/config.js">` before the app bundle.
- `frontend/public/config.js` — dev/default stub (`apiBaseUrl: ''`).
- `frontend/src/shared/config/runtime.ts · resolveApiBaseUrl()` — precedence
  `window.__CPG_CONFIG__.apiBaseUrl` → `import.meta.env.VITE_API_BASE_URL` → `/api`.
- `frontend/docker-entrypoint.sh` — nginx `/docker-entrypoint.d/` hook; rewrites `/config.js`
  from `API_BASE_URL` / `VITE_API_BASE_URL` on every start.
- `src/shared/api/client.ts` now calls `resolveApiBaseUrl()`.

## Container images

| Image | Dockerfile | Base | Port |
| --- | --- | --- | --- |
| `cpg-api` | `backend/Dockerfile` | `dotnet/sdk:8.0` → `dotnet/aspnet:8.0-noble-chiseled` (non-root) | 8080 |
| `cpg-web` | `frontend/Dockerfile` | `node:20-alpine` → `nginx:1.27-alpine` | 80 |

`frontend/nginx.conf` adds SPA fallback + `no-store` on `config.js` / `index.html` +
immutable caching for `/assets/`.

## Verification (this change)

| Gate | Result |
| --- | --- |
| `dotnet build backend/CPG.sln` | 0 warnings / 0 errors |
| `dotnet test` — Domain + Application unit tests | 16 passed |
| `npm run build` / `lint` / `tsc --noEmit` (frontend) | clean |
| `az bicep build infra/main.bicep` | 0 warnings / 0 errors |

Integration (BDD) suite paths are unchanged — the RabbitMQ transport and connection-string blob
client are selected whenever the Azure keys are absent, which is exactly the test configuration.
