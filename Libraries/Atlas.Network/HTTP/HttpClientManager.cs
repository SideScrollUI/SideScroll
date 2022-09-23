namespace Atlas.Network;


public record HttpClientConfig(string? Accept = null, TimeSpan? Timeout = null)
{
	public bool IsDefault => Accept == null && Timeout == null;
}

// todo: Figure out a way to reuse default client and inject in request
// alternative: HttpClientFactory
public static class HttpClientManager
{
	private static readonly HttpClientHandler _handler = new()
	{
		AllowAutoRedirect = false,
	};

	private static readonly HttpClient _defaultClient = new(_handler);

	private static readonly Dictionary<string, HttpClient> _clients = new();

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

			if (config.Timeout != null)
			{
				client.Timeout = config.Timeout.Value;
			}

			_clients[id] = client;
			return client;
		}
	}
}
