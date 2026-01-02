using SideScroll.Attributes;
using SideScroll.Tabs.Settings;
using System.Collections;

namespace SideScroll.Tabs.Bookmarks.Models;

/*public class DataRepoItem
{
	public string GroupId { get; set; }
	public string Key { get; set; }
	public object Value { get; set; }
}*/

// rename to TabInstanceSettings?
[PublicData, Obsolete("Use TabViewBookmark instead")]
public class TabBookmark
{
	public string? Name { get; set; }
	[PrivateData]
	public bool IsRoot { get; set; } // [TabRoot] set or first in a Bookmark
	public ITab? Tab { get; set; } // [TabRoot] will set this to use the serialized tab as the root tab

	public SelectedRow? SelectedRow { get; set; } // The parent selection that created this bookmark

	public TabViewSettings ViewSettings { get; set; } = new(); // list selections, doesn't know about children
	public Dictionary<string, TabBookmark> ChildBookmarks { get; set; } = []; // doesn't know which tabData to use, maps id to child info
	public Dictionary<string, object?>? BookmarkData { get; set; }

	//public List<DataRepoItem> DataRepoItems { get; set; } = new();
	public string? DataRepoGroupId { get; set; }
	public Type? DataRepoType { get; set; } // Interfaces need to specify this

	public override string? ToString() => Name;

	public string GetAddress(int maxDepth = 100, HashSet<TabBookmark>? visited = null)
	{
		visited ??= [];
		if (maxDepth <= 0 || !visited.Add(this)) return "";

		if (ChildBookmarks.Count == 0)
		{
			return ViewSettings.Address ?? "";
		}
		
		string comma = "";
		string address = "";
		if (ChildBookmarks.Count > 1)
		{
			address += "[";
		}
		foreach (var bookmark in ChildBookmarks)
		{
			address += comma;
			address += bookmark.Key + " / " + bookmark.Value.GetAddress(maxDepth - 1, visited);
			comma = ", ";
		}
		if (ChildBookmarks.Count > 1)
		{
			address += "]";
		}
		return address;
	}
}
