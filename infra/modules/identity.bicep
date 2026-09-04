// User-assigned managed identity shared by both Container Apps (zero-trust: one workload identity).

@description('Managed identity name.')
param name string

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: name
  location: location
  tags: tags
}

output resourceId string = managedIdentity.id
output principalId string = managedIdentity.properties.principalId
output clientId string = managedIdentity.properties.clientId
output name string = managedIdentity.name
