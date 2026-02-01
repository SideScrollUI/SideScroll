using SideScroll.Attributes;
using System.Text.Json.Serialization;

namespace SideScroll.Tabs.Bookmarks.Models;

[PublicData]
public class SelectedRowView
{
	public SelectedRow SelectedRow { get; set; }

	public TabBookmark TabBookmark { get; set; }

	public override string? ToString() => SelectedRow.ToString();

	[JsonConstructor]
	public SelectedRowView(SelectedRow selectedRow, TabBookmark? tabBookmark = null)
	{
		SelectedRow = selectedRow;
		TabBookmark = tabBookmark ?? new();
	}

	public SelectedRowView(object obj, TabBookmark? tabBookmark = null) : 
		this(new SelectedRow(obj), tabBookmark)
	{
	}
}
