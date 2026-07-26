using NUnit.Framework;
using SideScroll.Tabs.Tools.FileViewer;

namespace SideScroll.Tabs.Tests;

/// <summary>
/// File type detection runs under the user's culture. Turkish treats dotted and dotless i as
/// separate letters, so 'I' doesn't lowercase to 'i' and IgnoreCase doesn't equate them
/// </summary>
[Category("Tabs"), SetCulture("tr-TR")]
public class FileTypeCultureTests : BaseTest
{
	private string _basePath = null!;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("FileTypeCulture");
	}

	[SetUp]
	public void Setup()
	{
		// MethodName, Name includes the TestCase arguments which aren't valid in a path
		_basePath = Path.Combine(Environment.CurrentDirectory, "FileTypeCultureTests", TestContext.CurrentContext.Test.MethodName!);

		if (Directory.Exists(_basePath))
		{
			Directory.Delete(_basePath, true);
		}
		Directory.CreateDirectory(_basePath);
	}

	[TearDown]
	public void TearDown()
	{
		FileTypeDetector.ClearProbes();
	}

	private string CreateFile(string name)
	{
		string path = Path.Combine(_basePath, name);
		File.WriteAllText(path, "contents");
		return path;
	}

	[TestCase(".zip")]
	[TestCase(".ZIP")]
	[TestCase(".Zip")]
	[Description(
		"ExtensionTypes has to match regardless of case. Lowercasing with the current culture turned " +
		"\".ZIP\" into \".zıp\" (dotless i), which never matched the registered \".zip\".")]
	public void ExtensionTypes_MatchesRegardlessOfCase(string extension)
	{
		Assert.That(TabFile.ExtensionTypes.TryGetValue(extension, out Type? type), Is.True,
			$"'{extension}' should resolve to a tab type.");
		Assert.That(type, Is.EqualTo(typeof(TabZipFile)));
	}

	[Test, Description(
		"FileProbeContext.Extension is lowercased for probes to compare against, so it has to use " +
		"invariant casing rather than the user's culture")]
	public void ProbeContextExtension_IsInvariantLowercase()
	{
		string? captured = null;
		FileTypeDetector.RegisterProbe(context =>
		{
			captured = context.Extension;
			return null;
		});

		FileTypeDetector.ProbeFile(CreateFile("archive.ZIP"));

		Assert.That(captured, Is.EqualTo(".zip"),
			"The current culture would produce \".zıp\" with a dotless i.");
	}

	[TestCase("Data.atlas")]
	[TestCase("Data.ATLAS")]
	[TestCase("Data.Atlas")]
	[Description(
		"Serialized files open in the deserialized object view. The filesystem is case insensitive " +
		"on Windows and macOS, so the extension check has to be too.")]
	public void AtlasFiles_UseTheSerializedTab(string filename)
	{
		var fileView = new FileView(CreateFile(filename));

		Assert.That(fileView.Tab, Is.TypeOf<TabFileSerialized>(),
			$"'{filename}' should open with the serialized viewer.");
	}

	[Test, Description("Files that aren't serialized still use the generic file tab")]
	public void OtherFiles_UseTheGenericTab()
	{
		var fileView = new FileView(CreateFile("Data.txt"));

		Assert.That(fileView.Tab, Is.TypeOf<TabFile>());
	}
}
