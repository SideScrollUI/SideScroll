using NUnit.Framework;
using SideScroll.Network.Http;
using SideScroll.Tasks;
using System.Net;
using System.Text;

namespace SideScroll.Network.Tests;

[Category("HTTP")]
public class HttpUtilsTests : BaseTest
{
	// Nothing listens here, so requests fail without touching the network
	private const string UnreachableUri = "http://127.0.0.1:59321/";

	// Connecting to a closed port can take seconds to give up, so most tests time out instead
	private static readonly TimeSpan ShortTimeout = TimeSpan.FromMilliseconds(50);

	private int _maxAttempts;
	private TimeSpan _baseRetryDelay;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("HTTP");
	}

	[SetUp]
	public void Setup()
	{
		_maxAttempts = HttpUtils.MaxAttempts;
		_baseRetryDelay = HttpUtils.BaseRetryDelay;

		HttpUtils.MaxAttempts = 2;
		HttpUtils.BaseRetryDelay = TimeSpan.FromMilliseconds(1);
	}

	[TearDown]
	public void TearDown()
	{
		HttpUtils.MaxAttempts = _maxAttempts;
		HttpUtils.BaseRetryDelay = _baseRetryDelay;
	}

	[Test, Description("Timeouts are retried and return null instead of throwing a TaskCanceledException")]
	public async Task GetBytesAsyncReturnsNullOnTimeout()
	{
		ViewHttpResponse? response = await HttpUtils.GetBytesAsync(Call, UnreachableUri, ShortTimeout);

		Assert.That(response, Is.Null);
	}

	[Test, Description("Connection failures are retried and return null instead of throwing an HttpRequestException")]
	[Ignore("Takes ~6 seconds due to OS socket connection timeout on Windows")]
	public async Task GetBytesAsyncReturnsNullWhenUnreachable()
	{
		ViewHttpResponse? response = await HttpUtils.GetBytesAsync(Call, UnreachableUri);

		Assert.That(response, Is.Null);
	}

	[Test, Description("Head requests fail the same way instead of throwing")]
	public async Task GetHeadAsyncReturnsNullOnTimeout()
	{
		HttpClient original = HttpUtils.Client;
		HttpUtils.Client = new HttpClient { Timeout = ShortTimeout };

		try
		{
			HttpResponseMessage? response = await HttpUtils.GetHeadAsync(Call, UnreachableUri);

			Assert.That(response, Is.Null);
		}
		finally
		{
			HttpUtils.Client.Dispose();
			HttpUtils.Client = original;
		}
	}

	[Test, Description("Response bodies decode as UTF-8, ASCII would replace every byte over 0x7F")]
	public void DecodeStringUsesUtf8()
	{
		const string text = "Ünicode ☃ 日本語";
		byte[] bytes = Encoding.UTF8.GetBytes(text);

		Assert.That(HttpUtils.DecodeString(bytes), Is.EqualTo(text));
	}

	[Test, Description("A byte order mark would break parsers like JsonSerializer")]
	public void DecodeStringSkipsByteOrderMark()
	{
		byte[] bytes = [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes("{}")];

		Assert.That(HttpUtils.DecodeString(bytes), Is.EqualTo("{}"));
	}

	[Test]
	public void DecodeStringEmpty()
	{
		Assert.That(HttpUtils.DecodeString([]), Is.EqualTo(""));
	}

	[Test]
	public void EmptyViewHttpResponse_HasEmptyBody()
	{
		Assert.That(new ViewHttpResponse().Body, Is.Empty);
	}

	[Test]
	public async Task GetStringAsync_DisposesResponse()
	{
		HttpClient original = HttpUtils.Client;
		var content = new TrackingContent([1]);
		HttpUtils.Client = new HttpClient(new StubHandler((_, _) =>
			Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content })));

		try
		{
			Assert.That(await HttpUtils.GetStringAsync(Call, "http://example.com/value"), Is.EqualTo("\u0001"));
			Assert.That(content.Disposed, Is.True);
		}
		finally
		{
			HttpUtils.Client.Dispose();
			HttpUtils.Client = original;
		}
	}

	[Test, Description("A transient response must release its connection before the retry")]
	public async Task GetBytesAsyncDisposesTransientResponses()
	{
		HttpClient original = HttpUtils.Client;
		var transientContent = new TrackingContent();
		int attempts = 0;
		HttpUtils.Client = new HttpClient(new StubHandler((request, _) =>
		{
			attempts++;
			return Task.FromResult(attempts == 1
				? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = transientContent }
				: new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1]) });
		}));

		try
		{
			ViewHttpResponse? result = await HttpUtils.GetBytesAsync(Call, "http://example.com/value");

			Assert.That(result, Is.Not.Null);
			Assert.That(transientContent.Disposed, Is.True);
			result!.Response!.Dispose();
		}
		finally
		{
			HttpUtils.Client.Dispose();
			HttpUtils.Client = original;
		}
	}

	[Test, Description("Cancelling the Call aborts an in-flight HTTP request")]
	public async Task GetBytesAsyncObservesCallCancellation()
	{
		HttpClient original = HttpUtils.Client;
		HttpUtils.Client = new HttpClient(new StubHandler(async (_, cancelToken) =>
		{
			await Task.Delay(Timeout.InfiniteTimeSpan, cancelToken);
			return new HttpResponseMessage(HttpStatusCode.OK);
		}));
		var taskInstance = new TaskInstance();
		Call cancellableCall = new() { TaskInstance = taskInstance };

		try
		{
			Task<ViewHttpResponse?> request = HttpUtils.GetBytesAsync(cancellableCall, "http://example.com/value");
			taskInstance.Cancel();

			Assert.That(await request.WaitAsync(TimeSpan.FromSeconds(1)), Is.Null);
		}
		finally
		{
			HttpUtils.Client.Dispose();
			HttpUtils.Client = original;
		}
	}

	[Test]
	public void ViewHttpResponseDispose_DisposesOwnedResponse()
	{
		var content = new TrackingContent();
		var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
		var view = new ViewHttpResponse(response, []);

		view.Dispose();

		Assert.That(content.Disposed, Is.True);
		Assert.That(view.Response, Is.Null);
	}

	private sealed class StubHandler(
		Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			HttpResponseMessage response = await sendAsync(request, cancellationToken);
			response.RequestMessage ??= request;
			return response;
		}
	}

	private sealed class TrackingContent : ByteArrayContent
	{
		public TrackingContent() : base([])
		{
		}

		public TrackingContent(byte[] bytes) : base(bytes)
		{
		}

		public bool Disposed { get; private set; }

		protected override void Dispose(bool disposing)
		{
			Disposed = true;
			base.Dispose(disposing);
		}
	}
}
