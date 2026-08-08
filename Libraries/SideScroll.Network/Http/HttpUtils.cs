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
	public static int ReadBufferSize
	{
		get => _readBufferSize;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, nameof(ReadBufferSize));
			_readBufferSize = value;
		}
	}
	private static int _readBufferSize = 100_000;

	/// <summary>Gets or sets the maximum number of retry attempts for a request before returning <c>null</c>.</summary>
	public static int MaxAttempts
	{
		get => _maxAttempts;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, nameof(MaxAttempts));
			_maxAttempts = value;
		}
	}
	private static int _maxAttempts = 5;

	/// <summary>Gets or sets the base delay between retry attempts; doubled on each subsequent attempt.</summary>
	public static TimeSpan BaseRetryDelay
	{
		get => _baseRetryDelay;
		set
		{
			if (value < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(BaseRetryDelay), value, "Retry delay cannot be negative.");

			_baseRetryDelay = value;
		}
	}
	private static TimeSpan _baseRetryDelay = TimeSpan.FromMilliseconds(500); // < ^ MaxAttempts

	/// <summary>Gets or sets the shared <see cref="HttpClient"/> used for HEAD requests.</summary>
	public static HttpClient Client { get; set; } = new();

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
		public double Percent => 100.0 * Downloaded / TotalLength;
	}

	/// <summary>Synchronously fetches <paramref name="uri"/> and returns the decoded response body, or <c>null</c> on failure.</summary>
	public static string? GetString(Call call, string uri)
	{
		return Task.Run(() => GetStringAsync(call, uri)).GetAwaiter().GetResult();
	}

	/// <summary>Asynchronously fetches <paramref name="uri"/> and returns the decoded response body, or <c>null</c> on failure.</summary>
	public static async Task<string?> GetStringAsync(Call call, string uri)
	{
		// Disposed here, GetBytesAsync() transfers ownership of the response message into it
		using ViewHttpResponse? response = await GetBytesAsync(call, uri);

		// Null rather than throwing, which is what this returns for every other failure
		if (response?.Response?.IsSuccessStatusCode != true) return null;

		byte[]? bytes = response.Bytes;
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
		HttpClient client = HttpClientManager.GetClient(clientConfig);
		CancellationToken cancelToken = call.TaskInstance?.CancelToken ?? default;

		for (int attempt = 1; attempt <= MaxAttempts; attempt++)
		{
			if (attempt > 1)
			{
				await Task.Delay(BaseRetryDelay * Math.Pow(2, attempt - 2), cancelToken);
			}

			// Owned here until it's handed to the returned ViewHttpResponse. Reading the content can
			// fail after the request succeeded, and the retry below would otherwise abandon it
			HttpResponseMessage? response = null;
			try
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				response = await client.GetAsync(uri, cancelToken);

				// The finally below disposes it, this doesn't hand ownership on
				if (IsTransient(response.StatusCode) && attempt < MaxAttempts)
				{
					getCall.Log.Add("Transient error, retrying",
						new Tag("Uri", uri),
						new Tag("Status", response.StatusCode),
						new Tag("Attempt", attempt));
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

				// Stop on a permanent error (e.g. 404), keep retrying transient ones (e.g. 503)
				if (exception.StatusCode != null && (!IsTransient(exception.StatusCode) || attempt >= MaxAttempts))
					break;
			}
			catch (IOException exception)
			{
				// Reading the content can fail after the request itself succeeded
				getCall.Log.Add(exception);
			}
			catch (TaskCanceledException exception) // Also the timeout
			{
				getCall.Log.Add(exception);

				// Retrying a cancelled call would work through every remaining attempt
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
		IProgress<HttpGetProgress>? progress = null,
		CancellationToken cancelToken = default)
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
			if (attempt > 1)
			{
				await Task.Delay(BaseRetryDelay * Math.Pow(2, attempt - 2), cancelToken);
			}

			using HttpRequestMessage request = new(HttpMethod.Head, uri);

			try
			{
				HttpResponseMessage response = await Client.SendAsync(request, cancelToken);

				if (IsTransient(response.StatusCode) && attempt < MaxAttempts)
				{
					headCall.Log.Add("Transient error, retrying",
						new Tag("Uri", uri),
						new Tag("Status", response.StatusCode),
						new Tag("Attempt", attempt));
					response.Dispose();
					continue;
				}

				//response.Close();
				call.Log.Add("Uri Response",
					new Tag("Uri", request.RequestUri),
					new Tag("Response", response));

				return response;
			}
			catch (HttpRequestException exception)
			{
				headCall.Log.Add(exception);

				// Stop on a permanent error (e.g. 404), keep retrying transient ones (e.g. 503)
				if (exception.StatusCode != null && (!IsTransient(exception.StatusCode) || attempt >= MaxAttempts))
					break;
			}
			catch (IOException exception)
			{
				headCall.Log.Add(exception);
			}
			catch (TaskCanceledException exception) // Also the timeout
			{
				headCall.Log.Add(exception);

				// Retrying a cancelled call would work through every remaining attempt
				if (cancelToken.IsCancellationRequested)
					break;
			}
		}
		return null;
	}

	/// <summary>Returns whether a status code represents a temporary failure that's worth retrying.</summary>
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

	/// <summary>Gets the decoded response body.</summary>
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
	/// <remarks>
	/// GetBytesAsync() hands ownership of the response message here rather than disposing it, so its
	/// headers stay readable. Nothing released it before
	/// </remarks>
	public void Dispose()
	{
		Response?.Dispose();
		Response = null;
		GC.SuppressFinalize(this);
	}
}
