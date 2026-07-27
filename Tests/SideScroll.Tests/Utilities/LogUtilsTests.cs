using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Tests.Utilities;

[Category("Core")]
public class LogUtilsTests : BaseTest
{
	private string _directory = null!;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("LogUtils");
	}

	[SetUp]
	public void Setup()
	{
		_directory = Path.Combine(
			Path.GetTempPath(),
			"SideScrollLogUtils",
			Guid.NewGuid().ToString("N"));
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(_directory))
		{
			Directory.Delete(_directory, true);
		}
	}

	[Test, Description("Exceptions saved during the same timestamp interval must not overwrite each other")]
	public void SaveCreatesUniqueFiles()
	{
		LogUtils.Save(_directory, "Test", new InvalidOperationException("First"));
		LogUtils.Save(_directory, "Test", new InvalidOperationException("Second"));

		string[] files = Directory.GetFiles(_directory, "*.log");
		Assert.That(files, Has.Length.EqualTo(2));
		Assert.That(files.Select(File.ReadAllText), Is.EquivalentTo(new[]
		{
			new InvalidOperationException("First").ToString(),
			new InvalidOperationException("Second").ToString(),
		}));
	}
}
