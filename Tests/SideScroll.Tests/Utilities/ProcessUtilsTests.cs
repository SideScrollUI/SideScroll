using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Tests.Utilities;

[Category("Core")]
public class ProcessUtilsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ProcessUtils");
	}

	[Test]
	public void OpenFolder_NonExistentFolder_DoesNotThrow()
	{
		Assert.DoesNotThrow(() => ProcessUtils.OpenFolder("C:\\this_folder_should_not_exist_xyz_123"));
	}

	[Test, Platform("Win"), Description(
		"A backslash before the closing quote escapes it, so a folder ending in a separator reached " +
		"explorer as C:\\Users\\Public\" and it opened its default folder instead")]
	public void GetExplorerArgument_TrimsATrailingSeparator()
	{
		Assert.That(ProcessUtils.GetExplorerArgument(@"C:\Users\Public\", null), Is.EqualTo(@"""C:\Users\Public"""));
		Assert.That(ProcessUtils.GetExplorerArgument(@"C:\Users\Public", null), Is.EqualTo(@"""C:\Users\Public"""));
		Assert.That(ProcessUtils.GetExplorerArgument("C:/Users/Public/", null), Is.EqualTo(@"""C:\Users\Public"""));
	}

	[Test, Description(
		"The Linux and macOS branches of OpenBrowser() and OpenFolder() passed the value through the " +
		"arguments string, which is split on whitespace and quotes before the child sees it")]
	public void CreateStartInfoPassesTheValueAsOneArgument()
	{
		string url = "file:///home/me/my docs/page \"quoted\".html";

		var startInfo = ProcessUtils.CreateStartInfo("xdg-open", url);

		Assert.That(startInfo.FileName, Is.EqualTo("xdg-open"));
		Assert.That(startInfo.ArgumentList, Is.EqualTo(new[] { url }), "one argument, spaces and quotes intact");
		Assert.That(startInfo.Arguments, Is.Empty, "not the tokenized string form");
		Assert.That(startInfo.UseShellExecute, Is.False);
	}

	[Test, Platform("Win"), Description("A root keeps its separator, so it's doubled to survive the quote")]
	public void GetExplorerArgument_DoublesARootSeparator()
	{
		Assert.That(ProcessUtils.GetExplorerArgument(@"C:\", null), Is.EqualTo(@"""C:\\"""));
	}

	[Test, Platform("Win")]
	public void GetExplorerArgument_SelectsAFileInAFolderWithATrailingSeparator()
	{
		string folder = Path.Combine(Environment.CurrentDirectory, "ExplorerArgument") + '\\';
		Directory.CreateDirectory(folder);
		string filePath = Path.Combine(folder, "file.txt");
		File.WriteAllText(filePath, "");

		Assert.That(ProcessUtils.GetExplorerArgument(folder, "file.txt"), Is.EqualTo("/select,\"" + filePath + '"'));
	}

	[Test]
	public void GetDotnetRuntimes_RunsSuccessfully()
	{
		// Since we cannot mock the underlying dotnet process output easily here,
		// we verify that the method executes without throwing exceptions on the current machine.
		Assert.DoesNotThrow(() =>
		{
			var runtimes = ProcessUtils.GetDotnetRuntimes();
			// We should have at least one runtime if tests are running
			Assert.That(runtimes, Is.Not.Null);
		});
	}

	[Test, Description("Runtime paths are usable paths rather than bracketed dotnet display fields")]
	public void GetDotnetRuntimes_ReturnsUnwrappedPaths()
	{
		List<DotnetRuntimeInfo> runtimes = ProcessUtils.GetDotnetRuntimes();

		Assert.That(runtimes, Is.Not.Empty);
		Assert.That(runtimes.Select(runtime => runtime.Path), Has.None.StartsWith("["));
		Assert.That(runtimes.Select(runtime => runtime.Path), Has.None.EndsWith("]"));
		Assert.That(runtimes.All(runtime => Directory.Exists(runtime.Path)), Is.True);
	}
}
