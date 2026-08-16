using NUnit.Framework;
using SideScroll.Network.Http;

namespace SideScroll.Network.Tests;

[Category("HTTP")]
public class HttpMemoryCacheTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("HttpMemoryCache");
	}

	[TestCase(0)]
	[TestCase(-1)]
	[Description(
		"A SizeLimit of zero rejects every size one entry, so nothing was ever cached and every " +
		"lookup silently refetched. MemoryTypeCache already rejected these")]
	public void RejectsNonPositiveMaxItems(int maxItems)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new HttpMemoryCache(maxItems));
	}

	[Test]
	public void AcceptsAPositiveMaxItems()
	{
		using var cache = new HttpMemoryCache(1);

		Assert.That(cache.MaxItems, Is.EqualTo(1));
	}

	[TestCase(0)]
	[TestCase(-1)]
	[Description("MemoryCache rejects a non-positive relative expiration when an entry is added")]
	public void RejectsNonPositiveCacheDuration(int ticks)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			new HttpMemoryCache(cacheDuration: TimeSpan.FromTicks(ticks)));
	}

	[Test]
	public void AcceptsAPositiveCacheDuration()
	{
		using var cache = new HttpMemoryCache(cacheDuration: TimeSpan.FromMinutes(1));

		Assert.That(cache.CacheDuration, Is.EqualTo(TimeSpan.FromMinutes(1)));
	}

	[Test, Description("The same URI can be cached independently as more than one response type")]
	public void CacheKeysIncludeTheRequestedType()
	{
		using var cache = new HttpMemoryCache();
		const string key = "http-memory-cache-test";
		cache.Add(key, "text");
		cache.Add(key, 42);

		Assert.That(cache.TryGetValue(Call, key, out string? text), Is.True);
		Assert.That(text, Is.EqualTo("text"));
		Assert.That(cache.TryGetValue(Call, key, out int value), Is.True);
		Assert.That(value, Is.EqualTo(42));
	}

	[Test]
	public void DisposeReleasesUnderlyingCache()
	{
		var cache = new HttpMemoryCache();

		cache.Dispose();

		Assert.Throws<ObjectDisposedException>(() => cache.MemoryCache.CreateEntry("key"));
	}
	public class CachedBase { public string? Name { get; set; } }
	public class CachedDerived : CachedBase { public int Extra { get; set; } }

	[Test, Description(
		"Add() and TryGetValue() both read the type from the call site, so a uri stored through a " +
		"base typed reference is found by asking for that base type. Keying on the object's own " +
		"type instead would store it as the derived one and miss")]
	public void CachedThroughABaseReferenceIsFoundAsThatBase()
	{
		using var cache = new HttpMemoryCache();
		const string key = "http-memory-cache-polymorphic";

		CachedBase item = new CachedDerived { Name = "name", Extra = 7 };
		cache.Add(key, item);

		Assert.That(cache.TryGetValue(Call, key, out CachedBase? found), Is.True);
		Assert.That(found!.Name, Is.EqualTo("name"));
		Assert.That(found, Is.InstanceOf<CachedDerived>(), "The stored object keeps its own type");
	}

	[Test, Description("Disposing more than once is a no op rather than reaching the cache again")]
	public void DisposeIsIdempotent()
	{
		var cache = new HttpMemoryCache();

		cache.Dispose();

		Assert.DoesNotThrow(cache.Dispose);
	}
}
