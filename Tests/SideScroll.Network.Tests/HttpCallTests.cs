using NUnit.Framework;
using SideScroll.Network.Http;
using SideScroll.Tasks;
using System.Net;
using System.Text;

namespace SideScroll.Network.Tests;

[Category("HTTP")]
[NonParallelizable] // MaxAttempts and SleepMilliseconds are static
public class HttpCallTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("HttpCall");
	}

	private int _originalSleep;

	[SetUp]
	public void Setup()
	{
		// Retries would otherwise wait 500ms, then 1s, then 1.5s
		_originalSleep = HttpCall.SleepMilliseconds;
		HttpCall.SleepMilliseconds = 0;
	}

	[TearDown]
	public void TearDown()
	{
		HttpCall.SleepMilliseconds = _originalSleep;
	}

	/// <summary>Counts requests and returns whatever the test asks for, without touching the network</summary>
	private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
	{
		public int RequestCount { get; private set; }
		public List<string?> AcceptHeaders { get; } = [];

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			RequestCount++;
			AcceptHeaders.Add(request.Headers.Accept.ToString() is { Length: > 0 } accept ? accept : null);

			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(respond(request));
		}
	}

	// GetClient() is virtual so a request can be served without a network
	private sealed class StubHttpCall(Call call, HttpClient client) : HttpCall(call)
	{
		protected override HttpClient GetClient(HttpClientConfig clientConfig) => client;
	}

	private sealed class StubCachedCall(Call call, HttpCache cache, HttpClient client) : HttpCachedCall(call, cache)
	{
		protected override HttpClient GetClient(HttpClientConfig clientConfig) => client;
	}

	private static HttpResponseMessage Ok(string body) => new(HttpStatusCode.OK)
	{
		Content = new StringContent(body, Encoding.UTF8),
	};

	private static Call CancelledCall()
	{
		TaskInstance taskInstance = new();
		taskInstance.Cancel();
		return new Call { TaskInstance = taskInstance };
	}

	[Test, Description(
		"The cancel token reaches SendAsync now, and a cancelled call stops instead of working " +
		"through every remaining retry")]
	public void CancellingStopsTheRequest()
	{
		StubHandler handler = new(_ => Ok("unused"));
		using HttpClient client = new(handler);
		var httpCall = new StubHttpCall(CancelledCall(), client);

		// ThrowsAsync, not Assert.That(async () => ...), which binds to the void TestDelegate and
		// never awaits the returned task, so the constraint passes whatever happens
		Assert.ThrowsAsync<TaskCanceledException>(
			async () => await httpCall.GetBytesAsync("http://localhost/x"));

		// HttpClient dispatches to the handler before the cancelled token surfaces, so one request
		// is expected. What matters is that it stops there rather than retrying MaxAttempts times
		Assert.That(handler.RequestCount, Is.EqualTo(1));
	}

	[Test, Description("A read failure is retried rather than propagating, and the cause survives the final throw")]
	public void AReadFailureRetriesAndKeepsItsCause()
	{
		StubHandler handler = new(_ => throw new IOException("read failed"));
		using HttpClient client = new(handler);
		var httpCall = new StubHttpCall(new Call(), client);

		Exception exception = Assert.ThrowsAsync<Exception>(
			async () => await httpCall.GetBytesAsync("http://localhost/x"))!;

		Assert.That(handler.RequestCount, Is.EqualTo(HttpCall.MaxAttempts));
		Assert.That(exception.InnerException, Is.InstanceOf<IOException>(),
			"The final failure used to discard every cause it had logged.");
	}

	[Test, Description("Control: a successful request returns its body and doesn't retry")]
	public async Task ASuccessfulRequestReturnsItsBody()
	{
		StubHandler handler = new(_ => Ok("contents"));
		using HttpClient client = new(handler);
		var httpCall = new StubHttpCall(new Call(), client);

		byte[] bytes = await httpCall.GetBytesAsync("http://localhost/x");

		Assert.That(Encoding.UTF8.GetString(bytes), Is.EqualTo("contents"));
		Assert.That(handler.RequestCount, Is.EqualTo(1));
	}

	// ─── Cached calls ────────────────────────────────────────────────────

	private string _cachePath = null!;

	private HttpCache CreateCache()
	{
		_cachePath = Path.Combine(Environment.CurrentDirectory, "HttpCallTests", TestContext.CurrentContext.Test.Name);
		if (Directory.Exists(_cachePath))
		{
			Directory.Delete(_cachePath, true);
		}
		Directory.CreateDirectory(_cachePath);

		return new HttpCache(_cachePath, true);
	}

	[Test, Description(
		"The same uri returns different content per Accept header, and they shared one cache entry, " +
		"so a request for one representation was served the other's cached body")]
	public async Task DifferentAcceptHeadersGetSeparateCacheEntries()
	{
		// The Accept header is applied by HttpClientManager when it builds the client, which the
		// stub replaces, so each response is numbered instead to tell the two requests apart
		int served = 0;
		StubHandler handler = new(_ => Ok("response-" + ++served));
		using HttpClient client = new(handler);
		using HttpCache cache = CreateCache();
		var httpCall = new StubCachedCall(new Call(), cache, client);

		string? json = await httpCall.GetStringAsync("http://localhost/x", "application/json");
		string? xml = await httpCall.GetStringAsync("http://localhost/x", "application/xml");

		Assert.That(json, Is.EqualTo("response-1"));
		Assert.That(xml, Is.EqualTo("response-2"), "The json entry used to be returned here.");
		Assert.That(handler.RequestCount, Is.EqualTo(2));

		// Each representation keeps its own entry rather than the second overwriting the first
		Assert.That(await httpCall.GetStringAsync("http://localhost/x", "application/json"), Is.EqualTo("response-1"));
		Assert.That(await httpCall.GetStringAsync("http://localhost/x", "application/xml"), Is.EqualTo("response-2"));
		Assert.That(handler.RequestCount, Is.EqualTo(2), "Both should now come from the cache.");
	}

	[Test, Description("Control: repeating a request is served from the cache, and no Accept still keys on the uri alone")]
	public async Task RepeatedRequestsAreCached()
	{
		StubHandler handler = new(_ => Ok("contents"));
		using HttpClient client = new(handler);
		using HttpCache cache = CreateCache();
		var httpCall = new StubCachedCall(new Call(), cache, client);

		Assert.That(await httpCall.GetStringAsync("http://localhost/x"), Is.EqualTo("contents"));
		Assert.That(await httpCall.GetStringAsync("http://localhost/x"), Is.EqualTo("contents"));

		Assert.That(handler.RequestCount, Is.EqualTo(1), "The second request should come from the cache.");
	}

	// ─── Response disposal ───────────────────────────────────────────────

	[Test, Description(
		"GetBytesAsync() transfers ownership of the response message into the ViewHttpResponse so " +
		"its headers stay readable, and nothing released it")]
	public void DisposingTheViewResponseReleasesItsResponse()
	{
		HttpResponseMessage response = Ok("contents");
		ViewHttpResponse viewResponse = new(response, [1, 2, 3]);

		viewResponse.Dispose();

		Assert.That(viewResponse.Response, Is.Null);
		Assert.Throws<ObjectDisposedException>(() => _ = response.Content.ReadAsStringAsync().Result);
	}
}
