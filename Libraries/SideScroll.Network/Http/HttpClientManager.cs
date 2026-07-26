namespace SideScroll.Network.Http;

/// <summary>Configuration key for a pooled <see cref="HttpClient"/> instance.</summary>
public record HttpClientConfig(string? Accept = null, TimeSpan? Timeout = null)
{
	/// <summary>Gets whether this configuration represents the default client (no Accept header or custom timeout).</summary>
	public bool IsDefault => Accept == null && Timeout == null;
}

// todo: Figure out a way to reuse default client and inject in request
// alternative: HttpClientFactory
/// <summary>
/// Manages a pool of <see cref="HttpClient"/> instances keyed by <see cref="HttpClientConfig"/>,
/// returning the default shared client when no custom settings are needed.
/// </summary>
public static class HttpClientManager
{
	private static readonly HttpClientHandler _handler = new()
	{
		AllowAutoRedirect = false,
	};

	private static readonly HttpClient _defaultClient = new(_handler, disposeHandler: false);

	private static readonly Dictionary<HttpClientConfig, HttpClient> _clients = [];

	/// <summary>Returns a shared <see cref="HttpClient"/> matching the given <paramref name="config"/>, creating and caching a new one if needed.</summary>
	public static HttpClient GetClient(HttpClientConfig config)
	{
		if (config.IsDefault) return _defaultClient;

		lock (_clients)
		{
			if (_clients.TryGetValue(config, out HttpClient? client)) return client;

			// Shared handlers shouldn't be disposed by any single client instance
			client = new HttpClient(_handler, disposeHandler: false);

			if (config.Accept != null)
			{
				client.DefaultRequestHeaders.Add("Accept", config.Accept);
			}

			if (config.Timeout is { } timeout)
			{
				client.Timeout = timeout;
			}

			_clients[config] = client;
			return client;
		}
	}
}
