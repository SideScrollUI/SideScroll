using SideScroll.Attributes;

namespace SideScroll.Tabs.Samples.DataGrid.Filters;

/// <summary>
/// Items with property-level [Searchable] on SearchableChild only.
/// Both children share the same letter text, but only SearchableChild is searched.
/// This demonstrates that [Searchable] can be applied selectively per property
/// rather than enabling deep search for the entire class.
/// </summary>
public class TabSampleFilterPropertySearchable : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.ShowSearch = true;
			model.MaxSearchDepth = 3;

			string characters = "abcdefghijklmn";

			List<PropertySearchableItem> items = [];
			for (int i = 0; i < 7; i++)
			{
				items.Add(new PropertySearchableItem("Item " + i, i)
				{
					SearchableChild = new FilterItem(new string(characters[i], 3), i),
					NonSearchableChild = new FilterItem(new string(characters[i + 7], 3), i + 7),
				});
			}

			model.AddItems(items);
		}
	}
}

/// <summary>
/// Filter item with property-level [Searchable] on SearchableChild only.
/// Searching for text in SearchableChild finds this item; text in NonSearchableChild does not.
/// </summary>
public record PropertySearchableItem(string Text, int Number)
{
	/// <summary>[Searchable] enables deep search through this property's children.</summary>
	[Searchable, HiddenColumn]
	public FilterItem? SearchableChild { get; set; }

	/// <summary>No [Searchable] — text in this child's subtree is not searched.</summary>
	[HiddenColumn]
	public FilterItem? NonSearchableChild { get; set; }

	public override string ToString() => Text;
}
