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
		var cache = new HttpMemoryCache(1);

		Assert.That(cache.MaxItems, Is.EqualTo(1));
	}
}
