using SideScroll.Attributes;

namespace SideScroll.Tabs.Bookmarks.Models;

[PublicData]
public class SelectedRowView(SelectedRow selectedRow, TabViewBookmark? tabViewBookmark = null)
{
	public SelectedRow SelectedRow => selectedRow;

	public TabViewBookmark TabViewBookmark { get; set; } = tabViewBookmark ?? new();

	public override string? ToString() => SelectedRow.ToString();
}
