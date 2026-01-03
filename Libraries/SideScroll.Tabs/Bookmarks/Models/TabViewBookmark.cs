using SideScroll.Attributes;
using SideScroll.Tabs.Settings;
using System.Collections;

namespace SideScroll.Tabs.Bookmarks.Models;

[PublicData]
public class TabViewBookmark
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

			return "[" + string.Join(", ", addresses) + "] ";
		}
	}

	public double? Width { get; set; }

	public List<TabDataBookmark> TabDatas { get; set; } = [];

	public ITab? Tab { get; set; } // [TabRoot] will set this to use the serialized tab as the root tab

	[PrivateData]
	public bool IsRoot { get; set; } // [TabRoot] set or first in a Bookmark

	public Dictionary<string, object?>? BookmarkData { get; set; }

	// Temporary, Only FindMatches() uses, refactor these out?
	[Unserialized]
	public TabModel? TabModel { get; set; } // Used for search results

	//public List<DataRepoItem> DataRepoItems { get; set; } = new();

	//[JsonIgnore]
	//public List<SelectedRow> SelectedRows => TabDatas?.SelectMany(d => d.SelectedRows.Select(s => s.SelectedRow!)).ToList() ?? [];

	// Store Skipped bool instead?
	public SelectionType SelectionType => TabDatas?
				.FirstOrDefault(dataSettings => dataSettings.SelectionType != SelectionType.None)
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

	public void Import(Project project)
	{
		foreach (var row in TabDatas)
		{
			row.Import(project);
		}
	}

	public string GetAddress(int maxDepth = 100, HashSet<TabViewBookmark>? visited = null)
	{
		visited ??= [];
		if (maxDepth <= 0 || !visited.Add(this)) return "";

		return string.Join(',', TabDatas.Select(d => d.GetAddress(maxDepth, visited)));
	}

	// Returns the deepest TabBookmark that is rootable
	public TabViewBookmark? GetLeaf()
	{
		if (TabDatas.Count == 1)
		{
			var leaf = TabDatas
				.First()
				.SelectedRows.Select(s => s.TabViewBookmark.GetLeaf())
				.FirstOrDefault();
			if (leaf != null)
				return leaf;
		}

		if (IsRoot)
			return this;

		return null;
	}

	public static TabViewBookmark Create(params object[] objs)
	{
		TabViewBookmark rootBookmark = new();
		TabViewBookmark tabBookmark = rootBookmark;
		foreach (object obj in objs)
		{
			string? label = new SelectedRow(obj).ToString();
			if (label == null) throw new Exception("SelectedRow Label is null");

			TabViewBookmark newBookmark = new();
			tabBookmark.SelectRow(new(label, newBookmark));
			tabBookmark = newBookmark;
		}
		return rootBookmark;
	}

	// Single level multi-select
	public static TabViewBookmark CreateList(IList list)
	{
		var selectedRows = list
			.Cast<object>()
			.Select(obj => new SelectedRowView(new SelectedRow(obj)))
			.ToList();

		return new TabViewBookmark
		{
			TabDatas =
			[
				new()
				{
					SelectedRows = selectedRows,
				}
			],
		};
	}

	public void SelectPath(params string[] labels)
	{
		TabViewBookmark tabBookmark = this;
		foreach (string label in labels)
		{
			var view = new SelectedRowView(label);
			tabBookmark.SelectRows([view]);
			tabBookmark = view.TabViewBookmark;
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
		).ToHashSet();

		SelectRows(selectedRows);
	}

	private void SelectRows(ICollection<SelectedRowView> selectedRows)
	{
		TabDatas =
		[
			new TabDataBookmark
			{
				SelectionType = SelectionType.User,
				SelectedRows = selectedRows.ToList(),
			}
		];
	}

	public SelectedRowView AddChild(string label)
	{
		SelectedRowView childBookmark = new(label);
		if (TabDatas.Count == 0)
		{
			TabDatas.Add(new());
		}
		TabDatas.First().SelectedRows.Add(childBookmark);
		return childBookmark;
	}

	public TabViewSettings ToViewSettings()
	{
		TabViewSettings tabViewSettings = new()
		{
			Width = Width,
			TabDataSettings = TabDatas.Select(t => t.ToDataSettings()).ToList(),
		};
		return tabViewSettings;
	}

	public bool TryGetValue(string label, out TabViewBookmark? childBookmarkNode)
	{
		if (TabDatas.SelectMany(t => t.SelectedRows).FirstOrDefault(t => t.SelectedRow?.ToString() == label) is SelectedRowView selectedRowView)
		{
			childBookmarkNode = selectedRowView.TabViewBookmark;
			return true;
		}
		childBookmarkNode = null;
		return false;
	}
}
