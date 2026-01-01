using SideScroll.Attributes;
using SideScroll.Tabs.Settings;

namespace SideScroll.Tabs.Bookmarks.Models;

[PublicData]
public class TabDataBookmark
{
	[PrivateData]
	public SelectionType SelectionType { get; set; } = SelectionType.None;

	public List<string> ColumnNameOrder { get; set; } = []; // Order to show the columns in, users can drag columns around to reorder these

	// Not currently supported by DataGrid
	// public string? SortColumnName { get; set; } // Currently sorted column
	// public ListSortDirection SortDirection { get; set; }

	public string? DataRepoGroupId { get; set; }
	public Type? DataRepoType { get; set; } // Interfaces need to specify this

	public List<SelectedRowView> SelectedRows { get; set; } = [];

	public string? Filter { get; set; }

	public string? Address
	{
		get
		{
			var labels = SelectedRows
				.Select(s => s.SelectedRow?.Label)
				.ToList();

			if (labels.Count <= 1)
				return labels.FirstOrDefault();

			return "[" + string.Join(", ", labels) + "] ";
		}
	}

	public string GetAddress(int maxDepth = 100, HashSet<TabViewBookmark>? visited = null)
	{
		visited ??= [];

		if (SelectedRows.Count == 0)
		{
			return Address ?? "";
		}

		string comma = "";
		string address = "";
		if (SelectedRows.Count > 1)
		{
			address += '[';
		}
		foreach (var selectedRow in SelectedRows)
		{
			address += comma;
			address += selectedRow.ToString() + " / " + selectedRow.TabViewBookmark.GetAddress(maxDepth - 1, visited);
			comma = ", ";
		}
		if (SelectedRows.Count > 1)
		{
			address += ']';
		}
		return address;
	}

	public override string? ToString() => Address;

	public void Import(Project project)
	{
		foreach (var view in SelectedRows)
		{
			var row = view.SelectedRow;
			string? dataKey = row.DataKey ?? row.Label;
			if (dataKey == null || row.DataValue == null)
				continue;

			// Interfaces or base classes need to specify a type
			if (DataRepoType is Type type)
			{
				project.Data.App.Save(type, DataRepoGroupId, dataKey, row.DataValue);
			}
			else
			{
				project.Data.App.Save(DataRepoGroupId, dataKey, row.DataValue);
			}

			view.TabViewBookmark.Import(project);
		}
	}

	public TabDataSettings ToDataSettings()
	{
		TabDataSettings settings = new()
		{
			ColumnNameOrder = ColumnNameOrder,
			Filter = Filter,
			SelectedRows = SelectedRows.Select(s => s.SelectedRow!).ToHashSet(),
			SelectionType = SelectionType.Link,
		};
		return settings;
	}
}
