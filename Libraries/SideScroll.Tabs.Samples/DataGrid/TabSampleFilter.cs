using SideScroll.Attributes;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleFilter : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("No [Searchable]", new TabFilterNoSearchable()),
				new("Class [Searchable]", new TabFilterClassSearchable()),
				new("Property [Searchable]", new TabFilterPropertySearchable()),
			};
		}
	}

	/// <summary>
	/// Items without [Searchable] - only top-level Text and Number properties are searched.
	/// Searching for child text (e.g. "a") will not find any items in this list.
	/// </summary>
	private class TabFilterNoSearchable : ITab
	{
		public TabInstance Create() => new Instance();

		private class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				string characters = "abcdefghijklmn";

				List<TestFilterItem> items = [];
				for (int i = 0; i < 7; i++)
				{
					items.Add(new TestFilterItem("Item " + i, i)
					{
						Child = new TestFilterItem(characters[i].ToString(), i)
					});
				}

				model.Items = items;
				model.MaxSearchDepth = 1;
				model.ShowSearch = true;
			}
		}
	}

	/// <summary>
	/// Items with class-level [Searchable] - child items are also searched.
	/// Searching for child text (e.g. "a") will find "Item 0" because its Child has Text "a".
	/// </summary>
	private class TabFilterClassSearchable : ITab
	{
		public TabInstance Create() => new Instance();

		private class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				string characters = "abcdefghijklmn";

				List<TestSearchableFilterItem> items = [];
				for (int i = 0; i < 7; i++)
				{
					items.Add(new TestSearchableFilterItem("Item " + i, i)
					{
						Child = new TestSearchableFilterItem(characters[i].ToString(), i)
					});
				}

				model.Items = items;
				model.MaxSearchDepth = 1;
				model.ShowSearch = true;
			}
		}
	}

	/// <summary>
	/// Items with property-level [Searchable] - only the SearchableChild property's children are searched.
	/// Both children have the same letter text, but only SearchableChild is searched.
	/// </summary>
	private class TabFilterPropertySearchable : ITab
	{
		public TabInstance Create() => new Instance();

		private class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				string characters = "abcdefghijklmn";

				List<TestPropertySearchableItem> items = [];
				for (int i = 0; i < 7; i++)
				{
					items.Add(new TestPropertySearchableItem("Item " + i, i)
					{
						SearchableChild = new TestFilterItem(characters[i].ToString(), i),
						NonSearchableChild = new TestFilterItem(characters[i + 7].ToString(), i + 7),
					});
				}

				model.Items = items;
				model.MaxSearchDepth = 1;
				model.ShowSearch = true;
			}
		}
	}

	/// <summary>
	/// Filter item without [Searchable] - only top-level Text and Number properties are searched.
	/// </summary>
	public record TestFilterItem(string Text, int Number)
	{
		public TestFilterItem? Child;

		public override string ToString() => Text;
	}

	/// <summary>
	/// Filter item with class-level [Searchable] - child items are also searched.
	/// </summary>
	[Searchable]
	public record TestSearchableFilterItem(string Text, int Number)
	{
		public TestSearchableFilterItem? Child;

		public override string ToString() => Text;
	}

	/// <summary>
	/// Filter item with property-level [Searchable] on SearchableChild only.
	/// </summary>
	public record TestPropertySearchableItem(string Text, int Number)
	{
		[Searchable]
		public TestFilterItem? SearchableChild { get; set; }

		public TestFilterItem? NonSearchableChild { get; set; }

		public override string ToString() => Text;
	}
}
