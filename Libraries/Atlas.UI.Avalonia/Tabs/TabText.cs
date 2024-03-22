using Atlas.Core;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;

namespace Atlas.UI.Avalonia.Tabs;

public class TabText : ITab
{
	public string Text;

	public TabText(string text)
	{
		Text = text;
	}

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
