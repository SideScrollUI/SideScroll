using NUnit.Framework;
using SideScroll.Tabs.Headless;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Tools.FileViewer;
using SideScroll.Utilities;

namespace SideScroll.Tabs.Tests;

/// <summary>
/// A JSON file is shown as both its contents and a parsed tree. It used to read the file into a
/// string, keep that, and keep the tree parsed from it, so the tab held two copies of the file
/// </summary>
[Category("Tabs")]
public class TabFileJsonTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup() => Initialize(nameof(TabFileJsonTests));

	private static string CreateJsonFile(string json = """{"name":"value","count":3}""")
	{
		string path = Path.Combine(Path.GetTempPath(), nameof(TabFileJsonTests), Path.GetRandomFileName() + ".json");
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, json);
		return path;
	}

	private static async Task<TabModel> LoadModelAsync(string path)
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView view = await viewer.LoadTabAsync(new Call(), new TabFile(path));
		return view.Model;
	}

	private static object? GetItemValue(TabModel model, string name)
	{
		return model.ItemLists
			.SelectMany(list => list.Cast<object?>())
			.OfType<ListItem>()
			.FirstOrDefault(item => item.Name == name)
			?.Value;
	}

	[Test, Description(
		"Contents is a path, the way every other text file's is, rather than the file read into a " +
		"string the tab then holds")]
	public async Task JsonContentsIsAPathRatherThanTheFileText()
	{
		string path = CreateJsonFile();

		TabModel model = await LoadModelAsync(path);

		Assert.That(GetItemValue(model, "Contents"), Is.TypeOf<FilePath>());
	}

	[Test, Description("The parsed tree is still built, that's the point of the second item")]
	public async Task JsonIsStillParsed()
	{
		string path = CreateJsonFile();

		TabModel model = await LoadModelAsync(path);

		Assert.That(GetItemValue(model, "Json"), Is.Not.Null);
	}

	[Test, Description("A text file that isn't JSON is unchanged, it was already a path")]
	public async Task NonJsonTextContentsIsStillAPath()
	{
		string path = Path.Combine(Path.GetTempPath(), nameof(TabFileJsonTests), Path.GetRandomFileName() + ".txt");
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, "plain contents");

		TabModel model = await LoadModelAsync(path);

		Assert.That(GetItemValue(model, "Contents"), Is.TypeOf<FilePath>());
	}

	[Test, Description("Malformed JSON still loads the tab, with the contents readable")]
	public async Task MalformedJsonStillLoadsTheTab()
	{
		string path = CreateJsonFile("{ not json");

		TabModel model = await LoadModelAsync(path);

		Assert.That(GetItemValue(model, "Contents"), Is.TypeOf<FilePath>());
	}
}
