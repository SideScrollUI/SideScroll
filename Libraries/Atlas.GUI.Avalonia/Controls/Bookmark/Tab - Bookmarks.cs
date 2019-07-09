using System;
using Atlas.Core;
using Atlas.GUI.Avalonia.View;
using Atlas.Start.Avalonia.Tabs;
using Atlas.Tabs;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabBookmarks : ITab
	{
		public Project project;
		public ITab iTab;

		public TabBookmarks(Project project, ITab iTab)
		{
			this.project = project;
			this.iTab = iTab;
		}

		public TabInstance Create() { return new Instance(this); }

		public class Instance : TabInstance, ITabCreator
		{
			private TabControlBookmarkSettings bookmarkSettings;
			private TabControlBookmarksToolbar toolbar;
			private TabBookmarks tab;

			public Instance(TabBookmarks tab)
			{
				this.tab = tab;
				this.project = tab.project;
				tabModel.Name = "Bookmarks";
				tabModel.Bookmarks = new BookmarkCollection(project);
				//var currentBookMark = this.CreateBookmark();
				var currentBookMark = new Bookmark()
				{
					Name = "Current",
					//tabBookmark = tab,
				};
				tabModel.Bookmarks.Items.Insert(0, new TabBookmarkItem(currentBookMark));
			}

			public override void Load(Call call)
			{
				toolbar = new TabControlBookmarksToolbar();
				toolbar.buttonLoadAdd.Click += ButtonLoadAdd_Click;
				toolbar.buttonCopyClipBoard.Click += ButtonCopyClipBoard_Click;
				tabModel.AddObject(toolbar);

				bookmarkSettings = new TabControlBookmarkSettings(this);
				tabModel.AddObject(bookmarkSettings);

				tabModel.AddData(tabModel.Bookmarks.Items);
			}

			private void ButtonLoadAdd_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
				var bookmark = this.CreateBookmark();
				bookmark.Name = bookmark.Address;
				//tabModel.Bookmarks.Names.Add(new ViewBookmark(bookmark));
				//bookmarkSettings.IsVisible = true;
				bookmarkSettings.ShowBookmark(bookmark);
			}

			private void ButtonCopyClipBoard_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
			}

			public object CreateControl(object value, out string label)
			{
				var bookmark = (TabBookmarkItem)value;
				label = bookmark.Name;

				TabInstance tabInstance = tab.iTab.Create();
				tabInstance.project = tab.project;
				tabInstance.tabBookmark = bookmark.Bookmark.tabBookmark; // bookmark specified here will get auto loaded
				//tabInstance.LoadBookmark()
				return new TabView(tabInstance);
			}
		}
	}
}
