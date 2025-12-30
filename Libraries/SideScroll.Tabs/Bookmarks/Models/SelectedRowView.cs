using SideScroll.Attributes;

namespace SideScroll.Tabs.Bookmarks.Models;

[PublicData]
public class SelectedRowView
{
	public SelectedRow? SelectedRow { get; set; } // The parent selection that created this bookmark

	public TabViewBookmark TabViewBookmark { get; set; } = new();

	public override string? ToString() => SelectedRow?.ToString();

	public SelectedRowView() { }

	public SelectedRowView(SelectedRow selectedRow)
	{
		SelectedRow = selectedRow;
	}

	/*public void SelectPath(params string[] labels)
	{
		SelectedRowView tabBookmark = this;
		foreach (string label in labels)
		{
			tabBookmark.SelectRows(label);
			tabBookmark = AddChild(label);
		}
	}

	public void SelectRows(params string[] labels)
	{
		var selectedRows = labels.Select(label =>
			new SelectedRowView
			{
				SelectedRow = new()
				{
					Label = label,
				}
			}
		).ToHashSet();

		SelectRows(selectedRows);
	}

	private void SelectRows(ICollection<SelectedRowView> selectedRows)
	{
		TabViewBookmark = new TabViewBookmark
		{
			TabDatas =
			[
				new TabDataBookmark
				{
					SelectionType = SelectionType.User,
					Selected = selectedRows.ToList(),
				}
			],
		};
	}*/

	public SelectedRowView AddChild(string dataKey)
	{
		SelectedRowView childBookmark = new()
		{
			SelectedRow = new(dataKey),
		};
		TabViewBookmark.TabDatas.First().Selected.Add(childBookmark);
		return childBookmark;
	}

	/*public SelectedRowView? GetChild(string dataKey)
	{
		return ViewSettings.ChildBookmarks.GetValueOrDefault(dataKey);
	}*/
}
