using Atlas.Core;
using System.Net;
using System.Text;

namespace Atlas.Network;

public class HttpCall
{
	private const int MaxAttempts = 4;
	private const int SleepMilliseconds = 500; // < ^ MaxAttempts

	public Call Call;

	public HttpCall(Call call)
	{
		Call = call;
	}

	public async virtual Task<string?> GetStringAsync(string uri, string? accept = null)
	{
		byte[] bytes = await GetResponseAsync(uri, accept);
		if (bytes != null)
			return Encoding.ASCII.GetString(bytes);
		return null;
	}

	public async virtual Task<byte[]> GetBytesAsync(string uri)
	{
		return await GetResponseAsync(uri);
	}

	private async Task<byte[]> GetResponseAsync(string uri, string? accept = null)
	{
		using CallTimer getCall = Call.Timer("Get Uri", new Tag("URI", uri));

		HttpClientConfig clientConfig = new()
		{
			Accept = accept,
		};
		HttpClient client = HttpClientManager.GetClient(clientConfig);

		for (int attempt = 1; ; attempt++)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, uri);

			try
			{
				using HttpResponseMessage response = await client.SendAsync(request);

				Stream dataStream = response.Content.ReadAsStream();

				MemoryStream memoryStream = new();
				dataStream.CopyTo(memoryStream);
				byte[] data = memoryStream.ToArray();
				dataStream.Close();

				getCall.Log.Add("Downloaded HTTP File", 
					new Tag("URI", request.RequestUri), 
					new Tag("Size", memoryStream.Length));

				return data;
			}
			catch (WebException exception)
			{
				getCall.Log.AddError("URI request " + request.RequestUri + " failed: " + exception.Message);

				if (exception.Response != null)
				{
					string response = new StreamReader(exception.Response.GetResponseStream()).ReadToEnd();
					Call.Log.AddError(response);
				}
			}

			if (attempt >= MaxAttempts)
				break;

			await Task.Delay(SleepMilliseconds * attempt);
		}
		throw new Exception("HTTP request failed " + MaxAttempts + " times: " + uri);
	}
}
