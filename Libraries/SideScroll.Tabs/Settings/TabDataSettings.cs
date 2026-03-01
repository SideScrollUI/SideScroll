using SideScroll.Tabs.Bookmarks.Models;

namespace SideScroll.Tabs.Settings;

/// <summary>
/// Defines how rows were selected in a tab
/// </summary>
public enum SelectionType
{
	/// <summary>
	/// No selection type specified
	/// </summary>
	None,

	/// <summary>
	/// User manually selected the rows
	/// </summary>
	User,

	/// <summary>
	/// Rows were automatically selected
	/// </summary>
	Auto,

	/// <summary>
	/// Rows were selected via a bookmark link
	/// </summary>
	Link,
}

/// <summary>
/// Represents tab data settings including selected rows, column order, filters, and selection type
/// </summary>
public class TabDataSettings
{
	/// <summary>
	/// Gets or sets the set of selected rows
	/// </summary>
	public HashSet<SelectedRow> SelectedRows { get; set; } = [];

	/// <summary>
	/// Gets or sets how the rows were selected
	/// </summary>
	public SelectionType SelectionType { get; set; } = SelectionType.None;

	/// <summary>
	/// Gets or sets the column display order (users can drag columns to reorder)
	/// </summary>
	public List<string> ColumnNameOrder { get; set; } = [];

	// Not currently supported by Avalonia DataGrid
	// public string? SortColumnName { get; set; } // Currently sorted column
	// public ListSortDirection SortDirection { get; set; }

	/// <summary>
	/// Gets or sets the filter text applied to the data
	/// </summary>
	public string? Filter { get; set; }

	/// <summary>
	/// Gets the navigation address for the selected rows
	/// </summary>
	public string? Address
	{
		get
		{
			var labels = SelectedRows.Select(s => s.Label).ToList();
			if (labels.Count <= 1)
				return labels.FirstOrDefault();

			return '[' + string.Join(", ", labels) + ']';
		}
	}

	public override string? ToString() => Address;
}
