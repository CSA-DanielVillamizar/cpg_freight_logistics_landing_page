#!/usr/bin/env bash
# ---------------------------------------------------------------------------------------
# Pre-flight (what-if) and deploy the CPG Enterprises production infrastructure to Azure.
#
#   ./deploy.sh                 # context + lint + what-if only (safe default)
#   ./deploy.sh --deploy        # ... then apply
#   CPG_PG_ADMIN_PASSWORD=... CPG_JWT_SIGNING_KEY=... ./deploy.sh --deploy
# ---------------------------------------------------------------------------------------
set -euo pipefail

LOCATION="${LOCATION:-centralus}"
SUBSCRIPTION_ID="${SUBSCRIPTION_ID:-}"
DEPLOY=false
SKIP_WHATIF=false

for arg in "$@"; do
  case "$arg" in
    --deploy) DEPLOY=true ;;
    --skip-whatif) SKIP_WHATIF=true ;;
    --location=*) LOCATION="${arg#*=}" ;;
    --subscription=*) SUBSCRIPTION_ID="${arg#*=}" ;;
    *) echo "unknown arg: $arg" >&2; exit 2 ;;
  esac
done

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEMPLATE="$HERE/main.bicep"
PARAMETERS="$HERE/main.parameters.json"

rand_secret() { head -c "${1:-32}" /dev/urandom | base64 | tr -d '\n'; }

# --- 1. Azure context ---------------------------------------------------------------
[[ -n "$SUBSCRIPTION_ID" ]] && az account set --subscription "$SUBSCRIPTION_ID" >/dev/null
CTX_NAME="$(az account show --query name -o tsv)"
CTX_ID="$(az account show --query id -o tsv)"
CTX_TENANT="$(az account show --query tenantId -o tsv)"

echo '--------------------------------------------------------------------------'
echo " Subscription : ${CTX_NAME}"
echo " Subscription : ${CTX_ID}"
echo " Tenant       : ${CTX_TENANT}"
echo " Location     : ${LOCATION}"
echo '--------------------------------------------------------------------------'

# --- 2. Secrets -------------------------------------------------------------------
PG_PASSWORD="${CPG_PG_ADMIN_PASSWORD:-}"
if [[ -z "$PG_PASSWORD" ]]; then
  PG_PASSWORD="$(rand_secret 18)Aa1!"
  echo "WARNING: generated PostgreSQL admin password (store it now; also in Key Vault):" >&2
  echo "  $PG_PASSWORD" >&2
fi

JWT_KEY="${CPG_JWT_SIGNING_KEY:-}"
if [[ -z "$JWT_KEY" ]]; then
  JWT_KEY="$(rand_secret 48)"
  echo "WARNING: generated JWT signing key (also in Key Vault):" >&2
  echo "  $JWT_KEY" >&2
fi

SECURE_PARAMS=( "postgresAdministratorPassword=$PG_PASSWORD" "jwtSigningKey=$JWT_KEY" )

# --- 3. Lint --------------------------------------------------------------------
echo
echo '> az bicep build (lint)'
az bicep build --file "$TEMPLATE" --stdout >/dev/null
echo '  bicep build: OK'

# --- 4. What-if pre-flight ----------------------------------------------------
if [[ "$SKIP_WHATIF" != "true" ]]; then
  echo
  echo '> az deployment sub what-if'
  az deployment sub what-if \
    --location "$LOCATION" \
    --template-file "$TEMPLATE" \
    --parameters "$PARAMETERS" \
    --parameters "${SECURE_PARAMS[@]}"
fi

# --- 5. Deploy ---------------------------------------------------------------
if [[ "$DEPLOY" == "true" ]]; then
  NAME="cpg-infra-$(date +%Y%m%d-%H%M%S)"
  echo
  echo "> az deployment sub create ($NAME)"
  az deployment sub create \
    --name "$NAME" \
    --location "$LOCATION" \
    --template-file "$TEMPLATE" \
    --parameters "$PARAMETERS" \
    --parameters "${SECURE_PARAMS[@]}" \
    --query 'properties.outputs' \
    --output json
  echo
  echo 'Deployment complete.'
else
  echo
  echo 'Pre-flight only. Re-run with --deploy to apply.'
fi
