using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Serialize;

namespace SideScroll.Tabs.Bookmarks;

// Display Class
public class TabBookmarkItem(Bookmark bookmark, Project project) : ITab, IInnerTab
{
	//[ButtonColumn("-")]
	public event EventHandler<EventArgs>? OnDelete;

	[ButtonColumn("-")]
	public void Delete()
	{
		OnDelete?.Invoke(this, EventArgs.Empty);
	}

	[DataKey, WordWrap]
	public string Path => Bookmark.Path;

	[Formatted]
	public TimeSpan Age => Bookmark.TimeStamp.Age();

	[HiddenColumn]
	public Bookmark Bookmark => bookmark;

	[Hidden]
	public Project Project => project;

	[HiddenColumn]
	public ITab? Tab => Bookmark.TabBookmark.Tab;

	public override string ToString() => Bookmark.Name ?? Bookmark.Path;

	public TabInstance Create()
	{
		return Create(Bookmark, Project, this);
	}

	public static TabInstance Create(Bookmark bookmark, Project project, ITab iTab)
	{
		if (bookmark.Type == null)
		{
			throw new ArgumentNullException("Bookmark.Type");
		}

		if (!typeof(ITab).IsAssignableFrom(bookmark.Type))
		{
			throw new Exception("Bookmark.Type must implement ITab");
		}

		var call = new Call();
		Bookmark bookmarkCopy = bookmark.DeepClone(call, true)!; // This will get modified as users navigate
		bookmarkCopy.Reinitialize();

		ITab tab = bookmarkCopy.TabBookmark.Tab ?? (ITab)Activator.CreateInstance(bookmarkCopy.Type!)!;

		if (tab is IReload reloadable)
		{
			reloadable.Reload();
		}

		TabInstance tabInstance = tab.Create();
		tabInstance.Project = project.Open(bookmarkCopy);
		tabInstance.iTab = iTab;
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
