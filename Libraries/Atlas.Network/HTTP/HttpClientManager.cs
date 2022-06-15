using System.Collections.Generic;
using System.Net.Http;

namespace Atlas.Network;

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

	public static HttpClient GetClient(string accept = null)
	{
		if (accept == null) return _defaultClient;

		lock (_clients)
		{
			if (_clients.TryGetValue(accept, out HttpClient client)) return client;

			client = new HttpClient(_handler);
			client.DefaultRequestHeaders.Add("Accept", accept);
			_clients[accept] = client;
			return client;
		}
	}
}
