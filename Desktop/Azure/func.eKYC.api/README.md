# DGC.eKYC

A .NET 8.0 Web API solution for eKYC services.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server
- Redis

## Project Structure

- **DGC.eKYC.Api**: The main Web API entry point.
- **DGC.eKYC.Business**: Business logic layer.
- **DGC.eKYC.Dal**: Data Access Layer (Entity Framework Core).
- **DGC.eKYC.Deeplink**: Deeplink handling logic.

## Configuration

Before running the application, ensure you have configured your `appsettings.json` or environment variables with the necessary connection strings and settings.

### Required Connection Strings
- `DefaultDbConnection`: Connection string for SQL Server.
- `DefaultRedisConnection`: Connection string for Redis.

### Required Settings
The following sections must be configured:
- `PotApi` (BaseUri)
- `Logger` (BaseUri)
- `HuaweiRrEKycVendor` (Uri)

## How to Run Locally

1. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

2. **Run the API**
   Navigate to the solution root and run:
   ```bash
   dotnet run --project DGC.eKYC.Api/DGC.eKYC.Api.csproj
   ```

   The application will start on:
   - HTTPS: `https://localhost:7270`
   - HTTP: `http://localhost:5153`

## Docker Support

You can also run the application using Docker.

1. **Build the Image**
   Run the following command from the solution root:
   ```bash
   docker build -t dgc-ekyc-api -f DGC.eKYC.Api/Dockerfile .
   ```

2. **Run the Container**
   ```bash
   docker run -p 8080:8080 -p 8081:8081 dgc-ekyc-api
   ```
