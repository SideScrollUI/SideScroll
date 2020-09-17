using Atlas.Core;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Json;

namespace Atlas.UI.Avalonia.Tabs
{
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
				//textEditor.TextBlock.FontSize = 30;

				// No scrollbars
				/*var textBox = new TextBox()
				{
					VerticalAlignment = VerticalAlignment.Stretch,
					HorizontalAlignment = HorizontalAlignment.Left,
					Text = tab.text,
					TextWrapping = TextWrapping.Wrap,
					//IsReadOnly = true,
					MinWidth = 50,
					MaxWidth = 1500,
					//Width = 1500,
					BorderThickness = new Thickness(0),
					
				};
				tabModel.AddObject(textBox, true);*/

				//this.splitControls.AddControl(textBox, true, SeparatorType.Spacer);

				//tabAvaloniaEdit.textEditor.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top;
				//textBox.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;


				// wordwrap doesn't work
				var tabAvaloniaEdit = new TabControlAvaloniaEdit(this);
				//tabAvaloniaEdit.textEditor.VerticalAlignment = VerticalAlignment.Top;
				//tabAvaloniaEdit.textEditor.VerticalAlignment = VerticalAlignment.Stretch;
				//tabAvaloniaEdit.textEditor.IsReadOnly = true; // todo: allow editing?
				tabAvaloniaEdit.Text = Tab.Text;

				if (Tab.Text.StartsWith("{"))
				{
					try
					{
						JsonValue jsonValue = JsonValue.Parse(Tab.Text);
						if (jsonValue != null)
						{
							tabAvaloniaEdit.TextEditor.FontFamily = new FontFamily("Courier New");
						}
					}
					catch
					{
					}
				}

				model.AddObject(tabAvaloniaEdit, true);

				/*ShowLineNumbers = true;
				SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
				TextArea.TextEntering += textEditor_TextArea_TextEntering;
				TextArea.IndentationStrategy = new Indentation.CSharp.CSharpIndentationStrategy();*/

				//SearchPanel.Install(this.textEditor);
			}
		}
	}
}
/*
Markdown support?
*/
