using Atlas.Core;
using Atlas.Extensions;
using Atlas.Serialize;
using System;

namespace Atlas.Tabs
{
	// Display Class
	public class TabBookmarkItem : ITab, IInnerTab
	{
		//[ButtonColumn("-")]
		public event EventHandler<EventArgs> OnDelete;

		[ButtonColumn("-")]
		public void Delete()
		{
			OnDelete?.Invoke(this, null);
		}

		[WordWrap]
		public string Path => Bookmark.Path;

		[Formatted]
		public TimeSpan Age => Bookmark.TimeStamp.Age();

		[HiddenColumn]
		public Bookmark Bookmark { get; set; }

		private Project Project { get; set; }

		[HiddenColumn]
		public ITab Tab => Bookmark.TabBookmark.Tab;

		public override string ToString() => Bookmark.Name ?? Bookmark.Path;

		public TabBookmarkItem(Bookmark bookmark, Project project)
		{
			Bookmark = bookmark;
			Project = project;
		}

		public TabInstance Create()
		{
			if (Bookmark.Type == null)
				return null;

			if (!typeof(ITab).IsAssignableFrom(Bookmark.Type))
				throw new Exception("Bookmark.Type must implement ITab");

			var call = new Call();
			Bookmark bookmarkCopy = Bookmark.DeepClone(call, true); // This will get modified as users navigate

			ITab tab = bookmarkCopy.TabBookmark.Tab;
			if (tab == null)
				tab = (ITab)Activator.CreateInstance(bookmarkCopy.Type);

			if (tab is IReload reloadable)
				reloadable.Reload();

			TabInstance tabInstance = tab.Create();
			tabInstance.Project = Project.Open(bookmarkCopy); 
			tabInstance.iTab = this;
			tabInstance.IsRoot = true;
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
