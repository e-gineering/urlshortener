# URL Shortener

A .NET 8 web application for shortening URLs with QR code generation capabilities, built with ASP.NET Core and Azure Table Storage.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
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
- **Swagger UI**: `http://localhost:8080/swagger` (Development mode only)

## API Usage

### QR Codes

To generate a QR code, base64 encode a URL and make a GET request to `/api/qr/{url}` or a POST request to `/api/qr` with the base64 encoded URL in the payload. For example:

```bash
curl https://yoururl.com/api/qr -k -X POST -d '{"url":"d3d3LmUtZ2luZWVyaW5nLmNvbS9jb250YWN0LXVz"}' -H 'content-type: application/json'
```

## Project Structure

- **Egineering.UrlShortener** - Main web application (ASP.NET Core)
- **Egineering.UrlShortener.Services** - Business logic and Azure Table Storage service
- **wwwroot** - Static files (HTML, CSS, JavaScript)

## Technology Stack

- **.NET 8** - Runtime and framework
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
