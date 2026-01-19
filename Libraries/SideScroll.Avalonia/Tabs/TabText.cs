using SideScroll.Avalonia.Controls.TextEditor;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Utilities;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Avalonia.Tabs;

public class TabText(string text) : ITab
{
	public string Text => text;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonCopy { get; } = new("Copy", Icons.Svg.Copy);
	}

	private class Instance(TabText tab) : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			model.MinDesiredWidth = 100;
			model.MaxDesiredWidth = 1000;

			Toolbar toolbar = new();
			toolbar.ButtonCopy.ActionAsync = CopyAsync;
			model.AddObject(toolbar);

			var tabAvaloniaEdit = new TabAvaloniaEdit();
			tabAvaloniaEdit.SetFormatted(tab.Text);
			model.AddObject(tabAvaloniaEdit, true);
		}

		private async Task CopyAsync(Call call)
		{
			await ClipboardUtils.SetTextAsync(TabViewer.Instance, tab.Text);

			call.TaskInstance!.ShowMessage("Copied to Clipboard");
		}
	}
}
