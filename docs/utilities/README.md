# Utilities

Use `client.Utilities` for service health, request-header diagnostics, and
authenticated account information.

| Operation | Method | Authentication |
|---|---|---|
| Service status | `GetStatusAsync` | Not required |
| Request headers | `GetHeadersAsync` | Not required |
| User quota and entitlements | `GetUserAsync` | Required |

```csharp
var status = await client.Utilities.GetStatusAsync(cancellationToken);
foreach (var service in status.Values)
{
    Console.WriteLine($"{service.Service}: {service.Status}");
}

var headers = await client.Utilities.GetHeadersAsync(cancellationToken);
Console.WriteLine($"Request ID: {headers.RequestId}");
```

Utilities are implemented by the SDK but are currently not listed in the live schema.

