using SideScroll.Attributes;
using SideScroll.Tabs.Settings;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SideScroll.Tabs.Bookmarks.Models;

/*public class DataRepoItem
{
	public string GroupId { get; set; }
	public string Key { get; set; }
	public object Value { get; set; }
}*/

[PublicData]
public class TabBookmark
{
	public const string DefaultDataName = "default";

	public string? Address
	{
		get
		{
			var addresses = TabDatas
				.Select(d => d.Address)
				.ToList();

			if (addresses.Count <= 1)
				return addresses.FirstOrDefault();

			return '[' + string.Join(", ", addresses) + ']';
		}
	}

	public ITab? Tab { get; set; } // [TabRoot] will set this to use the serialized tab as the root tab

	[PrivateData]
	public bool IsRoot { get; set; } // [TabRoot] set or first in a Bookmark

	public double? Width { get; set; }

	public List<TabDataBookmark> TabDatas { get; set; } = [];

	// Custom Tab Data, object Types must be allowed or have [PublicData] set
	public Dictionary<string, object?>? BookmarkData { get; set; }

	// Temporary, Only FindMatches() uses, refactor these out?
	[Unserialized, JsonIgnore]
	public TabModel? TabModel { get; set; } // Used for search results

	//public List<DataRepoItem> DataRepoItems { get; set; } = new();

	[JsonIgnore]
	public List<SelectedRowView> SelectedRowViews => [.. TabDatas.SelectMany(d => d.SelectedRows)];

	[JsonIgnore]
	public List<SelectedRow> SelectedRows => [.. SelectedRowViews.Select(s => s.SelectedRow)];

	[JsonIgnore]
	public List<TabBookmark> SelectedTabViews => [.. SelectedRowViews.Select(s => s.TabBookmark)];

	// Store Skipped bool instead?
	public SelectionType SelectionType => TabDatas
				.FirstOrDefault(tabDataBookmark => tabDataBookmark.SelectionType != SelectionType.None)
				?.SelectionType ?? SelectionType.None;

	public override string? ToString() => Address;

	// Change to string id?
	public TabDataBookmark GetData(int index)
	{
		// Creates new Settings if necessary
		while (TabDatas.Count <= index)
		{
			TabDatas.Add(new TabDataBookmark());
		}
		return TabDatas[index];
	}

	public void SetData(object obj)
	{
		SetData(DefaultDataName, obj);
	}

	public void SetData(string name, object? obj)
	{
		BookmarkData ??= [];
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

		return string.Join(',', TabDatas.Select(d => d.GetAddress(maxDepth, visited)));
	}

	// Returns the deepest TabBookmark that is rootable
	public TabBookmark? GetLeaf()
	{
		if (SelectedRowViews.Count == 1)
		{
			var leaf = SelectedRowViews.Select(s => s.TabBookmark.GetLeaf())
				.FirstOrDefault();
			if (leaf != null)
				return leaf;
		}

		if (IsRoot)
			return this;

		return null;
	}

	public void Import(Project project)
	{
		foreach (TabDataBookmark tabDataBookmark in TabDatas)
		{
			tabDataBookmark.Import(project);
		}
	}

	public static TabBookmark Create(params object[] objs)
	{
		TabBookmark rootBookmark = new();
		TabBookmark tabBookmark = rootBookmark;
		foreach (object obj in objs)
		{
			string label = new SelectedRow(obj).ToString() ?? throw new Exception("SelectedRow Label is null");
			TabBookmark newBookmark = new();
			tabBookmark.SelectRow(new(label, newBookmark));
			tabBookmark = newBookmark;
		}
		return rootBookmark;
	}

	// Single level multi-select
	public static TabBookmark CreateList(IList list)
	{
		var selectedRows = list
			.Cast<object>()
			.Select(obj => new SelectedRowView(obj))
			.ToList();

		return new TabBookmark
		{
			TabDatas =
			[
				new()
				{
					SelectedRows = selectedRows,
					SelectionType = SelectionType.User,
				}
			],
		};
	}

	public void SelectPath(params string[] labels)
	{
		TabBookmark tabBookmark = this;
		foreach (string label in labels)
		{
			var view = new SelectedRowView(label);
			tabBookmark.SelectRows([view]);
			tabBookmark = view.TabBookmark;
		}
	}

	private void SelectRow(SelectedRowView selectedRow)
	{
		TabDatas =
		[
			new TabDataBookmark
			{
				SelectionType = SelectionType.User,
				SelectedRows = [selectedRow],
			}
		];
	}

	public void SelectRows(params string[] labels)
	{
		var selectedRows = labels.Select(label =>
			new SelectedRowView(label)
		).ToList();

		SelectRows(selectedRows);
	}

	private void SelectRows(ICollection<SelectedRowView> selectedRows)
	{
		TabDatas =
		[
			new TabDataBookmark
			{
				SelectionType = SelectionType.User,
				SelectedRows = [.. selectedRows],
			}
		];
	}

	public SelectedRowView AddSelected(string label)
	{
		SelectedRowView selectedRowView = new(label);
		AddSelected(selectedRowView);
		return selectedRowView;
	}

	public void AddSelected(SelectedRowView selectedRowView)
	{
		if (TabDatas.Count == 0)
		{
			TabDatas.Add(new());
		}
		TabDatas.First().SelectedRows.Add(selectedRowView);
	}

	public void SetSelectionType(SelectionType selectionType)
	{
		foreach (TabDataBookmark tabDataBookmark in TabDatas)
		{
			tabDataBookmark.SetSelectionType(selectionType);
		}
	}

	public bool TryGetValue(string label, [NotNullWhen(true)] out TabBookmark? childBookmarkNode)
	{
		if (SelectedRowViews.FirstOrDefault(t => t.SelectedRow.ToString() == label) is SelectedRowView selectedRowView)
		{
			childBookmarkNode = selectedRowView.TabBookmark;
			return true;
		}
		childBookmarkNode = null;
		return false;
	}

	public TabViewSettings ToViewSettings()
	{
		return new TabViewSettings
		{
			Width = Width,
			TabDataSettings = [.. TabDatas.Select(t => t.ToDataSettings())],
		};
	}
}
