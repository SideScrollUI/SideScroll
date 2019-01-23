using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlText : UserControl
	{
		public TabControlText(string label, string text) : base()
		{
			this.MaxWidth = 2000;
			//this.Text = Text;
			//InitializeComponent();
			//labelName.Content = label;
			TextBox textBox = new TextBox()
			{
				Text = text,
				TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
			};
			this.Content = textBox;
		}
	}
}
/*
Todo: replace with Tab Avalonia Edit
*/
