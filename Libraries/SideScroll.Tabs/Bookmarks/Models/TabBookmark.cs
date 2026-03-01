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

/// <summary>
/// Represents a hierarchical tab bookmark with support for selected rows, custom data, and navigation state
/// </summary>
[PublicData]
public class TabBookmark
{
	/// <summary>
	/// The default name for bookmark data
	/// </summary>
	public const string DefaultDataName = "default";

	/// <summary>
	/// Gets the navigation address for this bookmark
	/// </summary>
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

	/// <summary>
	/// Gets or sets the serialized tab instance (set with [TabRoot] attribute)
	/// </summary>
	public ITab? Tab { get; set; }

	/// <summary>
	/// Gets or sets whether this is a root bookmark (set by [TabRoot] or as the first in a Bookmark)
	/// </summary>
	[PrivateData]
	public bool IsRoot { get; set; }

	/// <summary>
	/// Gets or sets the tab width
	/// </summary>
	public double? Width { get; set; }

	/// <summary>
	/// Gets or sets the list of tab data bookmarks
	/// </summary>
	public List<TabDataBookmark> TabDatas { get; set; } = [];

	/// <summary>
	/// Gets or sets custom bookmark data (object types must be allowed or have [PublicData] attribute)
	/// </summary>
	public Dictionary<string, object?>? BookmarkData { get; set; }

	/// <summary>
	/// Gets or sets the tab model (used for search results, not serialized)
	/// </summary>
	[Unserialized, JsonIgnore]
	public TabModel? TabModel { get; set; }

	//public List<DataRepoItem> DataRepoItems { get; set; } = new();

	/// <summary>
	/// Gets all selected row views from all tab datas
	/// </summary>
	[JsonIgnore]
	public List<SelectedRowView> SelectedRowViews => [.. TabDatas.SelectMany(d => d.SelectedRows)];

	/// <summary>
	/// Gets all selected rows from all tab datas
	/// </summary>
	[JsonIgnore]
	public List<SelectedRow> SelectedRows => [.. SelectedRowViews.Select(s => s.SelectedRow)];

	/// <summary>
	/// Gets all child tab bookmarks from selected row views
	/// </summary>
	[JsonIgnore]
	public List<TabBookmark> SelectedTabViews => [.. SelectedRowViews.Select(s => s.TabBookmark)];

	/// <summary>
	/// Gets the selection type from the first non-None tab data
	/// </summary>
	public SelectionType SelectionType => TabDatas
				.FirstOrDefault(tabDataBookmark => tabDataBookmark.SelectionType != SelectionType.None)
				?.SelectionType ?? SelectionType.None;

	public override string? ToString() => Address;

	/// <summary>
	/// Gets or creates tab data at the specified index
	/// </summary>
	public TabDataBookmark GetData(int index) // Change to string id?
	{
		// Creates new Settings if necessary
		while (TabDatas.Count <= index)
		{
			TabDatas.Add(new TabDataBookmark());
		}
		return TabDatas[index];
	}

	/// <summary>
	/// Sets custom data using the default name
	/// </summary>
	public void SetData(object obj)
	{
		SetData(DefaultDataName, obj);
	}

	/// <summary>
	/// Sets custom bookmark data by name
	/// </summary>
	public void SetData(string name, object? obj)
	{
		BookmarkData ??= [];
		BookmarkData[name] = obj;
	}

	/// <summary>
	/// Gets custom bookmark data by name and type
	/// </summary>
	public T? GetData<T>(string name = DefaultDataName)
	{
		if (BookmarkData != null && BookmarkData.TryGetValue(name, out object? obj) && obj is T t)
			return t;

		return default;
	}

	/// <summary>
	/// Gets the hierarchical navigation address with cycle detection
	/// </summary>
	/// <param name="maxDepth">Maximum recursion depth</param>
	/// <param name="visited">Set of visited bookmarks to prevent cycles</param>
	public string GetAddress(int maxDepth = 100, HashSet<TabBookmark>? visited = null)
	{
		visited ??= [];
		if (maxDepth <= 0 || !visited.Add(this)) return "";

		return string.Join(',', TabDatas.Select(d => d.GetAddress(maxDepth, visited)));
	}

	/// <summary>
	/// Gets the deepest rootable tab bookmark in the hierarchy
	/// </summary>
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

	/// <summary>
	/// Imports all tab data into the specified project
	/// </summary>
	public void Import(Project project)
	{
		foreach (TabDataBookmark tabDataBookmark in TabDatas)
		{
			tabDataBookmark.Import(project);
		}
	}

	/// <summary>
	/// Creates a hierarchical bookmark from a sequence of objects
	/// </summary>
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

	/// <summary>
	/// Creates a single level bookmark with multiple selected items from a list
	/// </summary>
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

	/// <summary>
	/// Creates a hierarchical navigation path from label strings
	/// </summary>
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

	/// <summary>
	/// Selects multiple rows by their labels
	/// </summary>
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

	/// <summary>
	/// Adds a selected row with the specified label and returns its view
	/// </summary>
	public SelectedRowView AddSelected(string label)
	{
		SelectedRowView selectedRowView = new(label);
		AddSelected(selectedRowView);
		return selectedRowView;
	}

	/// <summary>
	/// Adds a selected row view to the first tab data
	/// </summary>
	public void AddSelected(SelectedRowView selectedRowView)
	{
		if (TabDatas.Count == 0)
		{
			TabDatas.Add(new());
		}
		TabDatas.First().SelectedRows.Add(selectedRowView);
	}

	/// <summary>
	/// Sets the selection type for all tab datas
	/// </summary>
	public void SetSelectionType(SelectionType selectionType)
	{
		foreach (TabDataBookmark tabDataBookmark in TabDatas)
		{
			tabDataBookmark.SetSelectionType(selectionType);
		}
	}

	/// <summary>
	/// Tries to get a child bookmark by label
	/// </summary>
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

	/// <summary>
	/// Converts this tab bookmark to tab view settings
	/// </summary>
	public TabViewSettings ToViewSettings()
	{
		return new TabViewSettings
		{
			Width = Width,
			TabDataSettings = [.. TabDatas.Select(t => t.ToDataSettings())],
		};
	}
}
