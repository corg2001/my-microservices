// ============================================================
// main.bicep — My Microservices Portfolio
// Covers: Container Apps, ACR, Cosmos DB, Service Bus (Standard),
//         Azure SQL, APIM, Event Hubs (Phase 2), Log Analytics,
//         Storage Account (Debezium offset storage)
// Deploy: az deployment group create \
//           --resource-group my-microservices-rg \
//           --template-file infra/main.bicep \
//           --parameters sqlAdminPassword=<secret>
// ============================================================

@description('Azure region for all resources')
param location string = 'eastus'

@description('SQL region — free tier db is pinned to West US 2')
param sqlLocation string = 'westus2'

@description('SQL admin password — pass via GitHub Secret, never hardcode')
@secure()
param sqlAdminPassword string

// ── Existing resource names ───────────────────────────────────
var acrName                = 'mymicroservicesacr'
var cosmosAccountName      = 'my-microservices-cosmos'
var serviceBusName         = 'my-microservices-bus'
var sqlServerName          = 'my-microservices-sql'
var sqlDatabaseName        = 'my-microservices-db'
var apimName               = 'my-microservices-apim'
var containerAppsEnvName   = 'my-microservices-env'
var logAnalyticsName       = 'workspace-mymicroservicesrgCzlN'

// Phase 2
var eventHubsNamespaceName = 'my-microservices-eh'
var eventHubName           = 'employee-salary-changes'
var storageAccountName     = 'mymicroservicesstore'   // max 24 chars, lowercase, no hyphens

// ── Log Analytics Workspace ───────────────────────────────────
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// ── Azure Container Registry ──────────────────────────────────
resource acr 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

// ── Cosmos DB ─────────────────────────────────────────────────
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    capabilities: [
      { name: 'EnableServerless' }
    ]
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
  }
}

resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  parent: cosmosAccount
  name: 'MyDatabase'
  properties: {
    resource: {
      id: 'MyDatabase'
    }
  }
}

resource cosmosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDatabase
  name: 'Orders'
  properties: {
    resource: {
      id: 'Orders'
      partitionKey: {
        paths: [ '/id' ]
        kind: 'Hash'
      }
    }
  }
}

// ── Service Bus (Standard — required for topics/pub-sub) ──────
resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

// Existing queue — kept so OrderService pub/sub continues working
resource orderCreatedQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBus
  name: 'order-created'
  properties: {
    defaultMessageTimeToLive: 'P14D'
    lockDuration: 'PT30S'
  }
}

// Phase 2 topic — CDC salary change events
resource salaryChangesTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBus
  name: 'employee-salary-changes'
  properties: {
    defaultMessageTimeToLive: 'P14D'
  }
}

// Subscription for EmployeeService (or future SalaryEventService)
resource salaryChangesSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: salaryChangesTopic
  name: 'salary-processor-sub'
  properties: {
    defaultMessageTimeToLive: 'P14D'
    lockDuration: 'PT30S'
    maxDeliveryCount: 10
  }
}

// ── Azure SQL ─────────────────────────────────────────────────
resource sqlServer 'Microsoft.Sql/servers@2022-11-01-preview' = {
  name: sqlServerName
  location: sqlLocation
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: sqlAdminPassword
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-11-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: sqlLocation
  sku: {
    name: 'GP_S_Gen5_1'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    autoPauseDelay: 60
    minCapacity: json('0.5')
  }
}

// Allow Azure services to reach SQL (required for Container Apps + EF Core)
resource sqlFirewallAzureServices 'Microsoft.Sql/servers/firewallRules@2022-11-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ── API Management ────────────────────────────────────────────
resource apim 'Microsoft.ApiManagement/service@2022-08-01' = {
  name: apimName
  location: location
  sku: {
    name: 'Developer'
    capacity: 1
  }
  properties: {
    publisherEmail: 'admin@my-microservices.local'
    publisherName: 'My Microservices'
  }
}

// ── Container Apps Environment ────────────────────────────────
resource containerAppsEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppsEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// ── Event Hubs (Phase 2 — Debezium CDC sink) ──────────────────
resource eventHubsNamespace 'Microsoft.EventHub/namespaces@2022-10-01-preview' = {
  name: eventHubsNamespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    kafkaEnabled: true   // Debezium connects via Kafka protocol
  }
}

resource eventHub 'Microsoft.EventHub/namespaces/eventhubs@2022-10-01-preview' = {
  parent: eventHubsNamespace
  name: eventHubName
  properties: {
    messageRetentionInDays: 1
    partitionCount: 2
  }
}

// Debezium internal topics — required before Debezium starts.
// cdc.configs: stores connector configurations
// cdc.offsets: tracks CDC log position (where Debezium left off)
// cdc.status:  tracks connector and task status
resource debeziumConfigsTopic 'Microsoft.EventHub/namespaces/eventhubs@2022-10-01-preview' = {
  parent: eventHubsNamespace
  name: 'cdc.configs'
  properties: {
    messageRetentionInDays: 1
    partitionCount: 1
  }
}

resource debeziumOffsetsTopic 'Microsoft.EventHub/namespaces/eventhubs@2022-10-01-preview' = {
  parent: eventHubsNamespace
  name: 'cdc.offsets'
  properties: {
    messageRetentionInDays: 1
    partitionCount: 25
  }
}

resource debeziumStatusTopic 'Microsoft.EventHub/namespaces/eventhubs@2022-10-01-preview' = {
  parent: eventHubsNamespace
  name: 'cdc.status'
  properties: {
    messageRetentionInDays: 1
    partitionCount: 1
  }
}

// ── Storage Account (Debezium offset + schema registry storage) 
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'   // Locally redundant — sufficient for offset tracking
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

// Container for Debezium offset storage
resource debeziumOffsetContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/debezium-offsets'
  properties: {
    publicAccess: 'None'
  }
}

// ── Outputs (useful for pipeline steps and debugging) ─────────
output acrLoginServer string = acr.properties.loginServer
output containerAppsEnvId string = containerAppsEnv.id
output serviceBusEndpoint string = serviceBus.properties.serviceBusEndpoint
output eventHubsEndpoint string = eventHubsNamespace.properties.serviceBusEndpoint
output cosmosEndpoint string = cosmosAccount.properties.documentEndpoint
output storageAccountName string = storageAccount.name

// ── Debezium Container App ────────────────────────────────────
// Runs continuously — reads CDC log from Azure SQL via Debezium
// and writes change events to Event Hubs (Kafka surface).
@secure()
param eventHubsConnectionString string

@secure()
param storageConnectionString string

@secure()
param sqlAdminPasswordDebezium string = sqlAdminPassword

resource debeziumContainerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: 'debezium'
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnv.id
    configuration: {
      secrets: [
        {
          name: 'eventhubs-connection-string'
          value: eventHubsConnectionString
        }
        {
          name: 'storage-connection-string'
          value: storageConnectionString
        }
        {
          name: 'sql-password'
          value: sqlAdminPasswordDebezium
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'debezium'
          image: '${acrName}.azurecr.io/debezium:latest'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'BOOTSTRAP_SERVERS'
              value: '${eventHubsNamespaceName}.servicebus.windows.net:9093'
            }
            {
              name: 'GROUP_ID'
              value: 'debezium-cdc-group'
            }
            {
              name: 'CONFIG_STORAGE_TOPIC'
              value: 'cdc.configs'
            }
            {
              name: 'OFFSET_STORAGE_TOPIC'
              value: 'cdc.offsets'
            }
            {
              name: 'STATUS_STORAGE_TOPIC'
              value: 'cdc.status'
            }
            {
              name: 'CONNECT_SECURITY_PROTOCOL'
              value: 'SASL_SSL'
            }
            {
              name: 'CONNECT_SASL_MECHANISM'
              value: 'PLAIN'
            }
            {
              name: 'EVENTHUBS_CONNECTION_STRING'
              secretRef: 'eventhubs-connection-string'
            }
            {
              name: 'STORAGE_CONNECTION_STRING'
              secretRef: 'storage-connection-string'
            }
            {
              name: 'SQL_PASSWORD'
              secretRef: 'sql-password'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1   // Must stay running — CDC requires continuous connection
        maxReplicas: 1   // Single instance — multiple instances would cause duplicate events
      }
    }
  }
}
