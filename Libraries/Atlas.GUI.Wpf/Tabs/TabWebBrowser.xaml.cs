using System;
using System.Windows.Controls;

namespace Atlas.GUI.Wpf
{
	public partial class TabWebBrowser : UserControl
	{
		public TabWebBrowser(string label, Uri uri)
		{
			InitializeComponent();
			labelName.Content = label;
			textBoxUri.Text = uri.AbsoluteUri;
			webBrowser.Navigate(uri);

			//if (webBrowser.Document)
		}
	}
}
/*
CefSharp looks like a good alternative to this, but it's really hard to setup in AnyPlatform mode
AnyPlatform mode is useful because the designer ONLY runs in x86 (really Microsoft?) but CefSharp wants a single mode
We need to run in x64 mode to use more than 4 GB
*/