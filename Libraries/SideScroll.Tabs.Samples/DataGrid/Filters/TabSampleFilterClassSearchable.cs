using SideScroll.Attributes;

namespace SideScroll.Tabs.Samples.DataGrid;

/// <summary>
/// Items with class-level [Searchable] — the filter recurses into child items.
/// Searching for a child letter (e.g. "a") finds "Item 0" because its Child.Text is "a".
/// Compare with <see cref="TabSampleFilterNoSearchable"/> where the same search finds nothing.
/// </summary>
public class TabSampleFilterClassSearchable : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			string characters = "abcdefghijklmn";

			List<SearchableFilterItem> items = [];
			for (int i = 0; i < 7; i++)
			{
				items.Add(new SearchableFilterItem("Item " + i, i)
				{
					Child = new SearchableFilterItem(characters[i].ToString(), i)
				});
			}

			model.Items = items;
			model.MaxSearchDepth = 1;
			model.ShowSearch = true;
		}
	}
}

/// <summary>
/// Filter item with class-level [Searchable].
/// When [Searchable] is present, FindMatches recurses into child items up to MaxSearchDepth.
/// </summary>
[Searchable]
public record SearchableFilterItem(string Text, int Number)
{
	public SearchableFilterItem? Child;

	public override string ToString() => Text;
}
