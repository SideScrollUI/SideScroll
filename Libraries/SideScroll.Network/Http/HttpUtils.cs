using SideScroll.Attributes;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace SideScroll.Network.Http;

/// <summary>
/// Static helpers for making HTTP GET and HEAD requests with automatic retries,
/// optional download-progress reporting, and structured call logging.
/// </summary>
public static class HttpUtils
{
	/// <summary>Gets or sets the read buffer size in bytes used when streaming content with progress reporting.</summary>
	public static int ReadBufferSize { get; set; } = 100_000;

	/// <summary>Gets or sets the maximum number of retry attempts for a request before returning <c>null</c>.</summary>
	public static int MaxAttempts { get; set; } = 5;

	/// <summary>Gets or sets the base delay between retry attempts; doubled on each subsequent attempt.</summary>
	public static TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromMilliseconds(500); // < ^ MaxAttempts

	/// <summary>Gets or sets the shared <see cref="HttpClient"/> used for HEAD requests.</summary>
	/// <remarks>
	/// Uses the same client as the GET requests so both follow the same redirect policy,
	/// otherwise a HEAD would report the redirect target's status and a GET the redirect itself
	/// </remarks>
	public static HttpClient Client { get; set; } = HttpClientManager.GetClient(new HttpClientConfig());

	/// <summary>Gets or sets the encoding used to decode response bodies.</summary>
	public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

	private const char ByteOrderMark = (char)0xFEFF;

	/// <summary>Decodes response bytes as text using <see cref="DefaultEncoding"/>.</summary>
	/// <remarks>
	/// Responses are usually UTF-8, decoding them as ASCII replaces every byte over 0x7F with a '?'
	/// </remarks>
	public static string DecodeString(byte[] bytes)
	{
		string text = DefaultEncoding.GetString(bytes);

		// A byte order mark decodes to a zero width character that breaks parsers like JsonSerializer
		return text.TrimStart(ByteOrderMark);
	}

	/// <summary>Tracks download progress for a streaming HTTP GET request.</summary>
	public class HttpGetProgress
	{
		/// <summary>Gets or sets the number of bytes downloaded so far.</summary>
		public long Downloaded { get; set; }

		/// <summary>Gets or sets the total content length in bytes.</summary>
		public long TotalLength { get; set; }

		/// <summary>Gets the download completion percentage (0–100).</summary>
		public double Percent => TotalLength > 0 ? 100.0 * Downloaded / TotalLength : 0;
	}

	/// <summary>Synchronously fetches <paramref name="uri"/> and returns the response body as text, or <c>null</c> on failure.</summary>
	public static string? GetString(Call call, string uri)
	{
		return Task.Run(() => GetStringAsync(call, uri)).GetAwaiter().GetResult();
	}

	/// <summary>Asynchronously fetches <paramref name="uri"/> and returns the response body as text, or <c>null</c> on failure.</summary>
	public static async Task<string?> GetStringAsync(Call call, string uri)
	{
		var response = await GetBytesAsync(call, uri);
		if (response?.Response?.IsSuccessStatusCode != true) return null;

		byte[]? bytes = response?.Bytes;
		if (bytes == null) return null;

		return DecodeString(bytes);
	}

	/// <summary>Synchronously fetches <paramref name="uri"/> and returns a <see cref="ViewHttpResponse"/>, or <c>null</c> on failure.</summary>
	public static ViewHttpResponse? GetBytes(Call call, string uri, TimeSpan? timeout = null, IProgress<HttpGetProgress>? progress = null)
	{
		return Task.Run(() => GetBytesAsync(call, uri, timeout, progress)).GetAwaiter().GetResult();
	}

	/// <summary>Asynchronously fetches <paramref name="uri"/> and returns a <see cref="ViewHttpResponse"/>, or <c>null</c> on failure after all retry attempts.</summary>
	public static async Task<ViewHttpResponse?> GetBytesAsync(Call call, string uri, TimeSpan? timeout = null, IProgress<HttpGetProgress>? progress = null)
	{
		using CallTimer getCall = call.Timer("Get Uri", new Tag("Uri", uri));

		HttpClientConfig clientConfig = new()
		{
			Timeout = timeout,
		};
		HttpClient client = clientConfig.IsDefault ? Client : HttpClientManager.GetClient(clientConfig);
		CancellationToken cancelToken = call.TaskInstance?.CancelToken ?? default;

		for (int attempt = 1; attempt <= MaxAttempts; attempt++)
		{
			HttpResponseMessage? response = null;
			try
			{
				if (attempt > 1)
				{
					// The first retry waits the base delay, doubling for each one after
					await Task.Delay(BaseRetryDelay * Math.Pow(2, attempt - 2), cancelToken);
				}

				Stopwatch stopwatch = Stopwatch.StartNew();
				response = await client.GetAsync(uri, cancelToken);

				if (IsTransient(response.StatusCode) && attempt < MaxAttempts)
				{
					getCall.Log.Add($"Transient error {response.StatusCode} on attempt {attempt}");
					continue;
				}

				byte[] bytes = await ReadContentAsync(response.Content, progress, cancelToken);

				stopwatch.Stop();

				ViewHttpResponse viewResponse = new()
				{
					Uri = uri,
					Filename = response.RequestMessage!.RequestUri!.Segments.Last(),
					Milliseconds = stopwatch.ElapsedMilliseconds,
					Bytes = bytes,
					Response = response,
				};

				//response.Close(); // We want the Headers still (might need to copy them elsewhere if this causes problems)
				call.Log.Add("Uri Response",
					new Tag("Uri", response.RequestMessage.RequestUri),
					new Tag("Size", bytes.Length));

				response = null; // Ownership transfers to the returned ViewHttpResponse
				return viewResponse;
			}
			catch (HttpRequestException exception)
			{
				getCall.Log.Add(exception);

				if (exception.StatusCode != null && (!IsTransient(exception.StatusCode) || attempt >= MaxAttempts))
					break;
			}
			catch (TaskCanceledException exception) // Timed out
			{
				getCall.Log.Add(exception);
				if (cancelToken.IsCancellationRequested)
					break;
			}
			finally
			{
				response?.Dispose();
			}
		}
		return null;
	}

	private static async Task<byte[]> ReadContentAsync(
		HttpContent content,
		IProgress<HttpGetProgress>? progress,
		CancellationToken cancelToken)
	{
		if (content.Headers.ContentLength == null || progress == null)
		{
			return await content.ReadAsByteArrayAsync(cancelToken);
		}

		await using var contentStream = await content.ReadAsStreamAsync(cancelToken);
		using var memoryStream = new MemoryStream();

		var buffer = new byte[ReadBufferSize];

		int bytes;
		while ((bytes = await contentStream.ReadAsync(buffer, cancelToken)) > 0)
		{
			memoryStream.Write(buffer, 0, bytes);
			progress.Report(new HttpGetProgress
			{
				Downloaded = memoryStream.Position,
				TotalLength = content.Headers.ContentLength.Value,
			});
		}

		return memoryStream.ToArray();
	}

	/// <summary>Synchronously sends an HTTP HEAD request to <paramref name="uri"/> and returns the response, or <c>null</c> on failure.</summary>
	public static HttpResponseMessage? GetHead(Call call, string uri)
	{
		return Task.Run(() => GetHeadAsync(call, uri)).GetAwaiter().GetResult();
	}

	/// <summary>Asynchronously sends an HTTP HEAD request to <paramref name="uri"/> and returns the response, or <c>null</c> on failure after all retry attempts.</summary>
	public static async Task<HttpResponseMessage?> GetHeadAsync(Call call, string uri)
	{
		using CallTimer headCall = call.Timer("Head Uri", new Tag("Uri", uri));
		CancellationToken cancelToken = call.TaskInstance?.CancelToken ?? default;

		for (int attempt = 1; attempt <= MaxAttempts; attempt++)
		{
			HttpRequestMessage? request = null;
			HttpResponseMessage? response = null;
			try
			{
				if (attempt > 1)
				{
					// The first retry waits the base delay, doubling for each one after
					await Task.Delay(BaseRetryDelay * Math.Pow(2, attempt - 2), cancelToken);
				}

				request = new HttpRequestMessage(HttpMethod.Head, uri);
				response = await Client.SendAsync(request, cancelToken);

				if (IsTransient(response.StatusCode) && attempt < MaxAttempts)
				{
					headCall.Log.Add($"Transient error {response.StatusCode} on attempt {attempt}");
					continue;
				}

				//response.Close();
				call.Log.Add("Uri Response",
					new Tag("Uri", request.RequestUri),
					new Tag("Response", response));

				HttpResponseMessage result = response;
				request = null; // Kept by response.RequestMessage for the caller
				response = null; // Ownership transfers to the caller
				return result;
			}
			catch (HttpRequestException exception)
			{
				headCall.Log.Add(exception);

				if (exception.StatusCode != null && (!IsTransient(exception.StatusCode) || attempt >= MaxAttempts))
					break;
			}
			catch (TaskCanceledException exception) // Timed out
			{
				headCall.Log.Add(exception);
				if (cancelToken.IsCancellationRequested)
					break;
			}
			finally
			{
				response?.Dispose();
				request?.Dispose();
			}
		}
		return null;
	}

	public static bool IsTransient(HttpStatusCode? statusCode)
	{
		return statusCode is 
			HttpStatusCode.RequestTimeout or 
			HttpStatusCode.TooManyRequests or 
			HttpStatusCode.InternalServerError or 
			HttpStatusCode.BadGateway or 
			HttpStatusCode.ServiceUnavailable or 
			HttpStatusCode.GatewayTimeout;
	}
}

/// <summary>Captures the result of an HTTP GET request including status, headers, raw bytes, and elapsed time.</summary>
public class ViewHttpResponse : IDisposable
{
	/// <summary>Gets or sets the request URI.</summary>
	[HiddenColumn]
	public string? Uri { get; set; }

	/// <summary>Gets or sets the filename extracted from the last URI path segment.</summary>
	public string? Filename { get; set; }

	/// <summary>Gets the response body decoded as text.</summary>
	[HiddenColumn]
	public string Body => HttpUtils.DecodeString(Bytes!);

	/// <summary>Gets the HTTP status code of the response.</summary>
	public HttpStatusCode? Status => Response?.StatusCode;

	/// <summary>Gets or sets the raw response bytes.</summary>
	[HiddenRow]
	public byte[]? Bytes { get; set; }

	/// <summary>Gets or sets the elapsed time of the request in milliseconds.</summary>
	public double Milliseconds { get; set; }

	/// <summary>Gets or sets an optional parsed view object derived from the response body.</summary>
	[HiddenColumn, Hide(null)]
	public object? View { get; set; }

	/// <summary>Gets or sets the underlying <see cref="HttpResponseMessage"/> from the request.</summary>
	[HiddenColumn]
	public HttpResponseMessage? Response { get; set; }

	/// <summary>Returns the response <see cref="Filename"/>.</summary>
	public override string? ToString() => Filename;

	/// <summary>Initializes an empty <see cref="ViewHttpResponse"/>.</summary>
	public ViewHttpResponse() { }

	/// <summary>Initializes a <see cref="ViewHttpResponse"/> from an existing response message and byte payload.</summary>
	public ViewHttpResponse(HttpResponseMessage response, byte[] bytes)
	{
		Response = response;
		Bytes = bytes;
	}

	/// <summary>Releases the owned HTTP response.</summary>
	public void Dispose()
	{
		Response?.Dispose();
		Response = null;
		GC.SuppressFinalize(this);
	}
}
