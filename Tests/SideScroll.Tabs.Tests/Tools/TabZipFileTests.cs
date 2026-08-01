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
}
