# Rdt-C-api-project
This is my personal C# project that i made in the RDT work experience. it uses a financial api wich returns stock prices and much information on  stocks.

## Running the solution locally
1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download). The SDK provides the `dotnet` CLI commands used below.
2. (Optional) Trust the ASP.NET Core HTTPS development certificate so the hosted site can run on `https://localhost`:
   ```bash
   dotnet dev-certs https --trust
   ```
3. Restore dependencies and build the solution from the repository root:
   ```bash
   dotnet restore WorkExperienceOct2024.sln
   dotnet build WorkExperienceOct2024.sln
   ```
4. Run the server project, which also serves the Blazor WebAssembly client:
   ```bash
   dotnet run --project WorkExperienceOct2024.Server/WorkExperienceOct2024.Server.csproj
   ```

## Configuration

The server expects an Alpha Vantage API key so it can call the upstream service. Provide it in one of the following ways before running `dotnet run`:

* Add it to `WorkExperienceOct2024.Server/appsettings.Development.json` under the `AlphaVantage:ApiKey` key.
* Store it securely with [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets?view=aspnetcore-8.0):
  ```bash
  cd WorkExperienceOct2024.Server
  dotnet user-secrets init
  dotnet user-secrets set "AlphaVantage:ApiKey" "<your-key>"
  ```
* Export it as an environment variable before launching the app:
  ```bash
  export ALPHAVANTAGE_API_KEY=<your-key>
  ```

## Troubleshooting
### "You intended to execute a .NET SDK command" message
This error appears when the .NET SDK is not installed or not on your system `PATH`. Install the .NET 8 SDK (step 1 above) and then reopen your terminal so it picks up the `dotnet` command. On Windows you may need to restart PowerShell or Command Prompt after installation.
