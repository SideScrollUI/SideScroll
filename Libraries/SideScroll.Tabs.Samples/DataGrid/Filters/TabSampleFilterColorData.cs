namespace SideScroll.Tabs.Samples.DataGrid;

/// <summary>
/// Shared color data used by both the flat data-grid and the recursive-tab filter examples.
/// The same three-level hierarchy (Group → Shade → Variant) is expressed two ways so the
/// difference in filter behaviour is easy to compare side-by-side.
/// </summary>
internal static class TabSampleFilterColorData
{
	private static readonly (string Group, string Shade, string[] Variants)[] _rows =
	[
		("Warm",    "Red",    ["Crimson", "Scarlet",  "Ruby"]),
		("Warm",    "Orange", ["Amber",   "Coral",    "Tangerine"]),
		("Warm",    "Yellow", ["Gold",    "Lemon",    "Canary"]),
		("Cool",    "Blue",   ["Navy",    "Sky",      "Cobalt"]),
		("Cool",    "Green",  ["Lime",    "Forest",   "Sage"]),
		("Cool",    "Purple", ["Violet",  "Lavender", "Plum"]),
		("Neutral", "White",  ["Ivory",   "Pearl",    "Snow"]),
		("Neutral", "Gray",   ["Ash",     "Slate",    "Silver"]),
		("Neutral", "Black",  ["Ebony",   "Onyx",     "Jet"]),
	];

	/// <summary>
	/// Flat rows for <see cref="TabSampleFilter3LevelData"/>: one <see cref="ColorDataItem"/> per
	/// variant, each carrying its shade and group as nested child/grandchild data.
	/// </summary>
	public static List<ColorDataItem> CreateDataItems()
	{
		List<ColorDataItem> items = [];
		int idx = 0;
		foreach (var (group, shade, variants) in _rows)
		{
			foreach (string variant in variants)
			{
				items.Add(new ColorDataItem(group, idx)
				{
					Child = new ColorDataChild(shade, idx)
					{
						Grandchild = new ColorDataGrandchild(variant, idx)
					}
				});
				idx++;
			}
		}
		return items;
	}

	/// <summary>
	/// Recursive ITab groups for <see cref="TabSampleFilter3LevelRecursive"/>: each group contains
	/// shades, each shade contains variants, and each level is its own navigable tab.
	/// </summary>
	public static List<FilterColorGroup> CreateGroups()
	{
		var grouped = new Dictionary<string, List<(string shade, string[] variants)>>();
		foreach (var (group, shade, variants) in _rows)
		{
			if (!grouped.TryGetValue(group, out var list))
			{
				grouped[group] = list = [];
			}
			list.Add((shade, variants));
		}

		return [.. grouped.Select(kv =>
			new FilterColorGroup(kv.Key,
				kv.Value.Select(s =>
					new FilterColorShade(s.shade,
						s.variants.Select(v => new FilterColorVariant(v)).ToList())
				).ToList()))];
	}
}
