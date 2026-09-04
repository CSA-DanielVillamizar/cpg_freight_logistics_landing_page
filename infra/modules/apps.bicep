// Compute: Log Analytics + Application Insights + Container Registry + Container Apps
// environment (Consumption) + the API and web Container Apps.
//
// FinOps: both apps default to minReplicas = 0 (scale-to-zero) on the Consumption profile.
// Zero-trust: the workload identity pulls from ACR (AcrPull) and every sensitive value is a
// Key Vault reference resolved with that same identity - no secret value in an env var.

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

param logAnalyticsName string
param appInsightsName string
param registryName string
param containerEnvName string
param apiAppName string
param webAppName string

@minValue(30)
param logRetentionDays int

@description('Resource id of the shared user-assigned managed identity.')
param managedIdentityId string

@description('Client id of the shared user-assigned managed identity (for DefaultAzureCredential).')
param managedIdentityClientId string

@description('Principal id of the shared user-assigned managed identity (for RBAC).')
param managedIdentityPrincipalId string

@description('Key Vault URI (ends with a slash).')
param keyVaultUri string

@description('Blob service endpoint of the storage account.')
param blobServiceUri string

@description('Service Bus fully-qualified namespace host.')
param serviceBusHostName string

param apiContainerImage string
param webContainerImage string

@minValue(0)
param apiMinReplicas int

@minValue(1)
param apiMaxReplicas int

@minValue(0)
param webMinReplicas int

@minValue(1)
param webMaxReplicas int

param jwtIssuer string
param jwtAudience string

// Built-in role: AcrPull
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'

var apiInternalPort = 8080
var webInternalPort = 80

// ---------------------------------------------------------------------------------------
// Observability
// ---------------------------------------------------------------------------------------

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: logRetentionDays
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
  }
}

// ---------------------------------------------------------------------------------------
// Container Registry (Basic - lowest SKU; admin user disabled, pull via managed identity)
// ---------------------------------------------------------------------------------------

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: registryName
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
  }
}

resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, managedIdentityPrincipalId, acrPullRoleId)
  scope: containerRegistry
  properties: {
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
  }
}

// ---------------------------------------------------------------------------------------
// Container Apps environment (Consumption workload profile)
// ---------------------------------------------------------------------------------------

resource containerEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerEnvName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
    zoneRedundant: false
  }
}

// ---------------------------------------------------------------------------------------
// API Container App
// ---------------------------------------------------------------------------------------

resource apiApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: apiAppName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerEnv.id
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      maxInactiveRevisions: 2
      ingress: {
        external: true
        targetPort: apiInternalPort
        transport: 'auto'
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: managedIdentityId
        }
      ]
      secrets: [
        {
          name: 'postgres-connection-string'
          keyVaultUrl: '${keyVaultUri}secrets/postgres-connection-string'
          identity: managedIdentityId
        }
        {
          name: 'jwt-signing-key'
          keyVaultUrl: '${keyVaultUri}secrets/jwt-signing-key'
          identity: managedIdentityId
        }
        {
          name: 'servicebus-connection-string'
          keyVaultUrl: '${keyVaultUri}secrets/servicebus-connection-string'
          identity: managedIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiContainerImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_HTTP_PORTS'
              value: string(apiInternalPort)
            }
            {
              name: 'ConnectionStrings__Postgres'
              secretRef: 'postgres-connection-string'
            }
            {
              name: 'ConnectionStrings__ServiceBus'
              secretRef: 'servicebus-connection-string'
            }
            {
              name: 'ServiceBus__FullyQualifiedNamespace'
              value: serviceBusHostName
            }
            {
              name: 'Jwt__SigningKey'
              secretRef: 'jwt-signing-key'
            }
            {
              name: 'Jwt__Issuer'
              value: jwtIssuer
            }
            {
              name: 'Jwt__Audience'
              value: jwtAudience
            }
            {
              name: 'BlobStorage__Provider'
              value: 'AzureManagedIdentity'
            }
            {
              name: 'BlobStorage__ServiceUri'
              value: blobServiceUri
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: managedIdentityClientId
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsights.properties.ConnectionString
            }
            {
              name: 'Cors__AllowedOrigins__0'
              value: 'https://${webAppName}.${containerEnv.properties.defaultDomain}'
            }
            {
              name: 'AllowedHosts'
              value: '*'
            }
          ]
        }
      ]
      scale: {
        minReplicas: apiMinReplicas
        maxReplicas: apiMaxReplicas
        rules: [
          {
            name: 'http-concurrency'
            http: {
              metadata: {
                concurrentRequests: '20'
              }
            }
          }
        ]
      }
    }
  }
  dependsOn: [
    acrPullAssignment
  ]
}

// ---------------------------------------------------------------------------------------
// Web Container App
// ---------------------------------------------------------------------------------------

resource webApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: webAppName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerEnv.id
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      maxInactiveRevisions: 2
      ingress: {
        external: true
        targetPort: webInternalPort
        transport: 'auto'
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: managedIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'web'
          image: webContainerImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'API_BASE_URL'
              value: 'https://${apiApp.properties.configuration.ingress.fqdn}/api'
            }
            {
              name: 'VITE_API_BASE_URL'
              value: 'https://${apiApp.properties.configuration.ingress.fqdn}/api'
            }
          ]
        }
      ]
      scale: {
        minReplicas: webMinReplicas
        maxReplicas: webMaxReplicas
        rules: [
          {
            name: 'http-concurrency'
            http: {
              metadata: {
                concurrentRequests: '40'
              }
            }
          }
        ]
      }
    }
  }
  dependsOn: [
    acrPullAssignment
  ]
}

output registryLoginServer string = containerRegistry.properties.loginServer
output registryName string = containerRegistry.name
output containerEnvName string = containerEnv.name
output apiUrl string = 'https://${apiApp.properties.configuration.ingress.fqdn}'
output webUrl string = 'https://${webApp.properties.configuration.ingress.fqdn}'
output appInsightsName string = appInsights.name
