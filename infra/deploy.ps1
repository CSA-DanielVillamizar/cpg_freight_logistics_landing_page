<#
.SYNOPSIS
    Pre-flight (what-if) and deploy the CPG Enterprises production infrastructure to Azure.

.DESCRIPTION
    1. Reads the current Azure CLI context (subscription / tenant).
    2. Resolves the two secrets (PostgreSQL admin password, JWT signing key) from environment
       variables, or generates strong values and prints them once.
    3. Runs `az bicep build` (lint) on infra/main.bicep.
    4. Runs `az deployment sub what-if` as the pre-flight validation.
    5. With -Deploy, runs `az deployment sub create` (creates the resource group + all resources).

.EXAMPLE
    ./deploy.ps1                       # context + lint + what-if only (safe default)
    ./deploy.ps1 -Deploy               # ... then apply
    $env:CPG_PG_ADMIN_PASSWORD = '...' ; $env:CPG_JWT_SIGNING_KEY = '...' ; ./deploy.ps1 -Deploy
#>
[CmdletBinding()]
param(
    [string] $Location = 'centralus',
    [string] $SubscriptionId,
    [switch] $Deploy,
    [switch] $SkipWhatIf
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$template = Join-Path $here 'main.bicep'
$parameters = Join-Path $here 'main.parameters.json'

function New-StrongSecret([int] $Bytes = 32) {
    # RNGCryptoServiceProvider (not the static RandomNumberGenerator.Fill helper) so this
    # works on both Windows PowerShell 5.1 (.NET Framework) and PowerShell 7+ (.NET).
    $buffer = [byte[]]::new($Bytes)
    $rng = [System.Security.Cryptography.RNGCryptoServiceProvider]::new()
    try {
        $rng.GetBytes($buffer)
    }
    finally {
        $rng.Dispose()
    }
    return [Convert]::ToBase64String($buffer)
}

# --- 1. Azure context -----------------------------------------------------------------
if ($SubscriptionId) { az account set --subscription $SubscriptionId | Out-Null }
$ctx = az account show --output json | ConvertFrom-Json
if (-not $ctx) { throw 'Not signed in. Run "az login" first.' }

Write-Host '--------------------------------------------------------------------------'
Write-Host (" Subscription : {0}" -f $ctx.name)
Write-Host (" Subscription : {0}" -f $ctx.id)
Write-Host (" Tenant       : {0}" -f $ctx.tenantId)
Write-Host (" Location     : {0}" -f $Location)
Write-Host '--------------------------------------------------------------------------'

# --- 2. Secrets ---------------------------------------------------------------------
$pgPassword = $env:CPG_PG_ADMIN_PASSWORD
if ([string]::IsNullOrWhiteSpace($pgPassword)) {
    $pgPassword = (New-StrongSecret 18) + 'Aa1!'
    Write-Warning "Generated PostgreSQL admin password (store it now; it will also live in Key Vault):`n  $pgPassword"
}

$jwtKey = $env:CPG_JWT_SIGNING_KEY
if ([string]::IsNullOrWhiteSpace($jwtKey)) {
    $jwtKey = New-StrongSecret 48
    Write-Warning "Generated JWT signing key (also stored in Key Vault):`n  $jwtKey"
}

$secureParams = @(
    "postgresAdministratorPassword=$pgPassword",
    "jwtSigningKey=$jwtKey"
)

# --- 3. Lint ----------------------------------------------------------------------
Write-Host "`n> az bicep build (lint)"
az bicep build --file $template --stdout | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Bicep build failed.' }
Write-Host '  bicep build: OK'

# --- 4. What-if pre-flight -------------------------------------------------------
if (-not $SkipWhatIf) {
    Write-Host "`n> az deployment sub what-if"
    az deployment sub what-if `
        --location $Location `
        --template-file $template `
        --parameters $parameters `
        --parameters $secureParams
    if ($LASTEXITCODE -ne 0) { throw 'what-if failed.' }
}

# --- 5. Deploy ------------------------------------------------------------------
if ($Deploy) {
    $name = "cpg-infra-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Write-Host "`n> az deployment sub create ($name)"
    az deployment sub create `
        --name $name `
        --location $Location `
        --template-file $template `
        --parameters $parameters `
        --parameters $secureParams `
        --query 'properties.outputs' `
        --output json
    if ($LASTEXITCODE -ne 0) { throw 'Deployment failed.' }
    Write-Host "`nDeployment complete."
}
else {
    Write-Host "`nPre-flight only. Re-run with -Deploy to apply." -ForegroundColor Yellow
}
