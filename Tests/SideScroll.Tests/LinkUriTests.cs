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
	public void TestParseVersionedPath()
	{
		Assert.That(LinkUri.TryParse("sidescroll://type/v3.1/path?query", out LinkUri? uri));

		Assert.That(uri!.Prefix, Is.EqualTo("sidescroll"));
		Assert.That(uri.Type, Is.EqualTo("type"));
		Assert.That(uri.Path, Is.EqualTo("path"));
		Assert.That(uri.Query, Is.EqualTo("query"));
	}
}
