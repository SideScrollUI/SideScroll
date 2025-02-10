using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Tabs.Bookmarks;

public class LinkedBookmark(LinkUri linkUri, Bookmark bookmark)
{
	[MaxHeight(150)]
	public LinkUri LinkUri => linkUri;
	public Bookmark Bookmark => bookmark;
	public DateTime TimeStamp => Bookmark.TimeStamp;

	public override string ToString() => Bookmark.ToString();
}

public class BookmarkCollection
{
	public Project Project { get; init; }

	public string GroupId { get; init; }

	public ItemCollectionUI<TabLinkedBookmark> Items { get; set; } = new()
	{
		PostOnly = true,
	};

	public TabLinkedBookmark? NewBookmark { get; set; }

	public bool ShowLinkInfoTab { get; set; }

	private readonly DataRepoView<LinkedBookmark> _dataRepoBookmarks;

	private readonly object _lock = new();

	public BookmarkCollection(Project project, string groupId = "Bookmarks")
	{
		Project = project;
		GroupId = groupId;

		_dataRepoBookmarks = Project.DataApp.OpenView<LinkedBookmark>(GroupId);
	}

	public void Load(Call call, bool reload)
	{
		lock (_lock)
		{
			if (!reload && Items.Count > 0)
				return;

			Items.Clear();

			_dataRepoBookmarks.LoadAllOrderBy(call, nameof(LinkedBookmark.TimeStamp));

			foreach (LinkedBookmark linkedBookmark in _dataRepoBookmarks.Items.Values)
			{
				if (linkedBookmark.Bookmark.Name == TabInstance.CurrentBookmarkName)
					continue;

				Add(linkedBookmark);
			}
		}
	}

	private TabLinkedBookmark Add(LinkedBookmark linkedBookmark)
	{
		var tabItem = new TabLinkedBookmark(linkedBookmark, this);
		tabItem.OnDelete += Item_OnDelete;

		lock (_lock)
		{
			Items.Add(tabItem);
		}
		return tabItem;
	}

	public void AddNew(Call call, LinkUri linkUri, Bookmark bookmark)
	{
		LinkedBookmark linkedBookmark = new(linkUri, bookmark);
		lock (_lock)
		{
			Load(call, false);

			Remove(call, linkedBookmark.Bookmark.Path); // Remove previous bookmark
			_dataRepoBookmarks.Save(call, linkedBookmark.Bookmark.Path, linkedBookmark);
			NewBookmark = Add(linkedBookmark);
		}
	}

	private void Item_OnDelete(object? sender, EventArgs e)
	{
		var tabLink = (TabLinkedBookmark)sender!;
		lock (_lock)
		{
			_dataRepoBookmarks.Delete(null, tabLink.LinkedBookmark.Bookmark.Path);
			Items.Remove(tabLink);
		}
	}

	public void Remove(Call call, string key)
	{
		lock (_lock)
		{
			_dataRepoBookmarks.Delete(call, key);

			TabLinkedBookmark? existing = Items.SingleOrDefault(i => i.LinkedBookmark.Bookmark.Path == key);
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
