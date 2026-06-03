namespace SideScroll.Tabs.Samples.DataGrid;

/// <summary>
/// Items without [Searchable] — only top-level Text and Number are matched.
/// Searching for a child letter (e.g. "a") finds nothing because child items
/// are never searched without the attribute.
/// </summary>
public class TabSampleFilterNoSearchable : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			string characters = "abcdefghijklmn";

			List<FilterItem> items = [];
			for (int i = 0; i < 7; i++)
			{
				items.Add(new FilterItem("Item " + i, i)
				{
					Child = new FilterItem(characters[i].ToString(), i)
				});
			}

			model.Items = items;
			model.MaxSearchDepth = 1;
			model.ShowSearch = true;
		}
	}
}

/// <summary>
/// Basic filter item without [Searchable].
/// Only top-level Text and Number properties are searched by the filter.
/// The Child field is only reachable via explicit [Searchable] attribute or depth prefix.
/// </summary>
public record FilterItem(string Text, int Number)
{
	public FilterItem? Child;

	public override string ToString() => Text;
}
