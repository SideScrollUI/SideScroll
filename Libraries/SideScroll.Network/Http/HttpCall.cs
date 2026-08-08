namespace SideScroll.Network.Http;

/// <summary>
/// A simple HTTP GET helper that retries failed requests up to <see cref="MaxAttempts"/> times
/// with an exponential-style delay between attempts.
/// </summary>
public class HttpCall(Call call)
{
	/// <summary>Gets or sets the maximum number of download attempts before throwing.</summary>
	public static int MaxAttempts
	{
		get => _maxAttempts;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, nameof(MaxAttempts));
			_maxAttempts = value;
		}
	}
	private static int _maxAttempts = 4;

	/// <summary>Gets or sets the base sleep duration in milliseconds between retry attempts.</summary>
	public static int SleepMilliseconds
	{
		get => _sleepMilliseconds;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(SleepMilliseconds));
			_sleepMilliseconds = value;
		}
	}
	private static int _sleepMilliseconds = 500; // < ^ MaxAttempts

	/// <summary>Gets the logging call context used for timing and diagnostics.</summary>
	public Call Call => call;

	/// <summary>Fetches <paramref name="uri"/> and returns the decoded response body.</summary>
	public virtual async Task<string?> GetStringAsync(string uri, string? accept = null)
	{
		byte[] bytes = await GetResponseAsync(uri, accept);
		return HttpUtils.DecodeString(bytes);
	}

	/// <summary>Fetches <paramref name="uri"/> and returns the raw response bytes.</summary>
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
			using var request = new HttpRequestMessage(HttpMethod.Get, uri);

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
			catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
			{
				getCall.Log.AddError("URI request failed",
					new Tag("URI", request.RequestUri),
					new Tag("Attempt", attempt),
					new Tag("Message", exception.Message));
			}

			if (attempt >= MaxAttempts)
				break;

			// Multiply as a long and clamp. The setter allows any non negative value, so an int
			// product can wrap negative, where Task.Delay treats -1 as an infinite wait, and
			// anything past int.MaxValue milliseconds throws
			int delayMilliseconds = (int)Math.Min((long)SleepMilliseconds * attempt, int.MaxValue);
			await Task.Delay(delayMilliseconds);
		}
		throw new Exception("HTTP request failed " + MaxAttempts + " times: " + uri);
	}
}
