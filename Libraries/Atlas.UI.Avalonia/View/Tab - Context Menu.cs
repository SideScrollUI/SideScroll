using Atlas.Core;
using Atlas.Extensions;
using Atlas.Serialize;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Atlas.UI.Avalonia.View
{
	public class TabViewContextMenu : ContextMenu, IStyleable, ILayoutable //, IDisposable
	{
		private TabView tabView;
		private TabInstance tabInstance;

		private CheckBox checkboxAutoLoad;

		Type IStyleable.StyleKey => typeof(ContextMenu);

		public TabViewContextMenu(TabView tabView, TabInstance tabInstance)
		{
			this.tabView = tabView;
			this.tabInstance = tabInstance;

			Initialize();
		}

		private void Initialize()
		{
			//ContextMenu contextMenu = new ContextMenu();

			var list = new AvaloniaList<object>();
			MenuItem menuItemRefresh = new MenuItem() { Header = "_Refresh" };
			menuItemRefresh.Click += MenuItemRefresh_Click;
			list.Add(menuItemRefresh);

			MenuItem menuItemReload = new MenuItem() { Header = "_Reload" };
			menuItemReload.Click += MenuItemReload_Click;
			list.Add(menuItemReload);

			MenuItem menuItemReset = new MenuItem() { Header = "Re_set" };
			menuItemReset.Click += MenuItemReset_Click;
			list.Add(menuItemReset);

#if DEBUG
			MenuItem menuItemDebug = new MenuItem() { Header = "_Debug" };
			menuItemDebug.Click += MenuItemDebug_Click;
			list.Add(menuItemDebug);

			list.Add(new Separator());

			checkboxAutoLoad = new CheckBox()
			{
				IsChecked = tabInstance.project.userSettings.AutoLoad,
			};
			MenuItem menuItemAutoLoad = new MenuItem()
			{
				Header = "_AutoLoad",
				Icon = checkboxAutoLoad,
			};
			menuItemAutoLoad.Click += MenuItemAutoLoad_Click;
			list.Add(menuItemAutoLoad);
#endif

			Items = list;
		}

		private void MenuItemAutoLoad_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			tabInstance.project.userSettings.AutoLoad = !tabInstance.project.userSettings.AutoLoad;
			checkboxAutoLoad.IsChecked = tabInstance.project.userSettings.AutoLoad;
			tabInstance.project.SaveSettings();
		}

		private void MenuItemRefresh_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			tabInstance.Refresh();
		}

		private void MenuItemReload_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			//Initialize(); reloads data without reloading UI
			tabInstance.LoadSettings(); // reloads tab settings, recreates all controls
		}

		private void MenuItemReset_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			//tabInstance.Reset()
			tabView.TabViewSettings = new TabViewSettings()
			{
				Name = tabInstance.Model.Name,
			};
			tabInstance.SaveTabSettings();
			tabInstance.Reintialize(true);
			tabView.Load();
			// Could have parent instance reload children
		}

		private void MenuItemDebug_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			TabModel debugListCollection = new TabModel("Debug");
			TabView clone = Serialize.SerializerMemory.Clone<TabView>(tabInstance.taskInstance.Call, this);
			debugListCollection.AddData(clone);
			Control debugControl = clone.CreateChildControl(debugListCollection, "Debug");
		}

		/*#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				ClearControls();

				tabInstance.Dispose();

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~TabView() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion*/
	}
}

/*

private void UpdateSelectedTabInstances()
{
	tabInstance.children.Clear();
	foreach (Control control in childControls.Values)
	{
		TabView tabView = control as TabView;
		if (tabView != null)
		{
			tabInstance.children.Add(tabView.tabInstance);
		}
	}
}

private void UpdateNearbySplitters(int depth, TabView triggeredControl)
{
	// todo: use max SplitterDistance if not user triggered
	if (call.parent != null)
	{
		foreach (Control control in call.parent.childControls.Values)
		{
			TabView tabView = control as TabView;
			if (tabView != null)
			{
				if (tabView != triggeredControl)
					tabView.splitContainer.SplitterDistance = splitContainer.SplitterDistance;
			}
		}
	}
}

private void TabInstance_OnClearSelection(object sender, EventArgs e)
{
	foreach (TabDataGrid tabData in tabDatas)
	{
		tabData.SelectedItem = null; // dataGrid.UnselectAll() doesn't work
	}
}

private void horizontalSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
{
	//gridColumnLists.MaxWidth = // window width
	gridColumnLists.Width = new GridLength(1, GridUnitType.Auto);
	SaveSplitterDistance();
}

private void horizontalSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
{
	SaveSplitterDistance();

	/*UpdateNearbySplitters(1, this);
	tabConfiguration.SplitterDistance = splitContainer.SplitterDistance;
	foreach (Control control in tableLayoutPanelLeft.Controls)
	{
		control.AutoSize = false;
		control.Width = splitContainer.SplitterDistance;
		control.AutoSize = true;
	}*//*
}

private void verticalGridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
{
	//SaveSplitterDistance();
}

private void gridParentControls_MouseDown(object sender, MouseButtonEventArgs e)
{
	if (tabDatas.Count > 0)
	{
		tabDatas[0].dataGrid.Focus();
		e.Handled = true;
	}
}

private void UserControl_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
{
	if (!allowAutoScrolling)
		e.Handled = true;
}

*/
