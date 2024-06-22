using NUnit.Framework;

namespace SideScroll.Core.Test;

[Category("LinkUri")]
public class TestLinkUri : TestBase
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("LinkUri");
	}

	[Test]
	public void TestParseLinkId()
	{
		Assert.IsTrue(LinkUri.TryParseTyped("sidescroll://type/v3.1/id", out LinkUri? uri));

		Assert.AreEqual("sidescroll", uri!.Prefix);
		Assert.AreEqual("type", uri.Type);
		Assert.AreEqual(new Version(3, 1), uri.Version);
		Assert.AreEqual("id", uri.Path);
	}

	[Test]
	public void TestParseQuery()
	{
		Assert.IsTrue(LinkUri.TryParseTyped("sidescroll://type/path?query", out LinkUri? uri));

		Assert.AreEqual("sidescroll", uri!.Prefix);
		Assert.AreEqual("type", uri.Type);
		Assert.AreEqual("path", uri.Path);
		Assert.AreEqual("query", uri.Query);
	}

	[Test]
	public void TestParseVersionedPath()
	{
		Assert.IsTrue(LinkUri.TryParseTyped("sidescroll://type/v3.1/path?query", out LinkUri? uri));

		Assert.AreEqual("sidescroll", uri!.Prefix);
		Assert.AreEqual("type", uri.Type);
		Assert.AreEqual("path", uri.Path);
		Assert.AreEqual("query", uri.Query);
 	}
}
