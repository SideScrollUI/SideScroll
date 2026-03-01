using SideScroll.Attributes;
using System.Text.Json.Serialization;

namespace SideScroll.Tabs.Bookmarks.Models;

/// <summary>
/// Represents a selected row with its associated child tab bookmark for hierarchical navigation
/// </summary>
[PublicData]
public class SelectedRowView
{
	/// <summary>
	/// Gets or sets the selected row data
	/// </summary>
	public SelectedRow SelectedRow { get; set; }

	/// <summary>
	/// Gets or sets the child tab bookmark for this selection
	/// </summary>
	public TabBookmark TabBookmark { get; set; }

	public override string? ToString() => SelectedRow.ToString();

	/// <summary>
	/// Initializes a new selected row view with a selected row and optional child bookmark
	/// </summary>
	[JsonConstructor]
	public SelectedRowView(SelectedRow selectedRow, TabBookmark? tabBookmark = null)
	{
		SelectedRow = selectedRow;
		TabBookmark = tabBookmark ?? new();
	}

	/// <summary>
	/// Initializes a new selected row view from an object and optional child bookmark
	/// </summary>
	public SelectedRowView(object obj, TabBookmark? tabBookmark = null) : 
		this(new SelectedRow(obj), tabBookmark)
	{
	}
}
