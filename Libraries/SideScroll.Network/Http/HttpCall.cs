using System.Net;
using System.Text;

namespace SideScroll.Network.Http;

public class HttpCall(Call call)
{
	private const int MaxAttempts = 4;
	private const int SleepMilliseconds = 500; // < ^ MaxAttempts

	public Call Call = call;

	public virtual async Task<string?> GetStringAsync(string uri, string? accept = null)
	{
		byte[] bytes = await GetResponseAsync(uri, accept);
		if (bytes != null)
			return Encoding.ASCII.GetString(bytes);
		return null;
	}

	public virtual async Task<byte[]> GetBytesAsync(string uri)
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

				Stream dataStream = await response.Content.ReadAsStreamAsync();

				MemoryStream memoryStream = new();
				await dataStream.CopyToAsync(memoryStream);
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
					string response = await new StreamReader(exception.Response.GetResponseStream()).ReadToEndAsync();
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
