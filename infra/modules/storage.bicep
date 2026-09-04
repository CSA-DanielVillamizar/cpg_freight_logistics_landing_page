// Storage account for compliance documents.
//   - Shared-key access disabled -> the API reaches Blob storage only via managed identity.
//   - Lifecycle policy tiers the compliance-documents container Cool @ 90d, Archive @ 180d (FinOps).

@description('Storage account name (3-24 lowercase alphanumerics, globally unique).')
param name string

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

@description('Principal id of the workload managed identity that reads/writes blobs.')
param principalId string

@description('Blob container for carrier compliance documents.')
param complianceContainerName string = 'compliance-documents'

// Built-in role: Storage Blob Data Contributor
var blobDataContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource complianceContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobServices
  name: complianceContainerName
  properties: {
    publicAccess: 'None'
  }
}

resource lifecyclePolicy 'Microsoft.Storage/storageAccounts/managementPolicies@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    policy: {
      rules: [
        {
          name: 'compliance-documents-tiering'
          enabled: true
          type: 'Lifecycle'
          definition: {
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                '${complianceContainerName}/'
              ]
            }
            actions: {
              baseBlob: {
                tierToCool: {
                  daysAfterModificationGreaterThan: 90
                }
                tierToArchive: {
                  daysAfterModificationGreaterThan: 180
                }
              }
            }
          }
        }
      ]
    }
  }
}

resource blobContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, principalId, blobDataContributorRoleId)
  scope: storageAccount
  properties: {
    principalId: principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', blobDataContributorRoleId)
  }
}

output accountName string = storageAccount.name
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
output containerName string = complianceContainer.name
