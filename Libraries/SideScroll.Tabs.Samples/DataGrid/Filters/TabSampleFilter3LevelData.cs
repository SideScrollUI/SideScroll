using SideScroll.Attributes;

namespace SideScroll.Tabs.Samples.DataGrid.Filters;

/// <summary>
/// Flat data grid where each root item embeds a child and grandchild as data properties.
/// [Searchable] on all three levels lets FindMatches reach grandchild text with MaxSearchDepth = 2.
/// Try searching "crimson" — only "Warm" rows are shown because Crimson lives under Warm → Red.
/// </summary>
public class TabSampleFilter3LevelData : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = TabSampleFilterColorData.CreateDataItems();
			model.MaxSearchDepth = 2;
			model.ShowSearch = true;
		}
	}
}

/// <summary>
/// Level-1 data item. [Searchable] enables FindMatches to recurse into <see cref="Child"/>.
/// </summary>
[Searchable]
public class ColorDataItem(string text, int number)
{
	public string Text => text;

	[DataKey]
	public int Number => number;

	[HiddenColumn]
	public ColorDataChild? Child { get; set; }

	public override string ToString() => Text;
}

/// <summary>
/// Level-2 data item. [Searchable] enables FindMatches to recurse into <see cref="Grandchild"/>.
/// </summary>
[Searchable]
public record ColorDataChild(string Text, int Number)
{
	[HiddenColumn]
	public ColorDataGrandchild? Grandchild { get; set; }

	public override string ToString() => Text;
}

/// <summary>
/// Level-3 (leaf) data item. The color name is the final search target.
/// No [Searchable] needed — it is not searched recursively, only matched directly.
/// </summary>
public record ColorDataGrandchild(string Text, int Number)
{
	public override string ToString() => Text;
}
