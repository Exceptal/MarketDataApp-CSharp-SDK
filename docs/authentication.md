# Authentication

Most Market Data requests require an API token. Never commit a token, put it in a
sample, or print it in logs. Use user-secrets for local development and an environment
variable or managed secret provider in deployed applications.

## Configuring API token

You can create a client without passing options:

```csharp
using MarketDataApp;

using var httpClient = new HttpClient();
var client = new MarketDataClient(httpClient);
```

When no options are supplied, the SDK loads `MARKETDATA_*` values from these sources:

1. Environment variables (highest priority)
2. `.env` file in the assembly's working directory
3. .NET user secrets

For example, a local `.env` file can contain:

```dotenv
MARKETDATA_TOKEN=your-api-token
```

Environment variables override values from both `.env` and user secrets. The `.env`
file is intended for local development and should not be committed.

### User secrets

From an executable project:

```powershell
dotnet user-secrets init
dotnet user-secrets set "MARKETDATA_TOKEN" "your-api-token"
```

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
