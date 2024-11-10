using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;
using SideScroll.UI.Avalonia.Controls.TextEditor;
using SideScroll.UI.Avalonia.Utilities;

namespace SideScroll.UI.Avalonia.Tabs;

public class TabText(string text) : ITab
{
	public string Text = text;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonCopy { get; set; } = new("Copy", Icons.Svg.Copy);
	}

	public class Instance(TabText tab) : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			model.MinDesiredWidth = 100;
			model.MaxDesiredWidth = 1000;

			Toolbar toolbar = new();
			toolbar.ButtonCopy.ActionAsync = CopyAsync;
			model.AddObject(toolbar);

			var tabAvaloniaEdit = new TabControlAvaloniaEdit(this);
			tabAvaloniaEdit.SetFormatted(tab.Text);

			model.AddObject(tabAvaloniaEdit, true);
		}

		private async Task CopyAsync(Call call)
		{
			await ClipboardUtils.SetTextAsync(BaseWindow.Instance, tab.Text);
		}
	}
}
/*
Markdown support?
- Avalonia.Markdown slow for large text and doesn't allow text selection (yet?)
*/
