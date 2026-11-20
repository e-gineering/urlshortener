# URL Shortener

A .NET 10 web application for shortening URLs with QR code generation capabilities, built with ASP.NET Core and Azure Table Storage.

See AGENTS.md for guidance on working with AI coding agents in this repository.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Azure Storage Account (for storing URL mappings)
- Optional: Visual Studio 2022 or VS Code with C# extensions

## Configuration

The application requires configuration in `appsettings.json` or via user secrets:

1. **Azure Storage Connection String**: Configure the Azure Table Storage connection
2. **Security Token**: Set a security token for protected operations
3. **QR Code Theme** (optional): Customize QR code appearance

### Using User Secrets (Recommended for Development)

```bash
cd Egineering.UrlShortener
dotnet user-secrets set "ConnectionStrings:AzureStorage" "your-azure-storage-connection-string"
dotnet user-secrets set "SecurityToken" "your-security-token"
```

### Using appsettings.Development.json

Create or update `Egineering.UrlShortener/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "AzureStorage": "UseDevelopmentStorage=true"
  },
  "SecurityToken": "your-dev-token"
}
```

**Note**: For local development, you can use [Azurite](https://github.com/Azure/Azurite) emulator with `UseDevelopmentStorage=true`.

## Running the Application

### Option 1: Using .NET CLI (Recommended)

Due to Windows port restrictions (Hyper-V reserved ports), use explicit port configuration:

```bash
dotnet run --project Egineering.UrlShortener/Egineering.UrlShortener.csproj --urls "http://localhost:8080;https://localhost:8081"
```

The application will start at:
- HTTP: `http://localhost:8080`
- HTTPS: `https://localhost:8081`

### Option 2: Using Visual Studio

1. Open `Egineering.UrlShortener.sln`
2. Set `Egineering.UrlShortener` as the startup project
3. Press F5 or click the Run button

### Option 3: Using VS Code

1. Open the workspace folder
2. Press F5 or use the Run and Debug panel
3. Select ".NET Core Launch (web)" configuration

## Accessing the Application

- **Main Page**: `http://localhost:8080/` - View all shortened URLs
- **Setup Page**: `http://localhost:8080/setup` - Configure security key
- **Scalar API reference**: `http://localhost:8080/scalar` (Development mode only)
- **OpenAPI document**: `http://localhost:8080/openapi/v1.json` (Development mode only)

## API Usage

### QR Codes

To generate a QR code, base64 encode a URL and make a GET request to `/api/qr/{url}` or a POST request to `/api/qr` with the base64 encoded URL in the payload. For example:

```bash
curl https://yoururl.com/api/qr -k -X POST -d '{"url":"d3d3LmUtZ2luZWVyaW5nLmNvbS9jb250YWN0LXVz"}' -H 'content-type: application/json'
```

## Deployment

GitHub Actions (`.github/workflows/deploy.yml`) builds every pull request and deploys every push to `main` to the Azure App Service `as-eastus-eg-urlshortener-01`. The workflow signs in to Azure with OpenID Connect, so no Azure credentials are stored in GitHub.

The App Service uses its system-assigned managed identity for both storage and secrets:

- **Storage:** `ConnectionStrings:AzureStorage` in `appsettings.json` is the table endpoint URL, `https://saeastusegurlshortener01.table.core.windows.net`. This is not a secret. When the setting is a URL, the app signs in to Table Storage with its managed identity instead of using an account key. Locally, `appsettings.Development.json` sets it to a connection string, such as `UseDevelopmentStorage=true` for Azurite.
- **Security token:** the App Service app setting `SecurityToken` is a Key Vault reference to the secret `SecurityToken` in `kv-eg-urlshortener-01`. To change the token, update it in Key Vault and restart the App Service.

Other non-secret settings (such as `QRCodeTheme`) are also in `appsettings.json`.

### One-time setup

These commands create the setup. Run them in bash. Sign in to the Azure CLI and the GitHub CLI first.

```bash
RG=rg-urlshortener-eastus-01
APP=as-eastus-eg-urlshortener-01
KV=kv-eg-urlshortener-01
STORAGE=saeastusegurlshortener01
REPO=e-gineering/urlshortener
SUB=$(az account show --query id -o tsv)
TENANT=$(az account show --query tenantId -o tsv)

# Key Vault with RBAC, and permission for you to write secrets
az keyvault create -g $RG -n $KV -l eastus --enable-rbac-authorization true
KV_ID=$(az keyvault show -n $KV --query id -o tsv)
az role assignment create --assignee $(az ad signed-in-user show --query id -o tsv) \
  --role "Key Vault Secrets Officer" --scope $KV_ID

# Secret (enter the value at the prompt so it is not saved in shell history)
read -rsp "Security token: " V; az keyvault secret set --vault-name $KV -n SecurityToken --value "$V" -o none; echo

# App Service managed identity can read the secret and the URL table
PRINCIPAL=$(az webapp identity assign -g $RG -n $APP --query principalId -o tsv)
az role assignment create --assignee-object-id $PRINCIPAL --assignee-principal-type ServicePrincipal \
  --role "Key Vault Secrets User" --scope $KV_ID
az role assignment create --assignee-object-id $PRINCIPAL --assignee-principal-type ServicePrincipal \
  --role "Storage Table Data Contributor" \
  --scope $(az storage account show -g $RG -n $STORAGE --query id -o tsv)
az webapp config appsettings set -g $RG -n $APP --settings \
  "SecurityToken=@Microsoft.KeyVault(VaultName=$KV;SecretName=SecurityToken)"

# Identity for GitHub Actions. Azure accepts it only from the "production" environment of this repo.
CLIENT_ID=$(az ad app create --display-name sp-eg-urlshortener-github-deploy --query appId -o tsv)
az ad sp create --id $CLIENT_ID
az ad app federated-credential create --id $CLIENT_ID --parameters "{
  \"name\": \"github-urlshortener-production\",
  \"issuer\": \"https://token.actions.githubusercontent.com\",
  \"subject\": \"repo:$REPO:environment:production\",
  \"audiences\": [\"api://AzureADTokenExchange\"]
}"
az role assignment create --assignee $CLIENT_ID --role "Website Contributor" \
  --scope $(az webapp show -g $RG -n $APP --query id -o tsv)

# GitHub environment and variables used by the workflow.
# The variables are set on the environment, so only the deploy job can read them.
gh api -X PUT repos/$REPO/environments/production
gh variable set AZURE_CLIENT_ID -R $REPO --env production -b $CLIENT_ID
gh variable set AZURE_TENANT_ID -R $REPO --env production -b $TENANT
gh variable set AZURE_SUBSCRIPTION_ID -R $REPO --env production -b $SUB

# Remove your own write access to secrets when you are done
az role assignment delete --assignee $(az ad signed-in-user show --query id -o tsv) \
  --role "Key Vault Secrets Officer" --scope $KV_ID
```

### Identities and access

| Identity | Name | Access |
| --- | --- | --- |
| GitHub Actions deploy | App registration `sp-eg-urlshortener-github-deploy` | `Website Contributor` on `as-eastus-eg-urlshortener-01` only. Azure accepts its sign-in only from the `production` environment of `e-gineering/urlshortener`. It has no client secret. |
| App Service | System-assigned managed identity of `as-eastus-eg-urlshortener-01` | `Key Vault Secrets User` on `kv-eg-urlshortener-01` and `Storage Table Data Contributor` on `saeastusegurlshortener01`. Azure deletes it when the App Service is deleted. |

## Project Structure

- **Egineering.UrlShortener** - Main web application (ASP.NET Core)
- **Egineering.UrlShortener.Services** - Business logic and Azure Table Storage service
- **wwwroot** - Static files (HTML, CSS, JavaScript)

## Technology Stack

- **.NET 10** - Runtime and framework
- **ASP.NET Core** - Web framework
- **Azure Table Storage** - URL mapping storage
- **Material Components for the Web** - UI components
- **QRCoder** - QR code generation (assumed from QR functionality)

## Troubleshooting

### Socket Access Error / Port Restrictions

If you encounter "An attempt was made to access a socket in a way forbidden by its access permissions":

**Windows users**: This is caused by Hyper-V reserving port ranges. Use the explicit port configuration:

```bash
dotnet run --project Egineering.UrlShortener/Egineering.UrlShortener.csproj --urls "http://localhost:8080;https://localhost:8081"
```

To check which ports are reserved on your system:
```bash
netsh interface ipv4 show excludedportrange protocol=tcp
```

### Azure Storage Connection Issues

If you see connection errors, verify:
- Your connection string is correctly configured
- Azure Storage account is accessible
- For local development, Azurite emulator is running

### Missing Dependencies

Run `dotnet restore` to ensure all NuGet packages are installed.
