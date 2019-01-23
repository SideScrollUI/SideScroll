using Avalonia.Controls;
using System;
using System.Collections.Generic;
using CefGlue.Avalonia;
using Atlas.Tabs;
using Avalonia.Layout;

namespace Atlas.GUI.Avalonia.Tabs
{
	public class TabWebBrowser : ITab // Todo: Switch to TabContainer or TabView
	{
		public Uri uri;

		public TabWebBrowser(Uri uri)
		{
			this.uri = uri;
		}

		public TabInstance Create() { return new Instance(this); }

		public class Instance : TabInstance
		{
			private TabWebBrowser tab;

			public Instance(TabWebBrowser tab)
			{
				this.tab = tab;
			}

			//private ItemCollection<ListItem> items = new ItemCollection<ListItem>();
			//private CustomControl control;
			//private TabChart tabChart;

			public override void Load()
			{
				//labelName.Content = label;
				//textBoxUri.Text = uri.AbsoluteUri;
				//webBrowser.Navigate(uri);

				AvaloniaCefBrowser browser = new AvaloniaCefBrowser()
				{
					VerticalAlignment = VerticalAlignment.Stretch,
					HorizontalAlignment = HorizontalAlignment.Left,
					MinWidth = 50,
					MaxWidth = 1500,
					//BorderThickness = new Thickness(0),


					StartUrl = tab.uri.AbsoluteUri,
				};

				tabModel.AddObject(browser);
			}
		}
	}
}
/*
 Doesn't work with nightly build branch we need for DataGrid
 https://github.com/VitalElement/CefGlue.Core

	https://github.com/VitalElement/CefGlue.Core/commits/master
	Last commit Nov 3, 2017
	Almost a year since last updated
*/
