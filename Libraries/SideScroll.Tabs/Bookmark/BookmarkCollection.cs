using SideScroll;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Tabs;

public class BookmarkCollection
{
	public const string DataKey = "Bookmarks";

	public readonly Project Project;

	public ItemCollectionUI<TabBookmarkItem> Items { get; set; } = new()
	{
		PostOnly = true,
	};

	public TabBookmarkItem? NewBookmark { get; set; }

	private readonly DataRepoView<Bookmark> _dataRepoBookmarks;

	public BookmarkCollection(Project project)
	{
		Project = project;

		_dataRepoBookmarks = Project.DataApp.OpenView<Bookmark>(DataKey);
	}

	public void Load(Call call, bool reload)
	{
		lock (DataKey)
		{
			if (!reload && Items.Count > 0)
				return;

			Items.Clear();

			_dataRepoBookmarks.LoadAllOrderBy(call, nameof(Bookmark.TimeStamp));

			foreach (Bookmark? bookmark in _dataRepoBookmarks.Items.Values)
			{
				if (bookmark!.Name == TabInstance.CurrentBookmarkName)
					continue;

				Add(bookmark);
			}
		}
	}

	private TabBookmarkItem Add(Bookmark bookmark)
	{
		var tabItem = new TabBookmarkItem(bookmark, Project);
		tabItem.OnDelete += Item_OnDelete;

		lock (DataKey)
		{
			Items.Add(tabItem);
		}
		return tabItem;
	}

	public void AddNew(Call call, Bookmark bookmark)
	{
		lock (DataKey)
		{
			Load(call, false);

			Remove(call, bookmark.Path); // Remove previous bookmark
			_dataRepoBookmarks.Save(call, bookmark.Path, bookmark);
			NewBookmark = Add(bookmark);
		}
	}

	private void Item_OnDelete(object? sender, EventArgs e)
	{
		TabBookmarkItem bookmark = (TabBookmarkItem)sender!;
		lock (DataKey)
		{
			_dataRepoBookmarks.Delete(null, bookmark.Bookmark.Path);
			Items.Remove(bookmark);
		}
	}

	public void Remove(Call call, string key)
	{
		lock (DataKey)
		{
			_dataRepoBookmarks.Delete(call, key);

			TabBookmarkItem? existing = Items.SingleOrDefault(i => i.Bookmark.Path == key);
			if (existing != null)
				Items.Remove(existing);
		}
	}

	public void DeleteAll(Call? call)
	{
		_dataRepoBookmarks.DeleteAll(call);
	}
}
