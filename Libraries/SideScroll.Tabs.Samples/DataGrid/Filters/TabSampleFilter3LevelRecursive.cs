using SideScroll.Attributes;

namespace SideScroll.Tabs.Samples.DataGrid.Filters;

/// <summary>
/// The same color hierarchy as <see cref="TabSampleFilter3LevelData"/> but exposed as ITab
/// items, so navigating into a row opens a child tab for that level.
///
/// Filter for "crimson" at the root: only "Warm" is shown (FindMatches with MaxSearchDepth = 4
/// reaches FilterColorVariant through the extra ListMember/List indirection layers).
///
/// Navigate into "Warm": the level-2 tab opens already filtered to "Red" because
/// TabDataGrid.FilterText inherits the parent's FilterBookmarkNode filter text when
/// the child tab loads with no saved filter and MaxSearchDepth > 0.
/// </summary>
public class TabSampleFilter3LevelRecursive : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = TabSampleFilterColorData.CreateGroups();
			// Depth 4 is needed to reach FilterColorVariant through the recursive ITab
			// indirection: FilterColorGroup → (ListMembers) → Shades list → FilterColorShade
			// → (ListMembers) → Variants list → FilterColorVariant.ToString().
			model.MaxSearchDepth = 4;
			model.ShowSearch = true;
		}
	}
}

/// <summary>
/// Level-1 recursive item. [Searchable] lets FindMatches recurse into the child shades.
/// Opening it shows the level-2 tab with the shade list.
/// </summary>
[Searchable]
public class FilterColorGroup(string name, List<FilterColorShade> shades) : ITab
{
	public string Name => name;

	[HiddenColumn]
	public List<FilterColorShade> Shades => shades;

	public TabInstance Create() => new Instance(this);

	public override string ToString() => Name;

	private class Instance(FilterColorGroup group) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.CustomSettingsPath = group.Name;
			model.Items = group.Shades;
			// Depth 2 so the inherited parent filter can reach FilterColorVariant:
			// FilterColorShade → (ListMembers) → Variants list → FilterColorVariant.ToString().
			model.MaxSearchDepth = 2;
			model.ShowSearch = true;
		}
	}
}

/// <summary>
/// Level-2 recursive item. [Searchable] lets FindMatches recurse into the grandchild variants.
/// Opening it shows the level-3 tab with the variant list.
/// </summary>
[Searchable]
public class FilterColorShade(string name, List<FilterColorVariant> variants) : ITab
{
	public string Name => name;

	[HiddenColumn]
	public List<FilterColorVariant> Variants => variants;

	public TabInstance Create() => new Instance(this);

	public override string ToString() => Name;

	private class Instance(FilterColorShade shade) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.CustomSettingsPath = shade.Name;
			model.Items = shade.Variants;
			model.ShowSearch = true;
		}
	}
}

/// <summary>
/// Level-3 (leaf) recursive item. The color name is the final search target.
/// </summary>
public class FilterColorVariant(string name)
{
	public string Name => name;

	public override string ToString() => Name;
}
