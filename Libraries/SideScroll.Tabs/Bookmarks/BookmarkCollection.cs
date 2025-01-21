using SideScroll.Collections;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Tabs.Bookmarks;

public class BookmarkCollection
{
	public Project Project { get; init; }

	public string GroupId { get; init; }

	public ItemCollectionUI<TabBookmarkItem> Items { get; set; } = new()
	{
		PostOnly = true,
	};

	public TabBookmarkItem? NewBookmark { get; set; }

	private readonly DataRepoView<Bookmark> _dataRepoBookmarks;

	private readonly object _lock = new();

	public BookmarkCollection(Project project, string groupId = "Bookmarks")
	{
		Project = project;
		GroupId = groupId;

		_dataRepoBookmarks = Project.DataApp.OpenView<Bookmark>(GroupId);
	}

	public void Load(Call call, bool reload)
	{
		lock (_lock)
		{
			if (!reload && Items.Count > 0)
				return;

			Items.Clear();

			_dataRepoBookmarks.LoadAllOrderBy(call, nameof(Bookmark.TimeStamp));

			foreach (Bookmark bookmark in _dataRepoBookmarks.Items.Values)
			{
				if (bookmark.Name == TabInstance.CurrentBookmarkName)
					continue;

				Add(bookmark);
			}
		}
	}

	private TabBookmarkItem Add(Bookmark bookmark)
	{
		var tabItem = new TabBookmarkItem(bookmark, Project);
		tabItem.OnDelete += Item_OnDelete;

		lock (_lock)
		{
			Items.Add(tabItem);
		}
		return tabItem;
	}

	public void AddNew(Call call, Bookmark bookmark)
	{
		lock (_lock)
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
		lock (_lock)
		{
			_dataRepoBookmarks.Delete(null, bookmark.Bookmark.Path);
			Items.Remove(bookmark);
		}
	}

	public void Remove(Call call, string key)
	{
		lock (_lock)
		{
			_dataRepoBookmarks.Delete(call, key);

			TabBookmarkItem? existing = Items.SingleOrDefault(i => i.Bookmark.Path == key);
			if (existing != null)
			{
				Items.Remove(existing);
			}
		}
	}

	public void DeleteAll(Call? call)
	{
		_dataRepoBookmarks.DeleteAll(call);
	}
}
