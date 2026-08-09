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
	public void IsTextFile_ExtensionCheckIsCaseInsensitive()
	{
		Assert.That(FileUtils.IsTextFile("missing.TXT"), Is.True);
		Assert.That(FileUtils.IsTextFile("missing.Md"), Is.True);
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

	// Asserted against the rule rather than by calling DeleteDirectory() with a real root. A test
	// that deleted to prove it doesn't delete becomes the failure it guards against if this regresses
	private static FileUtils.DeletePathRejection Rejection(string? path) =>
		FileUtils.ValidateDeletePath(path, out _);

	[Test, Description("Recursive deletion must never accept a filesystem root")]
	public void ValidateDeletePath_RejectsFilesystemRoots()
	{
		string root = Path.GetPathRoot(Path.GetFullPath(Path.GetTempPath()))!;

		Assert.That(Rejection(root), Is.EqualTo(FileUtils.DeletePathRejection.FilesystemRoot));
		Assert.That(Rejection(Path.TrimEndingDirectorySeparator(root)), Is.EqualTo(FileUtils.DeletePathRejection.FilesystemRoot));
	}

	[Test, Description(
		"A path rooted against the current directory or drive reads as absolute but isn't. " +
		"GetFullPath(\"C:\") is the working directory, and a leading separator rebases onto the current drive")]
	public void ValidateDeletePath_RejectsPathsThatAreNotFullyQualified()
	{
		Assert.That(Rejection("relative/path"), Is.EqualTo(FileUtils.DeletePathRejection.NotFullyQualified));

		if (OperatingSystem.IsWindows())
		{
			Assert.That(Rejection("C:"), Is.EqualTo(FileUtils.DeletePathRejection.NotFullyQualified));
			Assert.That(Rejection(@"\some\path"), Is.EqualTo(FileUtils.DeletePathRejection.NotFullyQualified));

			// Reads as a POSIX root and resolves onto the current drive
			Assert.That(Rejection("/"), Is.EqualTo(FileUtils.DeletePathRejection.NotFullyQualified));
		}
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public void ValidateDeletePath_RejectsBlankPaths(string? path)
	{
		Assert.That(Rejection(path), Is.EqualTo(FileUtils.DeletePathRejection.Blank));
	}

	[Test, Description("Control: an ordinary absolute directory below a root is allowed")]
	public void ValidateDeletePath_AllowsADirectoryBelowTheRoot()
	{
		string path = Path.Combine(Path.GetFullPath(Path.GetTempPath()), "SideScrollDeleteTarget");

		Assert.That(FileUtils.ValidateDeletePath(path, out string? fullPath), Is.EqualTo(FileUtils.DeletePathRejection.Allowed));
		Assert.That(fullPath, Is.EqualTo(path));
	}

	[Test, Description("Control: an allowed directory is still deleted, and a refused one logs a warning")]
	public void DeleteDirectory_DeletesAnAllowedDirectory()
	{
		string path = Path.Combine(Path.GetTempPath(), "SideScrollDeleteTarget", Guid.NewGuid().ToString("N")[..8]);
		Directory.CreateDirectory(Path.Combine(path, "nested"));
		File.WriteAllText(Path.Combine(path, "nested", "file.txt"), "contents");

		FileUtils.DeleteDirectory(Call, path);

		Assert.That(Directory.Exists(path), Is.False);

		FileUtils.DeleteDirectory(Call, "relative/path");
		Assert.That(Call.Log.EntriesText(), Does.Contain("Refusing to delete directory"));
	}

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
