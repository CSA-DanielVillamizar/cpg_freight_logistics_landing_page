// Key Vault (RBAC authorization) - the single source of truth for connection strings and secrets.
// The workload identity is granted "Key Vault Secrets User" so the Container Apps resolve
// Key Vault references at runtime without any secret value landing in an environment variable.

@description('Key Vault name (3-24 chars, globally unique).')
param name string

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

@description('Principal id of the workload managed identity that reads secrets.')
param principalId string

@description('Enable purge protection (cannot be disabled once enabled).')
param enablePurgeProtection bool

@description('JWT signing key persisted as a secret.')
@secure()
param jwtSigningKey string

// Built-in role: Key Vault Secrets User
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenant().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enablePurgeProtection: enablePurgeProtection ? true : null
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

resource jwtSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'jwt-signing-key'
  properties: {
    value: jwtSigningKey
    contentType: 'text/plain'
  }
}

resource secretsUserAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, principalId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    principalId: principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
  }
}

output vaultName string = keyVault.name
output vaultUri string = keyVault.properties.vaultUri
output vaultId string = keyVault.id
