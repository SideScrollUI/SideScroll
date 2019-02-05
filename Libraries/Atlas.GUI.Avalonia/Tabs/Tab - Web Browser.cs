using Avalonia.Controls;
using System;
using System.Collections.Generic;
using CefGlue.Avalonia;
using Atlas.Tabs;
using Avalonia.Layout;
using Avalonia.Media;

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
					//HorizontalAlignment = HorizontalAlignment.Left,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					//Width = 500,
					//Height = 500,
					MinHeight = 200,
					MinWidth = 200,
					MaxWidth = 1500,
					//BorderThickness = new Thickness(0),

					StartUrl = tab.uri.AbsoluteUri,
				};
				browser.LoadError += Browser_LoadError;
				browser.LoadingStateChange += Browser_LoadingStateChange;

				tabModel.AddObject(browser, true);
			}

			private void Browser_LoadingStateChange(object sender, LoadingStateChangeEventArgs e)
			{
			}

			private void Browser_LoadError(object sender, LoadErrorEventArgs e)
			{
			}
		}
	}
}
/*
 https://github.com/VitalElement/CefGlue.Core

This component seems to pull in the .NetFramework for windows
*/
