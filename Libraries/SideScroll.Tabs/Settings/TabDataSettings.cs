using SideScroll.Tabs.Bookmarks.Models;

namespace SideScroll.Tabs.Settings;

public enum SelectionType
{
	None,
	User,
	Auto,
	Link,
}

public class TabDataSettings
{
	public HashSet<SelectedRow> SelectedRows { get; set; } = [];
	public SelectionType SelectionType { get; set; } = SelectionType.None;

	// Order to show the columns in, users can drag columns around to reorder these
	public List<string> ColumnNameOrder { get; set; } = [];

	// Not currently supported by Avalonia DataGrid
	// public string? SortColumnName { get; set; } // Currently sorted column
	// public ListSortDirection SortDirection { get; set; }

	public string? Filter { get; set; }
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
