using NUnit.Framework;

namespace Atlas.Core.Test;

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
		LinkUri uri = LinkUri.Parse("atlas://type/v3.1/id")!;

		Assert.AreEqual("atlas", uri.Prefix);
		Assert.AreEqual("type", uri.Type);
		Assert.AreEqual(new Version(3, 1), uri.Version);
		Assert.AreEqual("id", uri.Path);
	}

	[Test]
	public void TestParseQuery()
	{
		LinkUri uri = LinkUri.Parse("atlas://type/path?query")!;

		Assert.AreEqual("atlas", uri.Prefix);
		Assert.AreEqual("type", uri.Type);
		Assert.AreEqual("path", uri.Path);
		Assert.AreEqual("query", uri.Query);
	}

	[Test]
	public void TestParseVersionedPath()
	{
		LinkUri uri = LinkUri.Parse("atlas://type/v3.1/path?query")!;

		Assert.AreEqual("atlas", uri.Prefix);
		Assert.AreEqual("type", uri.Type);
		Assert.AreEqual("path", uri.Path);
		Assert.AreEqual("query", uri.Query);
 	}
}
