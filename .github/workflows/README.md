# CI/CD — Azure Container Apps

| Workflow | Trigger | Builds | Deploys to |
|---|---|---|---|
| `deploy-api.yml` | push to `main` under `backend/**` (or manual) | `crcpgorlandoprdcus01.azurecr.io/cpg-api` | `ca-cpgorlando-api-prd-cus-01` |
| `deploy-web.yml` | push to `main` under `frontend/**` (or manual) | `crcpgorlandoprdcus01.azurecr.io/cpg-web` | `ca-cpgorlando-web-prd-cus-01` |

Both run in resource group `rg-cpgorlando-prd-cus-01`. Images are tagged with the
commit SHA (`github.sha`) **and** `latest`; the Container App is pointed at the
immutable SHA tag so every run produces a new revision.

## Authentication — OIDC, no stored secret

`azure/login@v2` logs in with a short-lived GitHub OIDC token. Azure trusts it via
a **federated credential** on the service principal `cpg-github-actions-sp`
(`Contributor` on `rg-cpgorlando-prd-cus-01`). The workflows carry only non-secret
identifiers in `env:` — `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`,
`AZURE_SUBSCRIPTION_ID`. There is nothing to rotate and no `AZURE_CREDENTIALS`
secret.

### The federated credential (one-time, already created)

```bash
az ad app federated-credential create \
  --id fcdbbfab-5dcf-407a-800e-67360eb4ad1f \
  --parameters '{
    "name": "github-actions-main",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:CSA-DanielVillamizar/cpg_freight_logistics_landing_page:ref:refs/heads/main",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

The `subject` pins trust to this repo on the `main` branch. Workflows that run from
any other ref (a tag, a PR, another branch) will not authenticate — add another
federated credential with the matching subject if that is ever needed.

### If the service principal is ever recreated

Re-run the `federated-credential create` above against the new app id and update
`AZURE_CLIENT_ID` in both workflows. The SP still needs `Contributor` on the RG:

```bash
az ad sp create-for-rbac --name "cpg-github-actions-sp" --role Contributor \
  --scopes "/subscriptions/2f5d85ee-0256-4e8e-9e1a-2c7c87563cb8/resourceGroups/rg-cpgorlando-prd-cus-01"
```
