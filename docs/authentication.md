# Authentication

Most Market Data requests require an API token. Never commit a token, put it in a
sample, or print it in logs. Use user-secrets for local development and an environment
variable or managed secret provider in deployed applications.

## .NET user-secrets

From an executable project:

```powershell
dotnet user-secrets init
dotnet user-secrets set "MARKETDATA_TOKEN" "your-api-token"
```

Load the provider and create options:

```csharp
using MarketData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);

var options = MarketDataClientOptions.FromConfiguration(builder.Configuration);
```

The SDK does not load `.env` files automatically.

## Environment variables

.NET maps a double underscore to a configuration section separator:

```powershell
$env:MARKETDATA_TOKEN = "your-api-token"
dotnet run
```

The equivalent configuration key is `MARKETDATA_TOKEN`. Other supported keys are
`BaseAddress`, `ApiVersion`, `Timeout`, `MaxRetries`, and `MaxConcurrentRequests`.

## Explicit options

Explicit options are useful in tests or short-lived tools. Keep the token outside
source control:

```csharp
var options = new MarketDataClientOptions
{
    ApiToken = Environment.GetEnvironmentVariable("MARKETDATA_TOKEN")
};
```

`ApiToken` may be `null` for unauthenticated/free requests, but authenticated endpoints
can then throw `AuthenticationException`.

