// Azure Service Bus (Standard) - the production replacement for the ephemeral local RabbitMQ.
//   - A scoped SAS rule connection string is stored in Key Vault (satisfies "all connection
//     strings in Key Vault").
//   - The workload identity is ALSO granted "Azure Service Bus Data Owner" so the app can
//     move to passwordless (managed identity) messaging without any infra change.

@description('Service Bus namespace name (6-50 chars, globally unique).')
param name string

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

@description('Principal id of the workload managed identity.')
param principalId string

@description('Name of the existing Key Vault that receives the connection string secret.')
param keyVaultName string

// Built-in role: Azure Service Bus Data Owner
var serviceBusDataOwnerRoleId = '090c5cfd-751d-490a-894a-3ce6f1109419'

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
    disableLocalAuth: false
    publicNetworkAccess: 'Enabled'
    zoneRedundant: false
  }
}

// MassTransit manages its own topics/subscriptions, so the application rule needs Manage.
resource applicationAuthRule 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'cpg-application'
  properties: {
    rights: [
      'Manage'
      'Send'
      'Listen'
    ]
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource serviceBusSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'servicebus-connection-string'
  properties: {
    value: applicationAuthRule.listKeys().primaryConnectionString
    contentType: 'text/plain'
  }
}

resource dataOwnerAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBusNamespace.id, principalId, serviceBusDataOwnerRoleId)
  scope: serviceBusNamespace
  properties: {
    principalId: principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataOwnerRoleId)
  }
}

output namespaceName string = serviceBusNamespace.name
output hostName string = replace(replace(serviceBusNamespace.properties.serviceBusEndpoint, 'https://', ''), ':443/', '')
