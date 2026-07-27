using NUnit.Framework;
using SideScroll.Network.Http;
using System.Net;

namespace SideScroll.Network.Tests;

[Category("HTTP")]
public class HttpMemoryCacheTests : BaseTest
{
	// Not a valid request uri, so a cache miss fails immediately instead of touching the network
	private const string Key = "http-memory-cache-test";

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("HTTP");
	}

	public class Order
	{
		public int Id { get; set; }
	}

	public class OrderSummary
	{
		public string? Name { get; set; }
	}

	[Test]
	public void GetReturnsCachedValue()
	{
		HttpMemoryCache cache = new();
		cache.Add(Key, new Order { Id = 5 });

		Assert.That(cache.TryGetValue(Call, Key, out Order? order), Is.True);
		Assert.That(order!.Id, Is.EqualTo(5));
	}

	[Test, Description("The same uri cached as another type is a miss, not an InvalidCastException")]
	public void MismatchedTypeIsNotACacheHit()
	{
		HttpMemoryCache cache = new();
		cache.Add(Key, new Order { Id = 5 });

		bool found = true;
		Assert.DoesNotThrow(() => found = cache.TryGetValue(Call, Key, out OrderSummary? _));
		Assert.That(found, Is.False);

		// The entry it was actually cached as still resolves
		Assert.That(cache.TryGetValue(Call, Key, out Order? order), Is.True);
		Assert.That(order!.Id, Is.EqualTo(5));
	}

	[Test, Description("A JSON null response is not a successful lookup and is not cached")]
	public void JsonNullIsNotFound()
	{
		HttpClient original = HttpUtils.Client;
		HttpUtils.Client = new HttpClient(new NullJsonHandler());

		try
		{
			HttpMemoryCache cache = new();

			Assert.That(cache.TryGetValue(Call, "http://example.com/null", out Order? order), Is.False);
			Assert.That(order, Is.Null);
			Assert.That(cache.MemoryCache.TryGetValue("http://example.com/null", out _), Is.False);
		}
		finally
		{
			HttpUtils.Client.Dispose();
			HttpUtils.Client = original;
		}
	}

	[Test]
	public void Dispose_ReleasesUnderlyingCache()
	{
		var cache = new HttpMemoryCache();

		Assert.DoesNotThrow(cache.Dispose);
		Assert.That(cache, Is.InstanceOf<IDisposable>());
	}

	[TestCase(0)]
	[TestCase(-1)]
	public void Constructor_RejectsNonPositiveDuration(int milliseconds)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			new HttpMemoryCache(cacheDuration: TimeSpan.FromMilliseconds(milliseconds)));
	}

	private sealed class NullJsonHandler : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("null"),
				RequestMessage = request,
			});
		}
	}
}
