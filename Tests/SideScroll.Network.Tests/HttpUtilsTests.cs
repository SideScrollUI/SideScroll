using NUnit.Framework;
using SideScroll.Network.Http;
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
}
