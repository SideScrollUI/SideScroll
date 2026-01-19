using NUnit.Framework;
using SideScroll.Collections;

namespace SideScroll.Tests.Collections;

[Category("Collections")]
public class MemoryTypeCacheTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("MemoryTypeCache");
	}

	[Test, Description("Cache duration should expire items after specified time")]
	public async Task CacheDuration_ShouldExpireItems()
	{
		// Arrange: Create cache with 100ms expiration
		var cache = new MemoryTypeCache<string>(maxItems: 100, cacheDuration: TimeSpan.FromMilliseconds(10));
		string key = "test-key";
		string value = "test-value";

		// Act: Set a value in the cache
		cache.Set(key, value);

		// Assert: Value should be available immediately
		bool foundImmediately = cache.TryGetValue(key, out string? retrievedValue);
		Assert.That(foundImmediately, Is.True, "Value should be found immediately after setting");
		Assert.That(retrievedValue, Is.EqualTo(value));

		// Wait for cache to expire (10ms + buffer)
		await Task.Delay(20);

		// Assert: Value should be expired and not found
		bool foundAfterExpiry = cache.TryGetValue(key, out string? expiredValue);
		Assert.That(foundAfterExpiry, Is.False, "Value should not be found after cache duration expires");
		Assert.That(expiredValue, Is.Null);
	}

	[Test, Description("Cache without duration should not expire items")]
	public async Task NoCacheDuration_ShouldNotExpireItems()
	{
		// Arrange: Create cache with no expiration
		var cache = new MemoryTypeCache<string>(maxItems: 100, cacheDuration: null);
		string key = "test-key";
		string value = "test-value";

		// Act: Set a value in the cache
		cache.Set(key, value);

		// Assert: Value should be available immediately
		bool foundImmediately = cache.TryGetValue(key, out string? retrievedValue);
		Assert.That(foundImmediately, Is.True);
		Assert.That(retrievedValue, Is.EqualTo(value));

		// Wait for some time
		await Task.Delay(20);

		// Assert: Value should still be available (no expiration)
		bool foundAfterWait = cache.TryGetValue(key, out string? stillCachedValue);
		Assert.That(foundAfterWait, Is.True, "Value should still be found when no cache duration is set");
		Assert.That(stillCachedValue, Is.EqualTo(value));
	}
}
