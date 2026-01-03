using SideScroll.Attributes;

namespace SideScroll.Tabs.Bookmarks.Models;

[PublicData]
public class SelectedRowView(SelectedRow selectedRow, TabViewBookmark? tabViewBookmark = null)
{
	public SelectedRow SelectedRow { get; set; } = selectedRow;

	public TabViewBookmark TabViewBookmark { get; set; } = tabViewBookmark ?? new();

	public override string? ToString() => SelectedRow.ToString();

	public SelectedRowView(string label, TabViewBookmark? tabViewBookmark = null) : 
		this(new SelectedRow(label), tabViewBookmark)
	{
	}
}
