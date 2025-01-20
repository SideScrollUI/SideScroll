using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Test;

[Category("Core")]
public class TestFileUtils : TestBase
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("FileUtils");
	}

	[Test]
	public void TestIsFileNotFound()
	{
		Assert.That(FileUtils.IsFileOpen("not_a_file"), Is.False);
	}
}
