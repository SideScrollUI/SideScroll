using NUnit.Framework;
using SideScroll.Tabs.Tools.FileViewer;
using System.IO.Compression;
using System.Text;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class TabZipFileTests : BaseTest
{
	private string _basePath = null!;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TabZipFile");
	}

	[SetUp]
	public void Setup()
	{
		_basePath = Path.Combine(Environment.CurrentDirectory, "TabZipFileTests", TestContext.CurrentContext.Test.Name);

		if (Directory.Exists(_basePath))
		{
			Directory.Delete(_basePath, true);
		}
		Directory.CreateDirectory(_basePath);
	}

	private TabModel LoadZip(string zipPath)
	{
		var tab = new TabZipFile { Path = zipPath };
		TabInstance instance = tab.Create();

		TabModel model = new();
		instance.Load(Call, model);
		return model;
	}

	private static IEnumerable<string> GetText(TabModel model) =>
		model.Objects.Select(tabObject => tabObject.Object).OfType<string>();

	[Test, Description(
		"A corrupt archive used to render as an empty grid because the error was swallowed before " +
		"the handler that reports it could run")]
	public void CorruptZip_ReportsTheError()
	{
		string zipPath = Path.Combine(_basePath, "corrupt.zip");
		File.WriteAllBytes(zipPath, Encoding.UTF8.GetBytes("this is not a zip archive"));

		TabModel model = LoadZip(zipPath);

		Assert.That(GetText(model), Has.Some.StartsWith("Error loading zip file"),
			"The failure should be shown, not left as an empty tab.");
		Assert.That(model.ItemLists, Is.Empty,
			"An empty list would imply the archive loaded and had no entries.");
	}

	[Test, Description("A valid archive still lists its entries")]
	public void ValidZip_ListsEntries()
	{
		string zipPath = Path.Combine(_basePath, "valid.zip");
		using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
		{
			using var writer = new StreamWriter(archive.CreateEntry("readme.txt").Open());
			writer.Write("contents");
		}

		TabModel model = LoadZip(zipPath);

		Assert.That(GetText(model), Has.None.StartsWith("Error loading zip file"));
		Assert.That(model.ItemLists.Single(), Has.Count.EqualTo(1));
		Assert.That(model.ItemLists.Single()[0]!.ToString(), Is.EqualTo("readme.txt"));
	}

	[Test, Description("A missing archive reports that instead of failing to open")]
	public void MissingZip_ReportsMissing()
	{
		TabModel model = LoadZip(Path.Combine(_basePath, "missing.zip"));

		Assert.That(GetText(model), Has.Some.Contains("doesn't exist"));
	}

	private static List<ZipNodeView> GetNodes(TabModel model) =>
		[.. model.ItemLists.SelectMany(list => list.Cast<object?>()).OfType<ZipNodeView>()];

	/// <summary>Every node in the tree, since an unnamed directory is nested rather than at the root</summary>
	private static List<ZipNodeView> GetAllNodes(TabModel model)
	{
		List<ZipNodeView> all = [];
		void Walk(IEnumerable<ZipNodeView> nodes)
		{
			foreach (ZipNodeView node in nodes)
			{
				all.Add(node);
				if (node is ZipDirectoryView directory)
				{
					Walk(directory.Children);
				}
			}
		}

		Walk(GetNodes(model));
		return all;
	}

	private string CreateZip(string name, Action<ZipArchive> build)
	{
		string zipPath = Path.Combine(_basePath, name);
		using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
		build(archive);
		return zipPath;
	}

	[Test, Description(
		"An archive can store a path with repeated separators. Splitting it kept the empty " +
		"component, building a directory with no name for the row to show")]
	public void RepeatedSeparatorsDoNotCreateAnUnnamedDirectory()
	{
		string zipPath = CreateZip("separators.zip", archive => archive.CreateEntry("a//b/"));

		TabModel model = LoadZip(zipPath);

		List<ZipNodeView> nodes = GetAllNodes(model);
		Assert.That(nodes, Is.Not.Empty);
		Assert.That(nodes.Select(node => node.Name), Has.None.Empty, "an unnamed row is unusable");
		Assert.That(nodes.Select(node => node.Name), Is.EqualTo(new[] { "a", "b" }));
	}

	[Test, Description("A directory the archive stores has a time of its own, which was discarded")]
	public void AStoredDirectoryKeepsItsTimestamp()
	{
		// Unspecified, a zip stores wall clock time with no zone, so it comes back as it went in
		var lastWrite = new DateTime(2024, 6, 15, 12, 30, 0);
		string zipPath = CreateZip("timestamps.zip", archive =>
		{
			ZipArchiveEntry entry = archive.CreateEntry("folder/");
			entry.LastWriteTime = lastWrite;
		});

		var directory = GetNodes(LoadZip(zipPath)).OfType<ZipDirectoryView>().Single();

		Assert.That(directory.LastWriteTime, Is.Not.Null);
		Assert.That(directory.LastWriteTime, Is.EqualTo(lastWrite));
		Assert.That(directory.Modified, Is.Not.Null);
	}

	[Test, Description(
		"A directory that only exists as a step in some file's path isn't stored by the archive, " +
		"so it has no time to report")]
	public void ADirectoryOnlyImpliedByAFileHasNoTimestamp()
	{
		string zipPath = CreateZip("implied.zip", archive => archive.CreateEntry("folder/file.txt"));

		var directory = GetNodes(LoadZip(zipPath)).OfType<ZipDirectoryView>().Single();

		Assert.That(directory.LastWriteTime, Is.Null);
	}

	[Test, Description(
		"A directory can be created while walking a file's path before the archive's own entry for " +
		"it is reached, and only that entry carries a time")]
	public void AStoredDirectoryReachedByAFileFirstStillGetsItsTimestamp()
	{
		// Unspecified, a zip stores wall clock time with no zone, so it comes back as it went in
		var lastWrite = new DateTime(2024, 6, 15, 12, 30, 0);
		string zipPath = CreateZip("ordering.zip", archive =>
		{
			archive.CreateEntry("folder/file.txt");
			archive.CreateEntry("folder/").LastWriteTime = lastWrite;
		});

		var directory = GetNodes(LoadZip(zipPath)).OfType<ZipDirectoryView>().Single();

		Assert.That(directory.LastWriteTime, Is.EqualTo(lastWrite));
	}

	[Test, Description(
		"An archive holding both a file and a directory under one name shows both. They differ by " +
		"size and by being navigable, so neither is hidden by the other")]
	public void AFileAndDirectorySharingANameAreBothShown()
	{
		string zipPath = CreateZip("collision.zip", archive =>
		{
			archive.CreateEntry("a");
			archive.CreateEntry("a/b.txt");
		});

		List<ZipNodeView> nodes = GetNodes(LoadZip(zipPath));

		Assert.That(nodes.OfType<ZipFileView>().Count(), Is.EqualTo(1));
		Assert.That(nodes.OfType<ZipDirectoryView>().Count(), Is.EqualTo(1));
		Assert.That(nodes.OfType<ZipDirectoryView>().Single().HasLinks, Is.True);
		Assert.That(nodes.OfType<ZipFileView>().Single().HasLinks, Is.False);
	}
}
