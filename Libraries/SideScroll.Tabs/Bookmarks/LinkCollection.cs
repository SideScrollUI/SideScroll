using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Bookmarks.Tabs;
using SideScroll.Utilities;

namespace SideScroll.Tabs.Bookmarks;

/// <summary>
/// Represents a linked bookmark with its URI and bookmark data
/// </summary>
[PublicData]
public class LinkedBookmark(LinkUri? linkUri, Bookmark bookmark)
{
	/// <summary>
	/// Gets the link URI for this bookmark
	/// </summary>
	[MaxHeight(150)]
	public LinkUri? LinkUri => linkUri;

	/// <summary>
	/// Gets the bookmark data
	/// </summary>
	public Bookmark Bookmark => bookmark;

	/// <summary>
	/// Gets the creation time of the bookmark
	/// </summary>
	public DateTime? CreatedTime => Bookmark.CreatedTime;

	/// <summary>
	/// Gets the unique link identifier
	/// </summary>
	public string LinkId => linkUri?.ToString() ?? bookmark.ToString();

	public override string ToString() => LinkId;
}

/// <summary>
/// Manages a collection of linked bookmarks with data repository persistence
/// </summary>
public class LinkCollection
{
	/// <summary>
	/// Gets the project associated with this link collection
	/// </summary>
	public Project Project { get; }

	/// <summary>
	/// Gets the group identifier for the data repository
	/// </summary>
	public string GroupId { get; }

	/// <summary>
	/// Gets or sets the UI collection of linked bookmark tabs
	/// </summary>
	public ItemCollectionUI<TabLinkedBookmark> Items { get; set; } = new()
	{
		PostOnly = true,
	};

	/// <summary>
	/// Gets or sets the most recently added bookmark
	/// </summary>
	public TabLinkedBookmark? NewBookmark { get; set; }

	/// <summary>
	/// Gets or sets whether to show the link info tab when opening links
	/// </summary>
	public bool ShowLinkInfoTab { get; set; }

	private readonly DataRepoView<LinkedBookmark> _dataRepoView;

	private readonly object _lock = new();

	/// <summary>
	/// Initializes a new link collection for the specified project and group
	/// </summary>
	/// <param name="project">The project to associate with</param>
	/// <param name="groupId">The data repository group ID (default: "Links")</param>
	public LinkCollection(Project project, string groupId = "Links")
	{
		Project = project;
		GroupId = groupId;

		_dataRepoView = Project.Data.App.OpenView<LinkedBookmark>(GroupId);
	}

	/// <summary>
	/// Loads all linked bookmarks from the data repository
	/// </summary>
	/// <param name="call">The call context for logging</param>
	/// <param name="reload">Whether to force reload even if items already exist</param>
	public void Load(Call call, bool reload)
	{
		lock (_lock)
		{
			if (!reload && Items.Count > 0)
				return;

			Items.Clear();

			_dataRepoView.LoadAllOrderBy(call, nameof(LinkedBookmark.CreatedTime));

			foreach (LinkedBookmark linkedBookmark in _dataRepoView.Values)
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

	/// <summary>
	/// Adds a new linked bookmark to the collection and saves it to the data repository
	/// </summary>
	/// <param name="call">The call context for logging</param>
	/// <param name="linkUri">The link URI</param>
	/// <param name="bookmark">The bookmark data</param>
	public void AddNew(Call call, LinkUri? linkUri, Bookmark bookmark)
	{
		bookmark.TabBookmark.SetSelectionType(Settings.SelectionType.Link);
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

			Call call = new();
			if (!linkProject.DataSettings.LinkId.IsNullOrEmpty())
			{
				FileUtils.DeleteDirectory(call, linkProject.Data.AppPath);
				FileUtils.DeleteDirectory(call, linkProject.Data.CachePath);
			}

			_dataRepoView.Delete(call, tabLink.LinkedBookmark.LinkId);
			Items.Remove(tabLink);
		}
	}

	/// <summary>
	/// Removes a linked bookmark from the collection by its link ID
	/// </summary>
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

	/// <summary>
	/// Deletes all linked bookmarks from the collection
	/// </summary>
	public void DeleteAll(Call? call)
	{
		_dataRepoView.DeleteAll(call);
	}
}
