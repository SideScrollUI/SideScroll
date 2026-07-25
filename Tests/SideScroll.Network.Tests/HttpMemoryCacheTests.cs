using NUnit.Framework;
using SideScroll.Network.Http;

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
}
