using NUnit.Framework;

namespace SideScroll.Tests;

[Category("LinkUri")]
public class LinkUriTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("LinkUri");
	}

	[Test]
	public void TestParseLinkId()
	{
		Assert.That(LinkUri.TryParse("sidescroll://type/v3.1/id", out LinkUri? uri));

		Assert.That(uri!.Prefix, Is.EqualTo("sidescroll"));
		Assert.That(uri.Type, Is.EqualTo("type"));
		Assert.That(uri.Version, Is.EqualTo(new Version(3, 1)));
		Assert.That(uri.Path, Is.EqualTo("id"));
	}

	[Test]
	public void TestParseLinkSubTypeId()
	{
		Assert.That(LinkUri.TryParse("sidescroll://type.subtype/v3.1/id", out LinkUri? uri));

		Assert.That(uri!.Prefix, Is.EqualTo("sidescroll"));
		Assert.That(uri.Type, Is.EqualTo("type.subtype"));
		Assert.That(uri.Version, Is.EqualTo(new Version(3, 1)));
		Assert.That(uri.Path, Is.EqualTo("id"));
	}

	[Test]
	public void TestParseQuery()
	{
		Assert.That(LinkUri.TryParse("sidescroll://type/path?query", out LinkUri? uri));

		Assert.That(uri!.Prefix, Is.EqualTo("sidescroll"));
		Assert.That(uri.Type, Is.EqualTo("type"));
		Assert.That(uri.Path, Is.EqualTo("path"));
		Assert.That(uri.Query, Is.EqualTo("query"));
	}

	[Test, Description("A uri without a query round trips without gaining a trailing '?'")]
	public void TestParseWithoutQuery()
	{
		const string url = "sidescroll://type/v3.1/path";

		Assert.That(LinkUri.TryParse(url, out LinkUri? uri));

		Assert.That(uri!.Query, Is.Null);
		Assert.That(uri.ToUri(), Is.EqualTo(url));
	}

	[Test]
	public void TestParseWithQueryRoundTrips()
	{
		const string url = "sidescroll://type/v3.1/path?query";

		Assert.That(LinkUri.TryParse(url, out LinkUri? uri));

		Assert.That(uri!.ToUri(), Is.EqualTo(url));
	}

	[Test]
	[SetCulture("tr-TR")]
	public void TestParse_NormalizationIsCultureInvariant()
	{
		Assert.That(LinkUri.TryParse("SIDE://ITEM/path", out LinkUri? uri));

		Assert.That(uri!.Prefix, Is.EqualTo("side"));
		Assert.That(uri.Type, Is.EqualTo("item"));
	}

	[Test]
	public void TestParseVersionedPath()
	{
		Assert.That(LinkUri.TryParse("sidescroll://type/v3.1/path?query", out LinkUri? uri));

		Assert.That(uri!.Prefix, Is.EqualTo("sidescroll"));
		Assert.That(uri.Type, Is.EqualTo("type"));
		Assert.That(uri.Path, Is.EqualTo("path"));
		Assert.That(uri.Query, Is.EqualTo("query"));
	}

	// ─── Versions ────────────────────────────────────────────────────────

	[TestCase("v1..2", TestName = "Empty component")]
	[TestCase("v.", TestName = "Only a dot")]
	[TestCase("v...", TestName = "Only dots")]
	[TestCase("v1.2.3.4.5", TestName = "Too many components")]
	[TestCase("v99999999999", TestName = "Larger than an int")]
	[Description("The regex only matches digits and dots, so an invalid version has to return false instead of throwing")]
	public void TestParseInvalidVersion(string version)
	{
		Assert.That(LinkUri.TryParse($"sidescroll://type/{version}/path", out LinkUri? uri), Is.False);
		Assert.That(uri, Is.Null);

		Assert.Throws<ArgumentException>(() => LinkUri.Parse($"sidescroll://type/{version}/path"));
	}

	[TestCase("v1", "1.0", TestName = "A single component gets a minor version")]
	[TestCase("v1.2", "1.2", TestName = "Major and minor")]
	[TestCase("v1.2.3.4", "1.2.3.4", TestName = "All four components")]
	public void TestParseValidVersion(string version, string expected)
	{
		Assert.That(LinkUri.TryParse($"sidescroll://type/{version}/path", out LinkUri? uri));

		Assert.That(uri!.Version, Is.EqualTo(Version.Parse(expected)));
		Assert.That(uri.Path, Is.EqualTo("path"));
	}
}
