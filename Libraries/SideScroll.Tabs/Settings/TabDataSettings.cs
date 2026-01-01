using SideScroll.Attributes;
using SideScroll.Tabs.Bookmarks.Models;

namespace SideScroll.Tabs.Settings;

public enum SelectionType
{
	None,
	User,
	Auto,
	Link,
}

[PublicData]
public class TabDataSettings
{
	public HashSet<SelectedRow> SelectedRows { get; set; } = []; // needs to be nullable or we need another initialized value
	public SelectionType SelectionType { get; set; } = SelectionType.None;
	public List<string> ColumnNameOrder { get; set; } = []; // Order to show the columns in, users can drag columns around to reorder these

	// Not currently supported by DataGrid
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

			return "[" + string.Join(", ", labels) + "] ";
		}
	}

	public override string? ToString() => Address;
}
