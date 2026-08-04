# Copilot instructions for MarketDataApp C# SDK

This file helps Copilot-based sessions understand how to build, test, and reason about this repository.

---

## Build, test, and lint commands

- SDK requirement: dotnet 10 (see `global.json` - sdk version 10.0.302).
- Build solution: `dotnet build` (or `dotnet build MarketData.slnx`).
- Run full tests: `dotnet test` from repo root.
- Run tests for the test project directly:
  `dotnet test src/MarketData.Tests/MarketData.Tests/MarketData.Tests.csproj`
- Run a single test by FullyQualifiedName:
  `dotnet test --filter "FullyQualifiedName=MarketData.Tests.Namespaces.ClassName.TestMethodName"`
  or by method name substring:
  `dotnet test --filter "Name~TestMethodName"`
- Collect code coverage: `dotnet test --collect:"XPlat Code Coverage"` (project already references coverlet.collector).
- Create NuGet package: `dotnet pack src/MarketData/MarketData.csproj -c Release`.

Notes:
- The library sets `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in the library project; fix warnings or the build will fail.

---

## High-level architecture (big picture)

- MarketDataClient (entry point) composes an internal ApiClient and exposes five API surfaces:
  - UtilitiesApi, MarketsApi, StocksApi, FundsApi, OptionsApi.
- ApiClient is the HTTP transport layer: builds URIs, sends GET requests, handles timeouts, parses rate-limit headers and implements retry/backoff logic.
- Each API surface translates request objects into query parameters, calls ApiClient.GetAsync, and uses JsonResponseParser to decode JSON or CsvResponse for CSV endpoints.
- Data model:
  - Requests: immutable record types (e.g., StockPricesRequest) that validate arguments in constructors.
  - Responses: typed response records deriving from MarketDataResponse<T>. Responses include metadata (StatusCode, RequestUrl, RequestId, RateLimit) and expose RawBody and SaveToFile.
- Error handling: a sealed exception hierarchy under MarketData.Exceptions (AuthenticationException, RateLimitException, ServerException, ParseException, NetworkException, etc.) with ErrorContext attached.
- Tests: xUnit-based project (src/MarketData.Tests/MarketData.Tests). Tests use a lightweight StubHttpMessageHandler and MarketDataTestClient helpers to create deterministic HttpClient-backed MarketDataClient instances.

---

## Key conventions and repository-specific patterns

- Async naming: all network operations are `XxxAsync` and return typed response objects (e.g., `GetPricesAsync` returns `StockPricesResponse`). CSV endpoints have `Get*CsvAsync` variants returning `CsvResponse`.
- Response creation: use JsonResponseParser.CreateResponse / CreateCsvResponse so RawBodyBytes and metadata are populated consistently.
- No global mutable state: ApiClient exposes the last-seen rate limit via `MarketDataClient.LatestRateLimit` (read-only snapshot).
- Request objects validate inputs in constructors (throw on empty symbols/nulls). Maintain that pattern when adding new requests.
- When modifying transport behavior, preserve ApiClient's retry/backoff and use `MarketDataClientOptions` for configuration. Default config reads from `MARKETDATA_TOKEN`, `MARKETDATA_BASE_URL`, `MARKETDATA_API_VERSION`, `MARKETDATA_USER_AGENT` when using `FromConfiguration`.
- Tests avoid real network calls: prefer injecting a StubHttpMessageHandler or mocking HttpClient. Do not add tests that hit `api.marketdata.app`.

---

## Related files to consult

- src/MarketData/ApiClient.cs (transport, retry, rate-limit parsing)
- src/MarketData/JsonResponseParser.cs (decoding JSON, CSV response creation)
- src/MarketData/MarketDataClient.cs (public surface construction)
- src/MarketData/MarketDataClientOptions.cs (configuration keys)
- src/MarketData/Responses and src/MarketData/*Api.cs (request/response shapes)
- src/MarketData.Tests/TestSupport (StubHttpMessageHandler, MarketDataTestClient)

---

If you want, configure MCP servers relevant to .NET SDK projects (for example, a test runner or coverage server). Would you like an MCP server configured for running tests or coverage? 

Summary: created .github/copilot-instructions.md with build/test commands, architecture overview, and repository-specific conventions. Reply if you want adjustments or additional coverage areas added.
