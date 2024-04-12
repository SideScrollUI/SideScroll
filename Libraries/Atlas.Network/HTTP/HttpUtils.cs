using Atlas.Core;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Atlas.Network;

public static class HttpUtils
{
	private const int ReadBufferSize = 100000;

	public static int MaxAttempts { get; set; } = 5;
	public static TimeSpan BaseRetryDelay = TimeSpan.FromMilliseconds(500); // < ^ MaxAttempts

	public static readonly HttpClient Client = new();

	public class HttpGetProgress
	{
		public long Downloaded { get; set; }
		public long TotalLength { get; set; }

		public double Percent => 100.0 * Downloaded / TotalLength;
	}

	public static string? GetString(Call call, string uri)
	{
		return Task.Run(() => GetStringAsync(call, uri)).GetAwaiter().GetResult();
	}

	public static async Task<string?> GetStringAsync(Call call, string uri)
	{
		var response = await GetBytesAsync(call, uri);
		response?.Response?.EnsureSuccessStatusCode();
		byte[]? bytes = response?.Bytes;
		if (bytes != null)
			return Encoding.ASCII.GetString(bytes);

		return null;
	}

	public static ViewHttpResponse? GetBytes(Call call, string uri, TimeSpan? timeout = null, IProgress<HttpGetProgress>? progress = null)
	{
		return Task.Run(() => GetBytesAsync(call, uri, timeout, progress)).GetAwaiter().GetResult();
	}

	public static async Task<ViewHttpResponse?> GetBytesAsync(Call call, string uri, TimeSpan? timeout = null, IProgress<HttpGetProgress>? progress = null)
	{
		using CallTimer getCall = call.Timer("Get Uri", new Tag("Uri", uri));

		HttpClientConfig clientConfig = new()
		{
			Timeout = timeout,
		};
		HttpClient client = HttpClientManager.GetClient(clientConfig);

		for (int attempt = 1; attempt <= MaxAttempts; attempt++)
		{
			if (attempt > 1)
			{
				await Task.Delay(BaseRetryDelay * Math.Pow(2, attempt));
			}

			try
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				HttpResponseMessage response = await client.GetAsync(uri);

				byte[] bytes = await ReadContentAsync(response.Content, progress);

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

				return viewResponse;
			}
			catch (WebException exception)
			{
				getCall.Log.Add(exception);

				if (exception.Response != null)
				{
					string response = await new StreamReader(exception.Response.GetResponseStream()).ReadToEndAsync();
					getCall.Log.AddError("Exception: " + response);
				}

				if (exception.Status == WebExceptionStatus.ProtocolError)
					break;
			}
		}
		return null;
	}

	private static async Task<byte[]> ReadContentAsync(HttpContent content, IProgress<HttpGetProgress>? progress = null)
	{
		if (content.Headers.ContentLength == null || progress == null)
		{
			return await content.ReadAsByteArrayAsync();
		}

		await using var contentStream = await content.ReadAsStreamAsync();
		using var memoryStream = new MemoryStream();

		var buffer = new byte[ReadBufferSize];

		int bytes;
		while ((bytes = await contentStream.ReadAsync(buffer)) > 0)
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

	public static HttpResponseMessage? GetHead(Call call, string uri)
	{
		return Task.Run(() => GetHeadAsync(call, uri)).GetAwaiter().GetResult();
	}

	public static async Task<HttpResponseMessage?> GetHeadAsync(Call call, string uri)
	{
		using CallTimer headCall = call.Timer("Head Uri", new Tag("Uri", uri));

		for (int attempt = 1; attempt <= MaxAttempts; attempt++)
		{
			if (attempt > 1)
			{
				await Task.Delay(BaseRetryDelay * Math.Pow(2, attempt));
			}

			HttpRequestMessage request = new(HttpMethod.Head, uri);

			try
			{
				HttpResponseMessage response = await Client.SendAsync(request);

				//response.Close();
				call.Log.Add("Uri Response",
					new Tag("Uri", request.RequestUri),
					new Tag("Response", response));

				return response;
			}
			catch (WebException exception)
			{
				headCall.Log.Add(exception);

				if (exception.Response != null)
				{
					string response = await new StreamReader(exception.Response.GetResponseStream()).ReadToEndAsync();
					headCall.Log.AddError("Exception: " + response);
				}

				if (exception.Status == WebExceptionStatus.ProtocolError)
					break;
			}
		}
		return null;
	}
}

public class ViewHttpResponse
{
	[HiddenColumn]
	public string? Uri { get; set; }
	public string? Filename { get; set; }

	[HiddenColumn]
	public string Body => Encoding.ASCII.GetString(Bytes!);

	public HttpStatusCode? Status => Response?.StatusCode;

	[HiddenRow]
	public byte[]? Bytes { get; set; }
	public double Milliseconds { get; set; }

	[HiddenColumn, Hide(null)]
	public object? View { get; set; }

	[HiddenColumn]
	public HttpResponseMessage? Response { get; set; }

	public override string? ToString() => Filename;

	public ViewHttpResponse() { }

	public ViewHttpResponse(HttpResponseMessage response, byte[] bytes)
	{
		Response = response;
		Bytes = bytes;
	}
}
