using SideScroll.Core.Utilities;
using NUnit.Framework;

namespace SideScroll.Core.Test;

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
		Assert.IsFalse(FileUtils.IsFileOpen("not_a_file"));
	}
}
