using Atlas.Core;
using Atlas.Extensions;
using Atlas.Serialize;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs
{
	// Display Class
	public class TabBookmarkItem : ITab, InnerTab
	{
		//[ButtonColumn("-")]
		public event EventHandler<EventArgs> OnDelete;

		[ButtonColumn("-")]
		public void Delete()
		{
			OnDelete?.Invoke(this, null);
		}

		[Name("Bookmark"), WordWrap]
		public string Name => Bookmark.Name ?? Bookmark.Address;
		public TimeSpan Age => DateTime.UtcNow.Subtract(Bookmark.TimeStamp).Trim();
		[HiddenColumn]
		public Bookmark Bookmark { get; set; }
		private Project Project { get; set; }

		[HiddenColumn]
		public ITab Tab => Bookmark.TabBookmark.Tab;

		public override string ToString() => Name;

		public TabBookmarkItem(Bookmark bookmark, Project project)
		{
			Bookmark = bookmark;
			Project = project;
		}

		public TabInstance Create()
		{
			if (Bookmark.Type == null)
				return null;

			var bookmarkCopy = Bookmark.DeepClone<Bookmark>(); // This will get modified as users navigate

			ITab tab = bookmarkCopy.TabBookmark.Tab;
			if (tab == null)
				tab = (ITab)Activator.CreateInstance(bookmarkCopy.Type);
			TabInstance tabInstance = tab.Create();
			tabInstance.Project = Project.Open(bookmarkCopy); 
			tabInstance.iTab = this;
			//tabInstance.ParentTabInstance = this;
			tabInstance.SelectBookmark(bookmarkCopy.TabBookmark);
			return tabInstance;
		}

		/*public class Instance : TabInstance
		{
			private TabBookmarkItem tab;

			public Instance(TabBookmarkItem tab)
			{
				this.tab = tab;
			}

			public override void Load(Call call, TabModel model)
			{
				ITab bookmarkTab = ((ITab)Activator.CreateInstance(tab.Bookmark.Type)).Create();
				bookmarkTab.Create();
				SelectBookmark(tab.Bookmark.tabBookmark);
				//model.Items = items;
			}
		}*/
	}
}
