using SideScroll.Attributes;

namespace SideScroll.Tabs.Bookmarks.Models;

[PublicData]
public class SelectedRowView
{
	public SelectedRow? SelectedRow { get; set; } // The parent selection that created this bookmark

	public TabViewBookmark TabViewBookmark { get; set; } = new(); // The child TabView

	public override string? ToString() => SelectedRow?.ToString();

	public SelectedRowView() { }

	public SelectedRowView(SelectedRow selectedRow)
	{
		SelectedRow = selectedRow;
	}
}
