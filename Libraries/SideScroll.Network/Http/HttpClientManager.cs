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

	private static readonly HttpClient _defaultClient = new(_handler);

	private static readonly Dictionary<string, HttpClient> _clients = [];

	/// <summary>Returns a shared <see cref="HttpClient"/> matching the given <paramref name="config"/>, creating and caching a new one if needed.</summary>
	public static HttpClient GetClient(HttpClientConfig config)
	{
		if (config.IsDefault) return _defaultClient;

		lock (_clients)
		{
			string id = config.ToString();
			if (_clients.TryGetValue(id, out HttpClient? client)) return client;

			client = new HttpClient(_handler);

			if (config.Accept != null)
			{
				client.DefaultRequestHeaders.Add("Accept", config.Accept);
			}

			if (config.Timeout is { } timeout)
			{
				client.Timeout = timeout;
			}

			_clients[id] = client;
			return client;
		}
	}
}
