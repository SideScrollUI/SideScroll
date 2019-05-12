using System;
using Atlas.Core;
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
				tabModel.Bookmarks = new BookmarkCollection(project);
			}

			public override void Load(Call call)
			{
				toolbar = new TabControlBookmarksToolbar();
				toolbar.buttonLoadAdd.Click += ButtonLoadAdd_Click;
				toolbar.buttonCopyClipBoard.Click += ButtonCopyClipBoard_Click; ;
				tabModel.AddObject(toolbar);

				bookmarkSettings = new TabControlBookmarkSettings(this);
				tabModel.AddObject(bookmarkSettings);

				tabModel.AddData(tabModel.Bookmarks.Names);
			}

			private void ButtonLoadAdd_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
				bookmarkSettings.IsVisible = true;
			}

			private void ButtonCopyClipBoard_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
			}

			public object CreateControl(object value, out string label)
			{
				var bookmark = (ViewBookmarkName)value;
				label = bookmark.Name;

				TabInstance tabInstance = tab.iTab.Create();
				tabInstance.tabBookmark = bookmark.Bookmark.tabBookmark;
				//tabInstance.LoadBookmark()
				return tabInstance;
			}
		}
	}
}
