using Eto.Forms;
using System;

namespace Atlas.GUI.Eto
{
	public class TabText : RichTextArea
	{
		public TabText(string label) : base()
		{
			//this.Text = Text;
			//InitializeComponent();
			//labelName.Content = label;
		}
	}
}
/*
RichTextArea loads faster but edits slower
TabText loads 10x slower but edits faster
*/
