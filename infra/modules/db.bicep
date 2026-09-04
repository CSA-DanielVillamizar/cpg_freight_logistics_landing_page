// Azure Database for PostgreSQL - Flexible Server, Burstable B1ms.
//   Burstable accrues CPU credits during off-peak hours; ideal for spiky dispatch traffic (FinOps).
//   The full ADO.NET connection string (with the admin password) is stored in Key Vault.

@description('PostgreSQL Flexible Server name (3-63 lowercase, globally unique for the FQDN).')
param name string

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

@description('Administrator login.')
param administratorLogin string

@description('Administrator password.')
@secure()
param administratorPassword string

@description('Application database name.')
param databaseName string

@description('Name of the existing Key Vault that receives the connection string secret.')
param keyVaultName string

@description('PostgreSQL major version.')
param postgresVersion string = '16'

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2023-06-01-preview' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: postgresVersion
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
    storage: {
      storageSizeGB: 32
      autoGrow: 'Enabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-06-01-preview' = {
  parent: postgres
  name: databaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// Allow other Azure services (Container Apps Consumption has no stable egress IPs).
// Production hardening: VNet integration + Private Endpoint (see infra/README.md).
resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-06-01-preview' = {
  parent: postgres
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource requireTls 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2023-06-01-preview' = {
  parent: postgres
  name: 'require_secure_transport'
  properties: {
    value: 'ON'
    source: 'user-override'
  }
}

var connectionString = 'Host=${postgres.properties.fullyQualifiedDomainName};Port=5432;Database=${databaseName};Username=${administratorLogin};Password=${administratorPassword};Ssl Mode=Require;Trust Server Certificate=true'

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource postgresSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'postgres-connection-string'
  properties: {
    value: connectionString
    contentType: 'text/plain'
  }
  dependsOn: [
    database
  ]
}

output serverName string = postgres.name
output fullyQualifiedDomainName string = postgres.properties.fullyQualifiedDomainName
output databaseName string = database.name
