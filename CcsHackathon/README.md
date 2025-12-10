# CCS Hackathon - Blazor Server App

This is a Blazor Server application with Microsoft Entra ID (Azure AD) authentication and Microsoft Graph integration.

## Prerequisites

- .NET 10 SDK
- An Azure AD tenant
- An Azure AD app registration with the following:
  - Client ID
  - Client Secret
  - Tenant ID
  - API permissions: `User.Read.All` (Application permission) or `User.Read.All` (Delegated permission)

## Configuration

### 1. Configure appsettings.json

Update the `AzureAd` section in `appsettings.json` with your Azure AD details:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourdomain.onmicrosoft.com",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "CallbackPath": "/signin-oidc"
  },
  "MicrosoftGraph": {
    "BaseUrl": "https://graph.microsoft.com/v1.0",
    "Scopes": "User.Read.All"
  }
}
```

### 2. Configure User Secrets (Recommended for Development)

For development, use User Secrets to store sensitive configuration:

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureAd:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "AzureAd:TenantId" "YOUR_TENANT_ID"
dotnet user-secrets set "AzureAd:ClientSecret" "YOUR_CLIENT_SECRET"
```

User secrets will override values in `appsettings.json`, so you can keep placeholder values in the JSON file.

## Azure AD App Registration Setup

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Create a new app registration or use an existing one
4. Configure the following:
   - **Redirect URIs**: Add `https://localhost:5001/signin-oidc` (or your production URL)
   - **API permissions**: Add `Microsoft Graph` > `User.Read.All` (Application or Delegated)
   - **Certificates & secrets**: Create a client secret and copy the value

## Running the Application

```bash
dotnet run
```

Navigate to `https://localhost:5001` (or the port shown in the console).

## Pages

- **Login** (`/login`): Microsoft sign-in page
- **Dashboard** (`/dashboard`): Lists users retrieved from Microsoft Graph (requires authentication)
- **Register Game** (`/register-game`): Form to register board games (requires authentication)

## Services

- `IGraphService` / `GraphService`: Retrieves users from Microsoft Graph
- `IAgentOrchestrator` / `AgentOrchestrator`: Empty interface for future implementation

## Project Structure

```
CcsHackathon/
├── Components/
│   ├── Pages/
│   │   ├── Login.razor
│   │   ├── Dashboard.razor
│   │   └── RegisterGame.razor
│   └── Layout/
├── Services/
│   ├── IGraphService.cs
│   ├── GraphService.cs
│   ├── IAgentOrchestrator.cs
│   └── AgentOrchestrator.cs
├── Program.cs
└── appsettings.json
```

