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
	/// <summary>
	/// Gets or sets how many configured clients are cached. Both parts of the key come from the
	/// caller, and <see cref="HttpClientConfig.Timeout"/> in particular has unlimited distinct
	/// values, so the pool is capped instead of growing for the life of the process.
	/// Configurations past the cap still get a working client, it just isn't reused
	/// </summary>
	public static int MaxClients
	{
		get => _maxClients;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(MaxClients));
			_maxClients = value;
		}
	}
	private static int _maxClients = 32;

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

			client = CreateClient(config);

			// Evicting a cached client would hand a second one to callers still using the first,
			// so stop caching at the cap instead. The uncached client shares the pooled handler,
			// owns nothing unmanaged, and is collected once the caller is done with it
			if (_clients.Count < MaxClients)
			{
				_clients[config] = client;
			}
			return client;
		}
	}

	private static HttpClient CreateClient(HttpClientConfig config)
	{
		// Shared handlers shouldn't be disposed by any single client instance
		HttpClient client = new(_handler, disposeHandler: false);

		if (config.Accept != null)
		{
			client.DefaultRequestHeaders.Add("Accept", config.Accept);
		}

		if (config.Timeout is { } timeout)
		{
			client.Timeout = timeout;
		}

		return client;
	}
}
