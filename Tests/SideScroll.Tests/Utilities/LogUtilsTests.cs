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

	[TestCase("../outside")]
	[TestCase("../../outside")]
	[TestCase("sub/name")]
	[TestCase(@"sub\name")]
	[TestCase("..")]
	[Description(
		"The prefix names the log file, and was concatenated straight into it, so one carrying a " +
		"separator or a parent segment wrote outside the directory the caller asked for")]
	public void APrefixThatEscapesTheDirectoryIsRejected(string filePrefix)
	{
		Assert.Throws<ArgumentException>(
			() => LogUtils.Save(_directory, filePrefix, new InvalidOperationException("Test")));

		Assert.That(Directory.Exists(_directory) ? Directory.GetFiles(_directory) : [], Is.Empty,
			"nothing was written");
	}

	[Test, Description("A rooted prefix makes Path.Combine() discard the directory entirely")]
	public void ARootedPrefixIsRejected()
	{
		string rooted = Path.Combine(Path.GetTempPath(), "escaped");

		Assert.Throws<ArgumentException>(
			() => LogUtils.Save(_directory, rooted, new InvalidOperationException("Test")));
	}

	[TestCase("")]
	[TestCase("   ")]
	[Description("An empty prefix names a file that starts with its own extension")]
	public void AnEmptyPrefixIsRejected(string filePrefix)
	{
		Assert.Throws<ArgumentException>(
			() => LogUtils.Save(_directory, filePrefix, new InvalidOperationException("Test")));
	}

	[TestCase("Test")]
	[TestCase("My App")]
	[TestCase("App.Name")]
	[TestCase("App-Name_1")]
	[Description("An ordinary prefix still writes, including the dots and spaces a product name has")]
	public void AnOrdinaryPrefixStillWrites(string filePrefix)
	{
		Assert.DoesNotThrow(
			() => LogUtils.Save(_directory, filePrefix, new InvalidOperationException("Test")));

		Assert.That(Directory.GetFiles(_directory), Has.Length.EqualTo(1));
		Assert.That(Path.GetFileName(Directory.GetFiles(_directory)[0]), Does.StartWith(filePrefix));
	}
}
