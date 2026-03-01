using SideScroll.Attributes;
using SideScroll.Tabs.Settings;
using System.Text.Json.Serialization;

namespace SideScroll.Tabs.Bookmarks.Models;

/// <summary>
/// Represents tab data state including selected rows, filters, and column settings for bookmark persistence
/// </summary>
[PublicData]
public class TabDataBookmark
{
	/// <summary>
	/// Gets or sets the selection type (user, link, or none)
	/// </summary>
	[PrivateData]
	public SelectionType SelectionType { get; set; } = SelectionType.None;

	/// <summary>
	/// Gets or sets the column display order, users can drag columns around to reorder these
	/// </summary>
	[JsonIgnore]
	public List<string> ColumnNameOrder { get; set; } = [];

	// Not currently supported by Avalonia DataGrid
	// public string? SortColumnName { get; set; } // Currently sorted column
	// public ListSortDirection SortDirection { get; set; }

	/// <summary>
	/// Gets or sets the data repository group identifier
	/// </summary>
	public string? DataRepoGroupId { get; set; }

	/// <summary>
	/// Gets or sets the data repository type (required for interfaces)
	/// </summary>
	public Type? DataRepoType { get; set; }

	/// <summary>
	/// Gets or sets the list of selected rows with their child bookmarks
	/// </summary>
	public List<SelectedRowView> SelectedRows { get; set; } = [];

	/// <summary>
	/// Gets or sets the filter text applied to the data
	/// </summary>
	public string? Filter { get; set; }

	/// <summary>
	/// Gets the navigation address for this tab data
	/// </summary>
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

	/// <summary>
	/// Gets the full hierarchical address including child bookmark addresses
	/// </summary>
	/// <param name="maxDepth">Maximum recursion depth to prevent infinite loops</param>
	/// <param name="visited">Set of visited bookmarks to prevent cycles</param>
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

	/// <summary>
	/// Imports this tab data's selected row data values into the project's data repository
	/// </summary>
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

	/// <summary>
	/// Sets the selection type for this tab data and all child bookmarks
	/// </summary>
	public void SetSelectionType(SelectionType selectionType)
	{
		SelectionType = selectionType;
		foreach (SelectedRowView selectedRowView in SelectedRows)
		{
			selectedRowView.TabBookmark.SetSelectionType(selectionType);
		}
	}

	/// <summary>
	/// Converts this tab data bookmark to tab data settings
	/// </summary>
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
