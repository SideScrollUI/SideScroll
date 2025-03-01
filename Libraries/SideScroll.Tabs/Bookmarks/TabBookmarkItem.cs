using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Serialize;

namespace SideScroll.Tabs.Bookmarks;

// Display Class
public class TabBookmarkItem(LinkedBookmark linkedBookmark, Project project) : ITab, IInnerTab
{
	//[ButtonColumn("-")]
	public event EventHandler<EventArgs>? OnDelete;

	[ButtonColumn("-")]
	public void Delete()
	{
		OnDelete?.Invoke(this, EventArgs.Empty);
	}

	[DataKey, WordWrap]
	public string LinkId => linkedBookmark.LinkId;

	[Formatted]
	public TimeSpan Age => Bookmark.TimeStamp.Age();

	[HiddenColumn]
	public LinkedBookmark LinkedBookmark => linkedBookmark;

	[HiddenColumn]
	public Bookmark Bookmark => linkedBookmark.Bookmark;

	[Hidden]
	public Project Project => project;

	[HiddenColumn]
	public ITab? Tab => Bookmark.TabBookmark.Tab;

	public override string ToString() => Bookmark.Name ?? Bookmark.Path;

	public TabInstance Create()
	{
		return Create(LinkedBookmark, Project, this);
	}

	public static TabInstance Create(LinkedBookmark linkedBookmark, Project project, ITab iTab)
	{
		Type? tabType = linkedBookmark.Bookmark.Type;
		if (tabType == null)
		{
			throw new ArgumentNullException("Bookmark.Type");
		}

		if (!typeof(ITab).IsAssignableFrom(tabType))
		{
			throw new Exception("Bookmark.Type must implement ITab");
		}

		var call = new Call();
		LinkedBookmark linkedBookmarkCopy = linkedBookmark.DeepClone(call, true)!; // This will get modified as users navigate
		Bookmark bookmark = linkedBookmarkCopy.Bookmark;
		bookmark.Reinitialize();

		ITab tab = bookmark.TabBookmark.Tab ?? (ITab)Activator.CreateInstance(bookmark.Type!)!;

		if (tab is IReload reloadable)
		{
			reloadable.Reload();
		}

		TabInstance tabInstance = tab.Create();
		tabInstance.Project = project.Open(linkedBookmarkCopy);
		tabInstance.iTab = iTab;
		tabInstance.IsRoot = true;
		tabInstance.SelectBookmark(bookmark.TabBookmark);
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
