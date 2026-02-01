using SideScroll.Attributes;
using SideScroll.Tabs.Settings;
using System.Text.Json.Serialization;

namespace SideScroll.Tabs.Bookmarks.Models;

[PublicData]
public class TabDataBookmark
{
	[PrivateData]
	public SelectionType SelectionType { get; set; } = SelectionType.None;

	[JsonIgnore]
	public List<string> ColumnNameOrder { get; set; } = []; // Order to show the columns in, users can drag columns around to reorder these

	// Not currently supported by Avalonia DataGrid
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
				.Select(s => s.SelectedRow.Label)
				.ToList();

			if (labels.Count <= 1)
				return labels.FirstOrDefault();

			return '[' + string.Join(", ", labels) + ']';
		}
	}

	public string GetAddress(int maxDepth = 100, HashSet<TabBookmark>? visited = null)
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
		foreach (SelectedRowView selectedRowView in SelectedRows)
		{
			address += comma;
			address += selectedRowView.ToString() + " / " + selectedRowView.TabBookmark.GetAddress(maxDepth - 1, visited);
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
		SelectionType = SelectionType.Link;
		foreach (SelectedRowView selectedRowView in SelectedRows)
		{
			var row = selectedRowView.SelectedRow;
			string? dataKey = row.DataKey ?? row.Label;
			if (dataKey != null && row.DataValue != null)
			{
				// Interfaces and base classes need to specify a type
				if (DataRepoType is Type type)
				{
					project.Data.App.Save(type, DataRepoGroupId, dataKey, row.DataValue);
				}
				else
				{
					project.Data.App.Save(DataRepoGroupId, dataKey, row.DataValue);
				}
			}
			selectedRowView.TabBookmark.Import(project);
		}
	}

	public void SetSelectionType(SelectionType selectionType)
	{
		SelectionType = selectionType;
		foreach (SelectedRowView selectedRowView in SelectedRows)
		{
			selectedRowView.TabBookmark.SetSelectionType(selectionType);
		}
	}

	public TabDataSettings ToDataSettings()
	{
		return new TabDataSettings
		{
			ColumnNameOrder = ColumnNameOrder,
			Filter = Filter,
			SelectedRows = [.. SelectedRows.Select(s => s.SelectedRow)],
			SelectionType = SelectionType,
		};
	}
}
