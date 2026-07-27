using NUnit.Framework;

namespace SideScroll.Tests;

[Category("Core")]
public class PathsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Paths");
	}

	[Test]
	public void Combine_LeadingWindowsSeparator_DoesNotDiscardBasePath()
	{
		string result = Paths.Combine("root", @"\folder", "file.txt");

		Assert.That(result, Is.EqualTo("root/folder/file.txt"));
	}
}
