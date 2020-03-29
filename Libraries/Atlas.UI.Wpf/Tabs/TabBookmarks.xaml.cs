using Atlas.Core;
using Atlas.Tabs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Atlas.UI.Wpf
{
	public partial class TabBookmarks : UserControl
	{
		public Project project;
		public TabInstance tabInstance;
		public TabModel tabModel;

		//private TabConfiguration tabConfiguration;
		
		public TabData tabData;

		public TabBookmarks()
		{
			InitializeComponent();
		}

		public void Initialize(TabInstance tabInstance)
		{
			this.tabInstance = tabInstance;
			this.project = tabInstance.project;
			this.tabModel = tabInstance.Model;
			AddListData();
		}

		public override string ToString()
		{
			return tabModel.Name;
		}

		public void Reload()
		{
			if (tabData != null)
			{
				grid.Children.Remove(tabData);
			}
			AddListData();
		}

		protected void AddListData()
		{
			tabModel.Bookmarks = tabModel.Bookmarks ?? new BookmarkCollection(tabInstance.project);
			tabData = new TabData(tabInstance, tabModel.Bookmarks.Items, new TabDataSettings());
			tabData.autoSelectFirst = false;
			tabData.OnSelectionChanged += OnSelectedBookmarkChanged;
			tabData.Initialize();
			Grid.SetRow(tabData, 0);
			grid.Children.Add(tabData);
		}

		private void OnSelectedBookmarkChanged(object sender, EventArgs e)
		{
			List<Bookmark> bookmarks = new List<Bookmark>();
			foreach (ViewBookmark name in tabData.SelectedItems)
			{
				Bookmark bookmark = project.DataApp.Load<Bookmark>(name.Name, tabInstance.taskInstance.Call);
				if (bookmark != null)
					bookmarks.Add(bookmark);
			}
			if (bookmarks.Count == 0)
				return;

			// Show merged set of selected bookmarks
			Bookmark selectedBookmarks = new Bookmark();
			selectedBookmarks.MergeBookmarks(bookmarks);
			tabInstance.SelectBookmark(selectedBookmarks.tabBookmark);
		}

		private void ToolBar_Loaded(object sender, RoutedEventArgs e)
		{
			RemoveToolbarOverflow((ToolBar)sender);
		}

		private void RemoveToolbarOverflow(ToolBar toolbar)
		{
			foreach (FrameworkElement a in toolbar.Items)
			{
				ToolBar.SetOverflowMode(a, OverflowMode.Never);
			}
			var overflowGrid = toolbar.Template.FindName("OverflowGrid", toolbar) as FrameworkElement;
			if (overflowGrid != null)
			{
				overflowGrid.Visibility = Visibility.Collapsed;
			}
		}

		private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (tabData != null && tabData.SelectedItems.Count > 0);
		}
		
		private void New_Executed(object sender, ExecutedRoutedEventArgs e)
		{
		}

		private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
		{

		}

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
		{
		}

		private void Cancel_Executed(object sender, ExecutedRoutedEventArgs e)
		{
		}

		private void Button_NewClick(object sender, RoutedEventArgs e)
		{
			panelNew.Visibility = Visibility.Visible;
			//textBoxName.Clear();
			textBoxName.Text = project.Navigator.Current.Changed;
			textBoxName.Focus();
			RemoveToolbarOverflow(toolbarSave);
		}

		private void Button_DeleteClick(object sender, RoutedEventArgs e)
		{
			foreach (ViewBookmark bookmarkName in tabData.SelectedItems)
				project.DataApp.Delete(typeof(Bookmark), bookmarkName.Name);
			tabModel.Bookmarks.Reload();
		}

		private void Button_NewSave(object sender, RoutedEventArgs e)
		{
			Bookmark bookmark = tabInstance.RootInstance.CreateBookmark();
			bookmark.Name = textBoxName.Text;
			project.DataApp.Save(bookmark.Name, bookmark);

			tabModel.Bookmarks.Items.Add(new TabBookmarkItem(bookmark));
			panelNew.Visibility = Visibility.Collapsed;
		}

		private void Button_NewCancel(object sender, RoutedEventArgs e)
		{
			panelNew.Visibility = Visibility.Collapsed;
		}
	}
}
