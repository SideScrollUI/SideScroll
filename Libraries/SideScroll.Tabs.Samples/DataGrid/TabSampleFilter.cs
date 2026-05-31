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
				new("3 Levels [Searchable]", new TabFilter3LevelSearchable()),
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
						SearchableChild = new TestFilterItem(new string(characters[i], 3), i),
						NonSearchableChild = new TestFilterItem(new string(characters[i + 7], 3), i + 7),
					});
				}

				model.Items = items;
				model.MaxSearchDepth = 3;
				model.ShowSearch = true;
			}
		}
	}

	/// <summary>
	/// 3-level hierarchy with [Searchable] on item and child classes.
	/// Each grandchild has a unique color name. Searching for a color (e.g. "red") finds
	/// only the item whose grandchild contains that color.
	/// Requires MaxSearchDepth = 2 to reach the grandchild level.
	/// </summary>
	private class TabFilter3LevelSearchable : ITab
	{
		public TabInstance Create() => new Instance();

		private class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				string[] colors = ["red", "green", "blue", "cyan", "magenta", "yellow", "white"];

				List<Test3LevelItem> items = [];
				for (int i = 0; i < 7; i++)
				{
					items.Add(new Test3LevelItem("Item " + i, i)
					{
						Child = new Test3LevelChild("Child " + i, i)
						{
							Grandchild = new Test3LevelGrandchild(colors[i], i)
						}
					});
				}

				model.Items = items;
				model.MaxSearchDepth = 4;
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
		[Searchable, HiddenColumn]
		public TestFilterItem? SearchableChild { get; set; }

		[HiddenColumn]
		public TestFilterItem? NonSearchableChild { get; set; }

		public override string ToString() => Text;
	}

	/// <summary>
	/// Top-level item for the 3-level search example. [Searchable] enables deep child search.
	/// </summary>
	[Searchable]
	public record Test3LevelItem(string Text, int Number)
	{
		[HiddenColumn]
		public Test3LevelChild? Child { get; set; }

		public override string ToString() => Text;
	}

	/// <summary>
	/// Second-level item for the 3-level search example. [Searchable] enables grandchild search.
	/// </summary>
	[Searchable]
	public record Test3LevelChild(string Text, int Number)
	{
		[HiddenColumn]
		public Test3LevelGrandchild? Grandchild { get; set; }

		public override string ToString() => Text;
	}

	/// <summary>
	/// Third-level (leaf) item for the 3-level search example.
	/// Each instance has a unique color name used as the searchable text.
	/// </summary>
	[Searchable]
	public record Test3LevelGrandchild(string Text, int Number)
	{
		public override string ToString() => Text;
	}
}
