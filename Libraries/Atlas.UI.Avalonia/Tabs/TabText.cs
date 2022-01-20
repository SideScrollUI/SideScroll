using Atlas.Core;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia.Tabs;

public class TabText : ITab
{
	public string Text;

	public TabText(string text)
	{
		Text = text;
	}

	public TabInstance Create() => new Instance(this);

	public class Instance : TabInstance
	{
		public TabText Tab;

		public Instance(TabText tab)
		{
			Tab = tab;
		}

		public override void LoadUI(Call call, TabModel model)
		{
			// No Json highlighting or search control
			/*var textBox = new TabControlTextBox()
			{
				TextWrapping = TextWrapping.Wrap,
				AcceptsReturn = true,
			};
			textBox.SetFormattedJson(Tab.Text);
			model.AddObject(textBox, true);*/

			// wordwrap doesn't work
			var tabAvaloniaEdit = new TabControlAvaloniaEdit(this);
			tabAvaloniaEdit.SetFormattedJson(Tab.Text);

			model.AddObject(tabAvaloniaEdit, true);
		}
	}
}
/*
Markdown support?
- Avalonia.Markdown slow for large text and doesn't allow text selection (yet?)
*/
