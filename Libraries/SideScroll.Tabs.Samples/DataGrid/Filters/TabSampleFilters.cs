using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Samples.DataGrid.Filters;

/// <summary>
/// Demonstrates the DataGrid filter and the [Searchable] attribute that controls
/// how deep it searches into nested data.
///
/// Entries:
///   No [Searchable]           — filter only matches top-level properties.
///   Class [Searchable]        — filter recurses into child items when the class is marked.
///   Property [Searchable]     — filter recurses only through the marked property.
///   3 Levels (Data)           — [Searchable] on all three data levels; filter reaches grandchild text.
///   3 Levels (Recursive Tabs) — same hierarchy exposed as ITab items; filter typed at
///                               the root is inherited by child tabs via
///                               TabDataGrid.FilterText checking ParentTabInstance.FilterBookmarkNode.
/// </summary>
public class TabSampleFilters : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("No [Searchable]",             new TabSampleFilterNoSearchable()),
				new("Class [Searchable]",          new TabSampleFilterClassSearchable()),
				new("Property [Searchable]",       new TabSampleFilterPropertySearchable()),
				new("3 Levels (Data)",             new TabSampleFilter3LevelData()),
				new("3 Levels (Recursive Tabs)",   new TabSampleFilter3LevelRecursive()),
			};
		}
	}
}
