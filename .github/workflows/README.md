# CI/CD — Azure Container Apps

| Workflow | Trigger | Builds | Deploys to |
|---|---|---|---|
| `deploy-api.yml` | push to `main` under `backend/**` (or manual) | `crcpgorlandoprdcus01.azurecr.io/cpg-api` | `ca-cpgorlando-api-prd-cus-01` |
| `deploy-web.yml` | push to `main` under `frontend/**` (or manual) | `crcpgorlandoprdcus01.azurecr.io/cpg-web` | `ca-cpgorlando-web-prd-cus-01` |

Both run in resource group `rg-cpgorlando-prd-cus-01`. Images are tagged with the
commit SHA (`github.sha`) **and** `latest`; the Container App is pointed at the
immutable SHA tag so every run produces a new revision.

## Required repository secret: `AZURE_CREDENTIALS`

Create a service principal scoped to the production resource group and store its
JSON output as the `AZURE_CREDENTIALS` repository secret
(`Settings → Secrets and variables → Actions → New repository secret`):

```bash
az ad sp create-for-rbac \
  --name "gha-cpgorlando-deployer" \
  --role "Contributor" \
  --scopes "/subscriptions/2f5d85ee-0256-4e8e-9e1a-2c7c87563cb8/resourceGroups/rg-cpgorlando-prd-cus-01" \
  --sdk-auth
```

The `--sdk-auth` JSON (`clientId` / `clientSecret` / `subscriptionId` / `tenantId` …)
is the exact value to paste into the secret. `azure/login@v2` consumes it directly.

`Contributor` on the resource group is enough for `az acr build` (push to ACR) and
`az containerapp update`. To tighten later, replace it with `AcrPush` on the
registry + `Contributor` on each Container App.
