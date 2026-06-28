# My Microservices

A cloud-native microservices architecture built with .NET 9, Dapr, and Azure.

## Architecture
                ┌─────────────────────────────────────────┐
                │         Azure API Management            │
                └──────────────┬──────────────────────────┘
                               │
          ┌────────────────────┼────────────────────┐
          │                    │                     │
          ▼                    ▼                     ▼
    OrderService         InventoryService      EmployeeService
    (Dapr enabled)       (Dapr enabled)        (EF Core)
          │                    │                     │
          ├──► Cosmos DB       │                     │
          │    (state store)   │                     │
          │                    │                     │
          └──► Service Bus ────┘                     │
               (pub/sub)                             │
                                                     ▼
                                                 Azure SQL

## Services

| Service | Description | Stack |
|---|---|---|
| OrderService | Handles order creation and retrieval | .NET 9, Dapr, Cosmos DB |
| InventoryService | Subscribes to order events and updates inventory | .NET 9, Dapr, Service Bus |
| EmployeeService | Manages employee records | .NET 9, EF Core, Azure SQL |

## Azure Resources

- **Azure Container Apps** — hosts all three microservices
- **Azure API Management** — single entry point, rate limiting, OAuth 2.0
- **Azure Cosmos DB** — NoSQL state store for orders (via Dapr)
- **Azure Service Bus** — async messaging between services (via Dapr pub/sub)
- **Azure Container Registry** — stores Docker images
- **Azure SQL Database** — relational storage for employees (EF Core)
- **Azure Entra ID** — OAuth 2.0 authentication

## Tech Stack

- .NET 9 / ASP.NET Core Web APIs
- Dapr (state store + pub/sub abstraction)
- Entity Framework Core (Code First migrations)
- xUnit unit tests
- Docker / Azure Container Apps
- OAuth 2.0 / Microsoft Entra ID
- Swagger / OpenAPI
- Bicep Infrastructure as Code

## Local Development

### Prerequisites
- .NET 9 SDK
- Docker Desktop
- Dapr CLI
- Azure CLI

### Run locally with Dapr

```bash
# OrderService
cd My_Microservices
dapr run --app-id orderservice --app-port 5222 --dapr-http-port 3500 --resources-path ../components -- dotnet run

# InventoryService (new terminal)
cd InventoryService
dapr run --app-id inventoryservice --app-port 5170 --dapr-http-port 3501 --resources-path ../components -- dotnet run

# EmployeeService (new terminal)
cd EmployeeService
dotnet run
```

### Run tests

```bash
dotnet test
```

## Configuration

Copy `appsettings.json` as `appsettings.Development.json` and fill in your values:

```json
{
  "CosmosDb": {
    "Endpoint": "<your-cosmos-endpoint>",
    "AuthKey": "<your-cosmos-key>",
    "DatabaseId": "MyDatabase",
    "ContainerId": "Orders",
    "PartitionKeyPath": "/id"
  },
  "ServiceBus": {
    "ConnectionString": "<your-service-bus-connection-string>",
    "QueueName": "order-created"
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<your-tenant-id>",
    "ClientId": "<your-client-id>",
    "Scopes": "access_as_user"
  }
}
```

## Deployment

All infrastructure is defined in `infrastructure/main.bicep`:

```bash
az deployment group create \
  --resource-group my-microservices-rg \
  --template-file infrastructure/main.bicep \
  --parameters sqlAdminPassword=<your-password>
```