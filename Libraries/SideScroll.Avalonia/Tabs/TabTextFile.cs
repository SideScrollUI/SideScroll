using SideScroll.Avalonia.Controls.TextEditor;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Utilities;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;
using SideScroll.Utilities;

namespace SideScroll.Avalonia.Tabs;

/// <summary>A tab that displays the contents of a text file in a read-only editor, with a toolbar button to copy the text to the clipboard.</summary>
/// <param name="filePath">The file whose contents are displayed.</param>
public class TabTextFile(FilePath filePath) : ITab
{
	/// <summary>The file whose contents are displayed.</summary>
	public FilePath FilePath => filePath;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonCopy { get; } = new("Copy", Icons.Svg.Copy);
	}

	private class Instance(TabTextFile tab) : TabInstance
	{
		private TabAvaloniaEdit? _tabAvaloniaEdit;

		public override void LoadUI(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonCopy.ActionAsync = CopyAsync;
			model.AddObject(toolbar);

			_tabAvaloniaEdit = new();
			_tabAvaloniaEdit.Load(tab.FilePath.Path);
			model.AddObject(_tabAvaloniaEdit, true);
		}

		private async Task CopyAsync(Call call)
		{
			await ClipboardUtils.SetTextAsync(TabViewer.Instance, _tabAvaloniaEdit!.Text);

			call.TaskInstance!.ShowMessage("Copied to Clipboard");
		}
	}
}
