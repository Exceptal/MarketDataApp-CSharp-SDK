using System.Text;

namespace MarketData;

/// <summary>
/// Typed response returned by every Market Data SDK endpoint.
/// </summary>
/// <typeparam name="T">Type of the decoded data payload.</typeparam>
public abstract record MarketDataResponse<T>
{
    /// <summary>Decoded data payload.</summary>
    public T Values { get; internal init; } = default!;

    /// <summary>HTTP status code of the response.</summary>
    public int StatusCode { get; internal init; }

    /// <summary>URL that was requested.</summary>
    public Uri RequestUrl { get; internal init; } = null!;

    /// <summary>Server-assigned request identifier, or <c>null</c> when not returned.</summary>
    public string? RequestId { get; internal init; }

    /// <summary>Rate-limit information from this response's headers.</summary>
    public RateLimitSnapshot? RateLimit { get; internal init; }

    // Raw response bytes — internal so the transport can populate it; not exposed on the
    // public interface (callers use RawBody or SaveToFile).
    internal byte[] RawBodyBytes { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Whether the API returned no data for the request. When <c>true</c>, <see cref="Values"/>
    /// is an empty collection (or empty string).
    /// </summary>
    public bool IsNoData => StatusCode == 404;

    /// <summary>The raw response body decoded as a UTF-8 string.</summary>
    public string RawBody => Encoding.UTF8.GetString(RawBodyBytes);

    /// <summary>Writes the raw response body to <paramref name="path"/>.</summary>
    public void SaveToFile(string path) => File.WriteAllBytes(path, RawBodyBytes);
}
