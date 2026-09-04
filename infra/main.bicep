// =====================================================================================
//  CPG Enterprises Logistics Platform - Production infrastructure (Azure, IaC)
//  Orchestrator. Creates the resource group and wires the logical modules.
//
//  FinOps posture:
//    - Azure Container Apps (Consumption) with scale-to-zero  -> no compute cost when idle
//    - PostgreSQL Flexible Server, Burstable B1ms             -> accrues CPU credits off-peak
//    - Service Bus Standard, Storage Standard_LRS, ACR Basic  -> lowest viable SKUs
//  Zero-trust posture:
//    - One user-assigned managed identity for both Container Apps
//    - Key Vault (RBAC) holds every connection string / secret; apps read via KV references
//    - Blob Storage and Service Bus reached via managed identity (RBAC), no shared keys in env
// =====================================================================================

targetScope = 'subscription'

// ---------------------------------------------------------------------------------------
// Parameters
// ---------------------------------------------------------------------------------------

@description('Azure region for every resource.')
param location string = 'centralus'

@description('CAF mnemonic used to compose resource names.')
param namePrefix string = 'cpgorlando'

@description('Environment abbreviation segment of the name.')
param environmentAbbr string = 'prd'

@description('Region abbreviation segment of the name.')
param regionAbbr string = 'cus'

@description('Instance number segment of the name.')
param instance string = '01'

@description('Mandatory tags applied to every resource.')
param tags object = {
  Project: 'CPGOrlando'
  Environment: 'Production'
  CostCenter: 'LogisticsPlatform'
}

@description('PostgreSQL Flexible Server administrator login.')
param postgresAdministratorLogin string = 'cpgpgadmin'

@description('PostgreSQL Flexible Server administrator password. Supplied at deploy time.')
@secure()
param postgresAdministratorPassword string

@description('JWT signing key for the API (>= 32 bytes). Supplied at deploy time.')
@secure()
param jwtSigningKey string

@description('JWT issuer claim.')
param jwtIssuer string = 'cpg-enterprises'

@description('JWT audience claim.')
param jwtAudience string = 'cpg-enterprises-clients'

@description('Container image for the API. Replace with the ACR image once built and pushed.')
param apiContainerImage string = 'mcr.microsoft.com/k8se/quickstart:latest'

@description('Container image for the web frontend. Replace with the ACR image once built and pushed.')
param webContainerImage string = 'mcr.microsoft.com/k8se/quickstart:latest'

@description('API Container App minimum replicas. 0 = scale-to-zero (FinOps default).')
@minValue(0)
@maxValue(5)
param apiMinReplicas int = 0

@description('API Container App maximum replicas.')
@minValue(1)
@maxValue(30)
param apiMaxReplicas int = 10

@description('Web Container App minimum replicas. 0 = scale-to-zero (FinOps default).')
@minValue(0)
@maxValue(5)
param webMinReplicas int = 0

@description('Web Container App maximum replicas.')
@minValue(1)
@maxValue(30)
param webMaxReplicas int = 5

@description('Log Analytics retention in days (30 is the free-tier ceiling).')
@minValue(30)
@maxValue(730)
param logRetentionDays int = 30

@description('Enable Key Vault purge protection. Leave false for greenfield so teardown does not require a purge.')
param enableKeyVaultPurgeProtection bool = false

// ---------------------------------------------------------------------------------------
// Naming (CAF: <type>-<mnemonic>-<env>-<region>-<instance>; no-hyphen types are concatenated)
// ---------------------------------------------------------------------------------------

var suffix = '${namePrefix}-${environmentAbbr}-${regionAbbr}-${instance}'
var suffixFlat = '${namePrefix}${environmentAbbr}${regionAbbr}${instance}'

var names = {
  resourceGroup: 'rg-${suffix}'
  identity: 'id-${suffix}'
  keyVault: 'kv-${suffix}'
  storage: 'st${suffixFlat}'
  registry: 'cr${suffixFlat}'
  serviceBus: 'sb-${suffix}'
  postgres: 'psql-${suffix}'
  logAnalytics: 'log-${suffix}'
  appInsights: 'appi-${suffix}'
  containerEnv: 'cae-${suffix}'
  apiApp: 'ca-${namePrefix}-api-${environmentAbbr}-${regionAbbr}-${instance}'
  webApp: 'ca-${namePrefix}-web-${environmentAbbr}-${regionAbbr}-${instance}'
}

var postgresDatabaseName = 'cpg'

// ---------------------------------------------------------------------------------------
// Resource group
// ---------------------------------------------------------------------------------------

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: names.resourceGroup
  location: location
  tags: tags
}

// ---------------------------------------------------------------------------------------
// Modules
// ---------------------------------------------------------------------------------------

module identity 'modules/identity.bicep' = {
  name: 'identity'
  scope: resourceGroup
  params: {
    name: names.identity
    location: location
    tags: tags
  }
}

module keyVault 'modules/keyvault.bicep' = {
  name: 'keyvault'
  scope: resourceGroup
  params: {
    name: names.keyVault
    location: location
    tags: tags
    principalId: identity.outputs.principalId
    enablePurgeProtection: enableKeyVaultPurgeProtection
    jwtSigningKey: jwtSigningKey
  }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  scope: resourceGroup
  params: {
    name: names.storage
    location: location
    tags: tags
    principalId: identity.outputs.principalId
  }
}

module messaging 'modules/messaging.bicep' = {
  name: 'messaging'
  scope: resourceGroup
  params: {
    name: names.serviceBus
    location: location
    tags: tags
    principalId: identity.outputs.principalId
    keyVaultName: keyVault.outputs.vaultName
  }
}

module database 'modules/db.bicep' = {
  name: 'database'
  scope: resourceGroup
  params: {
    name: names.postgres
    location: location
    tags: tags
    administratorLogin: postgresAdministratorLogin
    administratorPassword: postgresAdministratorPassword
    databaseName: postgresDatabaseName
    keyVaultName: keyVault.outputs.vaultName
  }
}

module apps 'modules/apps.bicep' = {
  name: 'apps'
  scope: resourceGroup
  params: {
    location: location
    tags: tags
    logAnalyticsName: names.logAnalytics
    appInsightsName: names.appInsights
    registryName: names.registry
    containerEnvName: names.containerEnv
    apiAppName: names.apiApp
    webAppName: names.webApp
    logRetentionDays: logRetentionDays

    managedIdentityId: identity.outputs.resourceId
    managedIdentityClientId: identity.outputs.clientId
    managedIdentityPrincipalId: identity.outputs.principalId

    keyVaultUri: keyVault.outputs.vaultUri
    blobServiceUri: storage.outputs.blobEndpoint
    serviceBusHostName: messaging.outputs.hostName

    apiContainerImage: apiContainerImage
    webContainerImage: webContainerImage
    apiMinReplicas: apiMinReplicas
    apiMaxReplicas: apiMaxReplicas
    webMinReplicas: webMinReplicas
    webMaxReplicas: webMaxReplicas

    jwtIssuer: jwtIssuer
    jwtAudience: jwtAudience
  }
}

// ---------------------------------------------------------------------------------------
// Outputs (no secrets)
// ---------------------------------------------------------------------------------------

output resourceGroupName string = resourceGroup.name
output keyVaultName string = keyVault.outputs.vaultName
output containerRegistryLoginServer string = apps.outputs.registryLoginServer
output apiUrl string = apps.outputs.apiUrl
output webUrl string = apps.outputs.webUrl
output postgresFqdn string = database.outputs.fullyQualifiedDomainName
output serviceBusHostName string = messaging.outputs.hostName
output storageAccountName string = storage.outputs.accountName
output managedIdentityClientId string = identity.outputs.clientId
