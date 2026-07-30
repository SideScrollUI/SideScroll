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
		HttpClient client = GetClient(clientConfig);
		CancellationToken cancelToken = Call.TaskInstance?.CancelToken ?? default;

		Exception? lastException = null;
		for (int attempt = 1; ; attempt++)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, uri);

			try
			{
				using HttpResponseMessage response = await client.SendAsync(request, cancelToken);

				// Don't return error responses, HttpCachedCall would cache them permanently
				response.EnsureSuccessStatusCode();

				Stream dataStream = await response.Content.ReadAsStreamAsync(cancelToken);

				MemoryStream memoryStream = new();
				await dataStream.CopyToAsync(memoryStream, cancelToken);
				byte[] data = memoryStream.ToArray();
				dataStream.Close();

				getCall.Log.Add("Downloaded HTTP File",
					new Tag("URI", request.RequestUri),
					new Tag("Size", memoryStream.Length));

				return data;
			}
			catch (HttpRequestException exception)
			{
				getCall.Log.AddError("URI request failed",
					new Tag("URI", request.RequestUri),
					new Tag("Status", exception.StatusCode),
					new Tag("Attempt", attempt),
					new Tag("Message", exception.Message));
				lastException = exception;

				// Only rethrow if the error is permanent (e.g. 404), allow transient errors (e.g. 503) to retry
				if (exception.StatusCode != null && !HttpUtils.IsTransient(exception.StatusCode))
					throw;
			}
			catch (IOException exception)
			{
				getCall.Log.AddError("URI response failed while reading",
					new Tag("URI", request.RequestUri),
					new Tag("Attempt", attempt),
					new Tag("Message", exception.Message));
				lastException = exception;
			}
			catch (TaskCanceledException exception) when (!cancelToken.IsCancellationRequested) // Timed out
			{
				getCall.Log.AddError("URI request timed out",
					new Tag("URI", request.RequestUri),
					new Tag("Attempt", attempt),
					new Tag("Message", exception.Message));
				lastException = exception;
			}

			if (attempt >= MaxAttempts)
				break;

			await Task.Delay(SleepMilliseconds * attempt, cancelToken);
		}

		string message = "HTTP request failed " + MaxAttempts + " times: " + uri;

		// Keep the status code, callers check it to tell a retried 503 apart from a network failure
		if (lastException is HttpRequestException httpException)
		{
			throw new HttpRequestException(message, httpException, httpException.StatusCode);
		}

		throw new Exception(message, lastException);
	}

	/// <summary>Returns the HTTP client used for a request.</summary>
	protected virtual HttpClient GetClient(HttpClientConfig clientConfig)
	{
		return HttpClientManager.GetClient(clientConfig);
	}
}
