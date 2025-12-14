using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Tests;

[Category("Core")]
public class FileUtilsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("FileUtils");
	}

	[Test]
	public void IsFileOpenFileNotFound()
	{
		Assert.That(FileUtils.IsFileOpen("not_a_file"), Is.False);
	}
}
