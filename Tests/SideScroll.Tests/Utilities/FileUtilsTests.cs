using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Tests.Utilities;

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

	[Test]
	public void IsTextStream_StreamReaderPreservesPosition()
	{
		using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("plain text"));
		using var reader = new StreamReader(stream);

		Assert.That(FileUtils.IsTextStream(reader), Is.True);
		Assert.That(reader.ReadToEnd(), Is.EqualTo("plain text"));
	}

	[Test]
	public void DefaultFilePath_IsEmpty()
	{
		FilePath path = default;

		Assert.That(path.Path, Is.Empty);
		Assert.That(path.ToString(), Is.Empty);
	}

	// ─── DirectoryCopy ───────────────────────────────────────────────────

	private string _basePath = null!;

	[SetUp]
	public void Setup()
	{
		_basePath = Path.Combine(Path.GetTempPath(), "SideScrollFileUtils", Guid.NewGuid().ToString("N")[..8]);
	}

	[TearDown]
	public void TearDown()
	{
		try
		{
			if (Directory.Exists(_basePath))
			{
				Directory.Delete(_basePath, true);
			}
		}
		catch (IOException)
		{
		}
	}

	private string CreateSource(params string[] subDirectories)
	{
		string source = Path.Combine(_basePath, "source");
		Directory.CreateDirectory(source);
		File.WriteAllText(Path.Combine(source, "file.txt"), "data");

		foreach (string subDirectory in subDirectories)
		{
			string path = Path.Combine(source, subDirectory);
			Directory.CreateDirectory(path);
			File.WriteAllText(Path.Combine(path, "nested.txt"), "nested");
		}

		return source;
	}

	private static int CountDirectories(string path) =>
		Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Length;

	[Test, Description(
		"The destination is created before the source subdirectories are enumerated, so one inside " +
		"the source used to be copied into itself until the path limit stopped it, leaving a deep " +
		"tree of garbage behind")]
	public void DirectoryCopy_DestinationInsideSource_Throws()
	{
		string source = CreateSource();
		string dest = Path.Combine(source, "backup");

		Assert.Throws<ArgumentException>(() =>
			FileUtils.DirectoryCopy(Call, source, dest, copySubDirs: true));

		Assert.That(CountDirectories(source), Is.Zero, "It shouldn't have created anything.");
	}

	[Test]
	public void DirectoryCopy_DestinationDeeperInsideSource_Throws()
	{
		string source = CreateSource("sub");
		string dest = Path.Combine(source, "sub", "backup");

		Assert.Throws<ArgumentException>(() =>
			FileUtils.DirectoryCopy(Call, source, dest, copySubDirs: true));
	}

	[Test]
	public void DirectoryCopy_DestinationIsSource_Throws()
	{
		string source = CreateSource();

		Assert.Throws<ArgumentException>(() =>
			FileUtils.DirectoryCopy(Call, source, source, copySubDirs: true));
	}

	[Test, Description("A destination beside the source is the normal case and still works")]
	public void DirectoryCopy_DestinationOutsideSource_Copies()
	{
		string source = CreateSource("sub");
		string dest = Path.Combine(_basePath, "dest");

		FileUtils.DirectoryCopy(Call, source, dest, copySubDirs: true);

		Assert.That(File.Exists(Path.Combine(dest, "file.txt")), Is.True);
		Assert.That(File.Exists(Path.Combine(dest, "sub", "nested.txt")), Is.True);
		Assert.That(CountDirectories(dest), Is.EqualTo(1), "Only the one subdirectory.");
	}

	[Test, Description("A source inside the destination is fine, nothing recurses in that direction")]
	public void DirectoryCopy_SourceInsideDestination_Copies()
	{
		string source = CreateSource("sub");
		string dest = _basePath;

		FileUtils.DirectoryCopy(Call, Path.Combine(source, "sub"), dest, copySubDirs: true);

		Assert.That(File.Exists(Path.Combine(dest, "nested.txt")), Is.True);
	}

	[Test, Description(
		"Without recursion there's nothing to nest, so copying into a child is left alone rather " +
		"than newly rejected")]
	public void DirectoryCopy_DestinationInsideSource_WithoutSubDirs_Copies()
	{
		string source = CreateSource();
		string dest = Path.Combine(source, "backup");

		FileUtils.DirectoryCopy(Call, source, dest, copySubDirs: false);

		Assert.That(File.Exists(Path.Combine(dest, "file.txt")), Is.True);
	}
}
