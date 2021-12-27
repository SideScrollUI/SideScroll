using Atlas.Serialize;
using Atlas.Tabs;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Interactivity;
using System;

namespace Atlas.UI.Avalonia.View
{
	public class TabMenuItem : MenuItem, IStyleable
	{
		Type IStyleable.StyleKey => typeof(MenuItem);

		public TabMenuItem(string header = null)
		{
			Header = header;
			Items = null; // Clear to avoid weak event handler leaks
		}
	}

	public class TabViewContextMenu : ContextMenu, IStyleable, ILayoutable, IDisposable
	{
		public TabView TabView;
		public TabInstance TabInstance;

		private CheckBox _checkboxAutoLoad;

		Type IStyleable.StyleKey => typeof(ContextMenu);

		public TabViewContextMenu(TabView tabView, TabInstance tabInstance)
		{
			TabView = tabView;
			TabInstance = tabInstance;

			Initialize();
		}

		private void Initialize()
		{
			var list = new AvaloniaList<object>();

			var menuItemRefresh = new TabMenuItem("_Refresh");
			menuItemRefresh.Click += MenuItemRefresh_Click;
			list.Add(menuItemRefresh);

			var menuItemReload = new TabMenuItem("_Reload");
			menuItemReload.Click += MenuItemReload_Click;
			list.Add(menuItemReload);

			var menuItemReset = new TabMenuItem("Re_set");
			menuItemReset.Click += MenuItemReset_Click;
			list.Add(menuItemReset);

#if DEBUG
			var menuItemDebug = new TabMenuItem("_Debug");
			menuItemDebug.Click += MenuItemDebug_Click;
			list.Add(menuItemDebug);

			list.Add(new Separator());

			_checkboxAutoLoad = new CheckBox()
			{
				IsChecked = TabInstance.Project.UserSettings.AutoLoad,
			};
			var menuItemAutoLoad = new TabMenuItem()
			{
				Header = "_AutoLoad",
				Icon = _checkboxAutoLoad,
			};
			menuItemAutoLoad.Click += MenuItemAutoLoad_Click;
			list.Add(menuItemAutoLoad);
#endif

			Items = list;
		}

		private void MenuItemAutoLoad_Click(object sender, RoutedEventArgs e)
		{
			TabInstance.Project.UserSettings.AutoLoad = !TabInstance.Project.UserSettings.AutoLoad;
			_checkboxAutoLoad.IsChecked = TabInstance.Project.UserSettings.AutoLoad;
			TabInstance.Project.SaveSettings();
		}

		private void MenuItemRefresh_Click(object sender, RoutedEventArgs e)
		{
			TabInstance.Refresh();
		}

		private void MenuItemReload_Click(object sender, RoutedEventArgs e)
		{
			TabInstance.LoadSettings(true); // reloads tab settings, recreates all controls
		}

		private void MenuItemReset_Click(object sender, RoutedEventArgs e)
		{
			TabView.TabViewSettings = new TabViewSettings();
			TabInstance.SaveTabSettings();
			TabInstance.Reintialize(true);
			TabView.Load();
			// Could have parent instance reload children
		}

		private void MenuItemDebug_Click(object sender, RoutedEventArgs e)
		{
			var debugModel = new TabModel("Debug");
			TabView clone = TabView.DeepClone();
			debugModel.AddData(clone);
			//Control debugControl = clone.CreateChildControl(debugModel, "Debug");
		}

		public void Dispose()
		{
			TabView = null;
			TabInstance = null;
			Items = null;
		}
	}
}

