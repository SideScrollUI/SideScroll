using System;
using System.Linq;
using Atlas.Core;
using Atlas.Serialize;
using Atlas.Start.Avalonia.Tabs;
using Atlas.Tabs;
using Atlas.UI.Avalonia.View;
using Avalonia;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabBookmarks : ITab
	{
		public Project project;
		public ITab iTab;
		public Linker linker;

		public TabBookmarks(Project project, ITab iTab, Linker linker)
		{
			this.project = project;
			this.iTab = iTab;
			this.linker = linker;
		}

		public TabInstance Create() => new Instance(this);

		public class Instance : TabInstance, ITabCreator
		{
			private TabControlBookmarkSettings bookmarkSettings;
			private TabControlBookmarksToolbar toolbar;
			private TabBookmarks tab;
			private Bookmark currentBookMark;

			public Instance(TabBookmarks tab)
			{
				this.tab = tab;
				this.project = tab.project;
				Model.Name = "Bookmarks";
				Model.Bookmarks = new BookmarkCollection(project);
				//tabModel.Bookmarks.OnDelete
				//var currentBookMark = this.CreateBookmark();
				currentBookMark = new Bookmark()
				{
					Name = "Current",
				};
				Model.Bookmarks.Items.Insert(0, new TabBookmarkItem(currentBookMark));
			}

			public override void LoadUI(Call call, TabModel model)
			{
				toolbar = new TabControlBookmarksToolbar();
				toolbar.buttonAdd.Add(ButtonAdd_Click);
				toolbar.buttonLink.Add(ButtonLink_Click);
				toolbar.buttonImport.Add(ButtonImport_Click);
				Model.AddObject(toolbar);

				bookmarkSettings = new TabControlBookmarkSettings(this);
				Model.AddObject(bookmarkSettings);

				Model.AddData(Model.Bookmarks.Items);

				/*foreach (var item in tabModel.Bookmarks.Items)
				{
					item.OnDelete += Item_OnDelete;
				}*/
			}

			// move into BookmarkCollection?
			/*private void Item_OnDelete(object sender, EventArgs e)
			{
				TabBookmarkItem bookmark = (TabBookmarkItem)sender;
				project.DataApp.Delete<Bookmark>(null, bookmark.Bookmark.Name);
				tabModel.Bookmarks.Reload();
				tabModel.Bookmarks.Items.Insert(0, new TabBookmarkItem(currentBookMark));
				//tabModel.Bookmarks.Items.Remove(new TabBookmarkItem(bookmark));
				//Reload();
			}*/

			private void ButtonAdd_Click(Call call)
			{
				var bookmark = CreateBookmark();
				//tabModel.Bookmarks.Names.Add(new ViewBookmark(bookmark));
				//bookmarkSettings.IsVisible = true;
				bookmarkSettings.ShowBookmarkSettings(bookmark);
			}

			private void ButtonLink_Click(Call call)
			{
				var bookmark = CreateBookmark();
				string uri = tab.linker.GetLinkUri(bookmark);
				((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(uri);
			}

			private void ButtonImport_Click(Call call)
			{
				string clipboardText = ((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).GetTextAsync().Result;
				Bookmark bookmark = tab.linker.GetBookmark(clipboardText);
				if (bookmark == null)
					return;
				SelectBookmark(bookmark.tabBookmark);
				//tabView.tabInstance.tabBookmark = bookmark.tabBookmark;
				//Reload();
			}

			public object CreateControl(object value, out string label)
			{
				var bookmarkItem = (TabBookmarkItem)value;
				label = bookmarkItem.Name;

				TabInstance tabInstance = tab.iTab.Create();
				tabInstance.project = tab.project;
				tabInstance.tabBookmark = bookmarkItem.Bookmark.tabBookmark.Clone<TabBookmark>(taskInstance.Call); // bookmark specified here will get auto loaded
				//tabInstance.LoadBookmark()
				return new TabView(tabInstance);
			}

			public override Bookmark CreateBookmark()
			{
				if (childTabInstances.Values.Count > 0)
					return childTabInstances.Values.First().CreateBookmark();

				return base.CreateBookmark();
				/*Bookmark bookmark = new Bookmark();
				//bookmark.tabBookmark.Name = Label;
				GetBookmark(bookmark.tabBookmark);
				bookmark = bookmark.Clone<Bookmark>(taskInstance.call); // sanitize
				return bookmark;*/
			}

			public override void SelectBookmark(TabBookmark tabBookmark)
			{
				if (childTabInstances.Values.Count > 0)
				{
					currentBookMark.tabBookmark = tabBookmark;
					/*SelectItem(currentBookMark);
					var childTab = childTabInstances.Values.First();
					childTab.tabBookmark = tabBookmark;
					childTab.Reload();
					//childTab.SelectBookmark(tabBookmark);*/
					Reload();
				}
				else
					base.SelectBookmark(tabBookmark);
			}
		}
	}
}
