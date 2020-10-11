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
	public class TabViewContextMenu : ContextMenu, IStyleable, ILayoutable
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
			var menuItemRefresh = new MenuItem() { Header = "_Refresh" };
			menuItemRefresh.Click += MenuItemRefresh_Click;
			list.Add(menuItemRefresh);

			var menuItemReload = new MenuItem() { Header = "_Reload" };
			menuItemReload.Click += MenuItemReload_Click;
			list.Add(menuItemReload);

			var menuItemReset = new MenuItem() { Header = "Re_set" };
			menuItemReset.Click += MenuItemReset_Click;
			list.Add(menuItemReset);

#if DEBUG
			var menuItemDebug = new MenuItem() { Header = "_Debug" };
			menuItemDebug.Click += MenuItemDebug_Click;
			list.Add(menuItemDebug);

			list.Add(new Separator());

			_checkboxAutoLoad = new CheckBox()
			{
				IsChecked = TabInstance.Project.UserSettings.AutoLoad,
			};
			var menuItemAutoLoad = new MenuItem()
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
			TabInstance.LoadSettings(); // reloads tab settings, recreates all controls
		}

		private void MenuItemReset_Click(object sender, RoutedEventArgs e)
		{
			//tabInstance.Reset()
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
	}
}

