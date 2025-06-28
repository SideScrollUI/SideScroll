using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Tabs.Bookmarks;

[PublicData]
public class LinkedBookmark(LinkUri linkUri, Bookmark bookmark)
{
	[MaxHeight(150)]
	public LinkUri LinkUri => linkUri;
	public Bookmark Bookmark => bookmark;
	public DateTime TimeStamp => Bookmark.TimeStamp;

	public string LinkId => linkUri.ToString();

	public override string ToString() => LinkId;
}

public class LinkCollection
{
	public Project Project { get; init; }

	public string GroupId { get; init; }

	public ItemCollectionUI<TabLinkedBookmark> Items { get; set; } = new()
	{
		PostOnly = true,
	};

	public TabLinkedBookmark? NewBookmark { get; set; }

	public bool ShowLinkInfoTab { get; set; }

	private readonly DataRepoView<LinkedBookmark> _dataRepoView;

	private readonly object _lock = new();

	public LinkCollection(Project project, string groupId = "Links")
	{
		Project = project;
		GroupId = groupId;

		_dataRepoView = Project.Data.App.OpenView<LinkedBookmark>(GroupId);
	}

	public void Load(Call call, bool reload)
	{
		lock (_lock)
		{
			if (!reload && Items.Count > 0)
				return;

			Items.Clear();

			_dataRepoView.LoadAllOrderBy(call, nameof(LinkedBookmark.TimeStamp));

			foreach (LinkedBookmark linkedBookmark in _dataRepoView.Items.Values)
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

			string linkId = linkedBookmark.LinkId;
			Remove(call, linkId); // Remove previous bookmark
			_dataRepoView.Save(call, linkId, linkedBookmark);
			NewBookmark = Add(linkedBookmark);
		}
	}

	private void Item_OnDelete(object? sender, EventArgs e)
	{
		var tabLink = (TabLinkedBookmark)sender!;
		lock (_lock)
		{
			var linkProject = Project.Open(tabLink.LinkedBookmark);

			if (!linkProject.UserSettings.LinkId.IsNullOrEmpty())
			{
				Directory.Delete(linkProject.Data.AppPath, true);
				Directory.Delete(linkProject.Data.CachePath, true);
			}

			_dataRepoView.Delete(null, tabLink.LinkedBookmark.LinkId);
			Items.Remove(tabLink);
		}
	}

	public void Remove(Call call, string linkId)
	{
		lock (_lock)
		{
			_dataRepoView.Delete(call, linkId);

			TabLinkedBookmark? existing = Items.SingleOrDefault(i => i.LinkedBookmark.LinkId == linkId);
			if (existing != null)
			{
				Items.Remove(existing);
			}
		}
	}

	public void DeleteAll(Call? call)
	{
		_dataRepoView.DeleteAll(call);
	}
}
