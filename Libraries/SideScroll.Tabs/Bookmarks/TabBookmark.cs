using SideScroll.Attributes;
using SideScroll.Tabs.Settings;
using System.Collections;
using System.Data;

namespace SideScroll.Tabs.Bookmarks;

/*public class DataRepoItem
{
	public string GroupId { get; set; }
	public string Key { get; set; }
	public object Value { get; set; }
}*/

// rename to TabInstanceSettings?
[PublicData]
public class TabBookmark
{
	public const string DefaultDataName = "default";

	[PrivateData]
	public Bookmark? Bookmark { get; set; }
	public string? Name { get; set; }
	public bool IsRoot { get; set; } // [TabRoot] set or first in a Bookmark
	public ITab? Tab { get; set; } // [TabRoot] will set this to use the serialized tab as the root tab

	public SelectedRow? SelectedRow { get; set; } // The parent selection that created this bookmark

	public TabViewSettings ViewSettings { get; set; } = new(); // list selections, doesn't know about children
	public Dictionary<string, TabBookmark> ChildBookmarks { get; set; } = []; // doesn't know which tabData to use, maps id to child info
	public Dictionary<string, object?>? BookmarkData { get; set; }

	//public List<DataRepoItem> DataRepoItems { get; set; } = new();
	public string? DataRepoGroupId { get; set; }
	public Type? DataRepoType { get; set; } // Interfaces need to specify this

	// Temporary, Only FindMatches() uses, refactor these out?
	[Unserialized]
	public HashSet<object> SelectedObjects { get; set; } = []; // does this work with multiple TabDatas?

	[Unserialized]
	public TabModel? TabModel { get; set; }

	public override string? ToString() => Name;

	public static TabBookmark Create(params object[] objs)
	{
		// get TabBookmark.SelectedObjects working again and replace?

		string? prevKey = null;
		TabBookmark? rootBookmark = null;
		TabBookmark? tabBookmark = null;
		foreach (object obj in objs)
		{
			string? dataKey = new SelectedRow(obj).ToString();
			if (dataKey == null) throw new Exception("SelectedRow DataKey is null");

			var newBookmark = new TabBookmark();
			newBookmark.SelectRows(dataKey);
			tabBookmark?.ChildBookmarks.Add(prevKey!, newBookmark);
			tabBookmark = newBookmark;
			rootBookmark ??= tabBookmark;
			prevKey = dataKey;
		}
		return rootBookmark!;
	}

	// Single level multi-select
	public static TabBookmark CreateList(IList list)
	{
		var selectedRows = list
			.Cast<object>()
			.Select(obj => new SelectedRow(obj))
			.ToHashSet();

		var tabBookmark = new TabBookmark();
		tabBookmark.SelectRows(selectedRows);
		return tabBookmark;
	}

	// Shallow Clone
	public TabBookmark Clone()
	{
		return new TabBookmark
		{
			Bookmark = Bookmark,
			Name = Name,
			SelectedRow = SelectedRow,
			DataRepoGroupId = DataRepoGroupId,
			BookmarkData = BookmarkData,
		};
	}

	public void Add(TabBookmark tabBookmark)
	{
		ChildBookmarks.Add(tabBookmark.Name!, tabBookmark);

		var selectedRow = tabBookmark.SelectedRow ?? new SelectedRow
		{
			Label = tabBookmark.Name,
		};
		ViewSettings ??= new TabViewSettings();
		ViewSettings.TabDataSettings ??= [];
		if (ViewSettings.TabDataSettings.Count == 0)
		{
			ViewSettings.TabDataSettings.Add(new TabDataSettings());
		}
		ViewSettings.TabDataSettings[0].SelectionType = SelectionType.User;
		ViewSettings.TabDataSettings[0].SelectedRows ??= [];
		ViewSettings.TabDataSettings[0].SelectedRows.Add(selectedRow);
		//Select(ChildBookmarks.Keys);
	}

	public SortedDictionary<string, T> GetSelectedData<T>()
	{
		var items = new SortedDictionary<string, T>();
		if (ViewSettings != null)
		{
			foreach (SelectedRow row in ViewSettings.SelectedRows)
			{
				string? dataKey = row.DataKey ?? row.Label;
				if (dataKey != null && row.DataValue != null && row.DataValue.GetType() == typeof(T))
				{
					items[dataKey] = (T)row.DataValue;
				}
			}
		}
		return items;
	}

	public void SetData(object obj)
	{
		SetData(DefaultDataName, obj);
	}

	public void SetData(string name, object? obj)
	{
		ViewSettings ??= new TabViewSettings();
		BookmarkData ??= new Dictionary<string, object?>();
		BookmarkData[name] = obj;
	}

	public T? GetData<T>(string name = DefaultDataName)
	{
		if (BookmarkData != null && BookmarkData.TryGetValue(name, out object? obj) && obj is T t)
			return t;

		return default;
	}

	public string GetAddress(int maxDepth = 100, HashSet<TabBookmark>? visited = null)
	{
		visited ??= [];
		if (maxDepth <= 0 || !visited.Add(this)) return "";

		if (ChildBookmarks?.Count > 0)
		{
			string comma = "";
			string address = "";
			if (ChildBookmarks.Count > 1)
			{
				address += "[";
			}
			//address += Name + "::";
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
		else
		{
			return ViewSettings?.Address ?? "";
		}
	}

	public void SelectPath(params string[] labels)
	{
		TabBookmark tabBookmark = this;
		foreach (string label in labels)
		{
			tabBookmark.SelectRows(label);
			tabBookmark = AddChild(label);
		}
	}

	public void SelectRows(params string[] labels)
	{
		var selectedRows = labels.Select(label =>
			new SelectedRow
			{
				Label = label,
			}
		).ToHashSet();

		SelectRows(selectedRows);
	}

	private void SelectRows(HashSet<SelectedRow> selectedRows)
	{
		ViewSettings = new TabViewSettings
		{
			TabDataSettings = new List<TabDataSettings>
			{
				new()
				{
					SelectionType = SelectionType.User,
					SelectedRows = selectedRows,
				},
			},
		};
	}

	public TabBookmark AddChild(string dataKey)
	{
		var childBookmark = new TabBookmark
		{
			Bookmark = Bookmark,
		};
		ChildBookmarks.Add(dataKey, childBookmark);
		return childBookmark;
	}

	public TabBookmark? GetChild(string dataKey)
	{
		if (ChildBookmarks == null)
			return null;

		return ChildBookmarks.GetValueOrDefault(dataKey);
	}

	public void Import(Project project)
	{
		if (ViewSettings == null)
			return;

		foreach (SelectedRow row in ViewSettings.SelectedRows)
		{
			string? dataKey = row.DataKey ?? row.Label;
			if (dataKey == null || row.DataValue == null)
				continue;

			if (DataRepoType is Type type)
			{
				project.Data.App.Save(type, DataRepoGroupId, dataKey, row.DataValue);
			}
			else
			{
				project.Data.App.Save(DataRepoGroupId, dataKey, row.DataValue);
			}
		}

		foreach (TabBookmark tabBookmark in ChildBookmarks.Values)
		{
			tabBookmark.Import(project);
		}
	}

	// Returns the deepest TabBookmark that is rootable
	public TabBookmark? GetLeaf()
	{
		if (ChildBookmarks.Count == 1)
		{
			var leaf = ChildBookmarks.First().Value.GetLeaf();
			if (leaf != null)
				return leaf;
		}

		if (IsRoot)
			return this;

		return null;
	}

	public void MergeNode(TabBookmark node)
	{
		foreach (var nodeEntry in node.ChildBookmarks)
		{
			if (ChildBookmarks.TryGetValue(nodeEntry.Key, out TabBookmark? existingNode))
			{
				existingNode.MergeNode(nodeEntry.Value);
			}
			else
			{
				ChildBookmarks.Add(nodeEntry.Key, nodeEntry.Value);
			}
		}

		if (ViewSettings == null)
		{
			ViewSettings = node.ViewSettings;
			return;
		}

		Name = " + " + node.Name;

		for (int i = 0; i < ViewSettings.TabDataSettings.Count; i++)
		{
			var currentSelection = ViewSettings.TabDataSettings[i].SelectedRows;
			var otherSelection = node.ViewSettings.TabDataSettings[i].SelectedRows;

			var labelsUsed = new HashSet<string>();
			var indicesUsed = new HashSet<int>();
			foreach (SelectedRow row in currentSelection)
			{
				if (row.Label != null)
				{
					labelsUsed.Add(row.Label);
				}
				else if (row.RowIndex is int rowIndex && rowIndex >= 0)
				{
					indicesUsed.Add(rowIndex);
				}
			}

			foreach (SelectedRow row in otherSelection)
			{
				if (row.Label != null)
				{
					if (!labelsUsed.Contains(row.Label))
					{
						currentSelection.Add(row);
					}
				}
				else if (row.RowIndex is int rowIndex && rowIndex >= 0)
				{
					if (!indicesUsed.Contains(rowIndex))
					{
						currentSelection.Add(row);
					}
				}
			}
		}
	}

	public void Reinitialize(Bookmark bookmark)
	{
		Bookmark = bookmark;

		foreach (var tabBookmark in ChildBookmarks.Values)
		{
			tabBookmark.Reinitialize(bookmark);
		}
	}
}
