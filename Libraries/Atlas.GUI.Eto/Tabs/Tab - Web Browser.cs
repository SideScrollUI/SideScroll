using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.GUI.Eto
{
	public class TabWebBrowser : WebView
	{
		public TabWebBrowser(string label, Uri uri)
		{
			this.Url = uri;
			//InitializeComponent();
			//labelName.Content = label;
			//textBoxUri.Text = uri.AbsoluteUri;
			//webBrowser.Navigate(uri);

			//if (webBrowser.Document)
		}
	}
}
