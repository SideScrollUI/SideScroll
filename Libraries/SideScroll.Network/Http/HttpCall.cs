namespace SideScroll.Network.Http;

/// <summary>
/// A simple HTTP GET helper that retries failed requests up to <see cref="MaxAttempts"/> times
/// with an exponential-style delay between attempts.
/// </summary>
public class HttpCall(Call call)
{
	/// <summary>Gets or sets the maximum number of download attempts before throwing.</summary>
	public static int MaxAttempts { get; set; } = 4;

	/// <summary>Gets or sets the base sleep duration in milliseconds between retry attempts.</summary>
	public static int SleepMilliseconds { get; set; } = 500; // < ^ MaxAttempts

	/// <summary>Gets the logging call context used for timing and diagnostics.</summary>
	public Call Call => call;

	/// <summary>Fetches <paramref name="uri"/> and returns the response body as text.</summary>
	public virtual async Task<string?> GetStringAsync(string uri, string? accept = null)
	{
		byte[] bytes = await GetResponseAsync(uri, accept);
		return HttpUtils.DecodeString(bytes);
	}

	/// <summary>Fetches <paramref name="uri"/> and returns the raw response bytes.</summary>
	public virtual async Task<byte[]> GetBytesAsync(string uri, string? accept = null)
	{
		return await GetResponseAsync(uri, accept);
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

				// Don't return error responses, HttpCachedCall would cache them permanently
				response.EnsureSuccessStatusCode();

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
			catch (HttpRequestException exception)
			{
				getCall.Log.AddError("URI request " + request.RequestUri + " failed: " + exception.Message);

				// Status codes won't change between attempts, rethrow so the caller sees which one
				if (exception.StatusCode != null)
					throw;
			}
			catch (TaskCanceledException exception) // Timed out
			{
				getCall.Log.AddError("URI request " + request.RequestUri + " timed out: " + exception.Message);
			}

			if (attempt >= MaxAttempts)
				break;

			await Task.Delay(SleepMilliseconds * attempt);
		}
		throw new Exception("HTTP request failed " + MaxAttempts + " times: " + uri);
	}
}
