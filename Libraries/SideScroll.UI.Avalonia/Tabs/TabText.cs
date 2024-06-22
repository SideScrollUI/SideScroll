using SideScroll.Tabs;
using SideScroll.UI.Avalonia.Controls.TextEditor;

namespace SideScroll.UI.Avalonia.Tabs;

public class TabText(string text) : ITab
{
	public string Text = text;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabText tab) : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			model.MinDesiredWidth = 100;
			model.MaxDesiredWidth = 1000;

			var tabAvaloniaEdit = new TabControlAvaloniaEdit(this);
			tabAvaloniaEdit.SetFormatted(tab.Text);

			model.AddObject(tabAvaloniaEdit, true);
		}
	}
}
/*
Markdown support?
- Avalonia.Markdown slow for large text and doesn't allow text selection (yet?)
*/
