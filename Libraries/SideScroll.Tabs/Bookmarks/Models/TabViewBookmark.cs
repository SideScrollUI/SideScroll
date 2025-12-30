using SideScroll.Attributes;
using SideScroll.Tabs.Settings;
using System.Collections;

namespace SideScroll.Tabs.Bookmarks.Models;

[PublicData]
public class TabViewBookmark
{
	public const string DefaultDataName = "default";

	//public string? Name { get; set; }

	public string? Address
	{
		get
		{
			if (TabDatas == null) return null;

			string address = "";
			string comma = "";
			int count = 0;
			foreach (TabDataBookmark tabDataBookmark in TabDatas)
			{
				foreach (SelectedRowView selectedRowView in tabDataBookmark.Selected)
				{
					address += comma;
					address += selectedRowView.SelectedRow?.Label;
					comma = ", ";
					count++;
				}
			}
			if (count > 1)
			{
				address += "[" + address + "] ";
			}

			return address;
		}
	}

	public double? SplitterDistance { get; set; }

	public List<TabDataBookmark> TabDatas { get; set; } = [];

	public ITab? Tab { get; set; } // [TabRoot] will set this to use the serialized tab as the root tab

	[PrivateData]
	public bool IsRoot { get; set; } // [TabRoot] set or first in a Bookmark

	public Dictionary<string, object?>? BookmarkData { get; set; }

	//public List<DataRepoItem> DataRepoItems { get; set; } = new();
	//public string? DataRepoGroupId { get; set; }
	//public Type? DataRepoType { get; set; } // Interfaces need to specify this

	//[JsonIgnore]
	//public List<SelectedRow> SelectedRows => TabDataSettings?.SelectMany(d => d.Selected.Select(s => s.SelectedRow!)).ToList() ?? [];

	// Store Skipped bool instead?
	public SelectionType SelectionType => TabDatas?
				.FirstOrDefault(dataSettings => dataSettings.SelectionType != SelectionType.None)
				?.SelectionType ?? SelectionType.None;

	public override string? ToString() => Address;

	// change to string id?
	public TabDataBookmark GetData(int index)
	{
		TabDatas ??= [];

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

	public string GetAddress(int maxDepth = 100, HashSet<SelectedRowView>? visited = null)
	{
		visited ??= [];

		return string.Join(',', TabDatas.Select(d => d.GetAddress(maxDepth, visited)));
	}

	// Returns the deepest TabBookmark that is rootable
	public TabViewBookmark? GetLeaf()
	{
		if (TabDatas.Count == 1)
		{
			var leaf = TabDatas.First().Selected.Select(s => s.TabViewBookmark.GetLeaf()).FirstOrDefault();
			if (leaf != null)
				return leaf;
		}

		if (IsRoot)
			return this;

		return null;
	}

	// Single level multi-select
	public static TabViewBookmark CreateList(IList list)
	{
		var selectedRows = list
			.Cast<object>()
			.Select(obj => new SelectedRowView(new(obj)))
			.ToList();

		return new TabViewBookmark
		{
			TabDatas =
			[
				new()
				{
					Selected = selectedRows,
				}
			],
		};
	}

	public void SelectPath(params string[] labels)
	{
		TabViewBookmark tabBookmark = this;
		foreach (string label in labels)
		{
			var view = new SelectedRowView(new(label));
			tabBookmark.SelectRows([view]);
			tabBookmark = view.TabViewBookmark;
		}
	}

	public void SelectRows(params string[] labels)
	{
		var selectedRows = labels.Select(label =>
			new SelectedRowView
			{
				SelectedRow = new()
				{
					Label = label,
				}
			}
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
				Selected = selectedRows.ToList(),
			}
		];
	}

	public SelectedRowView AddChild(string dataKey)
	{
		SelectedRowView childBookmark = new()
		{
			SelectedRow = new(dataKey),
		};
		TabDatas.First().Selected.Add(childBookmark);
		return childBookmark;
	}

	public TabViewSettings ToViewSettings()
	{
		TabViewSettings tabViewSettings = new()
		{
			SplitterDistance = SplitterDistance,
			//SelectedRows = TabDataSettings.SelectMany(t => t.Selected.Select(s => s.SelectedRow!)).ToList(),
			TabDataSettings = TabDatas.Select(t => t.ToDataSettings()).ToList(),
		};
		return tabViewSettings;
	}

	public bool TryGetValue(string label, out TabViewBookmark? childBookmarkNode)
	{
		if (TabDatas.SelectMany(t => t.Selected).FirstOrDefault(t => t.SelectedRow?.ToString() == label) is SelectedRowView selectedRowView)
		{
			childBookmarkNode = selectedRowView.TabViewBookmark;
			return true;
		}
		childBookmarkNode = null;
		return false;
	}
}
