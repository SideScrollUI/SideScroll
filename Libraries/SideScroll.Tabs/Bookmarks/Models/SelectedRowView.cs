using SideScroll.Attributes;

namespace SideScroll.Tabs.Bookmarks.Models;

[PublicData]
public class SelectedRowView(SelectedRow selectedRow, TabBookmark? tabBookmark = null)
{
	public SelectedRow SelectedRow { get; set; } = selectedRow;

	public TabBookmark TabBookmark { get; set; } = tabBookmark ?? new();

	public override string? ToString() => SelectedRow.ToString();

	public SelectedRowView(object obj, TabBookmark? tabBookmark = null) : 
		this(new SelectedRow(obj), tabBookmark)
	{
	}
}
