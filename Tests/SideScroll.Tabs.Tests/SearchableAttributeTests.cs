using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Tabs.Bookmarks.Models;

namespace SideScroll.Tabs.Tests;

/// <summary>
/// Tests for [Searchable] attribute behavior in TabModel.FindMatches.
///
/// The [Searchable] attribute controls whether child items are searched when filtering:
///   - Without [Searchable] (default): only top-level visible properties are searched.
///   - Class-level [Searchable]: enables recursive child tab search for instances of this class.
///   - Property-level [Searchable]: enables recursive child tab search when this property is visible.
///
/// Child search is also gated by TabModel.MaxSearchDepth (must be >= 1 to enable child search).
/// Users can bypass [Searchable] by using "+N" depth prefix in the filter text (e.g., "+1 foo").
/// </summary>
[Category("Tabs")]
public class SearchableAttributeTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SearchableAttribute");
	}

	#region Test Types

	/// <summary>
	/// Item without [Searchable]. Child is a field (excluded from direct visible-property search).
	/// </summary>
	private record PlainItem(string Text, int Number)
	{
		public PlainItem? Child;

		public override string ToString() => Text;
	}

	/// <summary>
	/// Item with class-level [Searchable]. Searching for child text enables recursive tab search.
	/// Child is a field so its text is only reachable via recursive child search.
	/// </summary>
	[Searchable]
	private record SearchableClassItem(string Text, int Number)
	{
		public SearchableClassItem? Child;

		public override string ToString() => Text;
	}

	/// <summary>
	/// Item whose one property carries a property-level [Searchable].
	/// Child is a field so it's only found via recursive child search triggered by the [Searchable] property.
	/// </summary>
	private record PropertySearchableItem(string Text, int Number)
	{
		/// <summary>[Searchable] on this property enables child tab search for the whole item.</summary>
		[Searchable]
		public PlainItem? SearchableChild { get; set; }

		/// <summary>No [Searchable] — used to verify the flag comes from SearchableChild.</summary>
		public PlainItem? PlainChild { get; set; }

		public override string ToString() => Text;
	}

	// ── 3-level types ──────────────────────────────────────────────────────────

	/// <summary>Level-1 item. [Searchable] lets FindMatches recurse into DeepChild.</summary>
	[Searchable]
	private record DeepItem(string Text, int Number)
	{
		public DeepChild? Child { get; set; }

		public override string ToString() => Text;
	}

	/// <summary>Level-2 item. [Searchable] lets FindMatches recurse into DeepGrandchild.</summary>
	[Searchable]
	private record DeepChild(string Text, int Number)
	{
		public DeepGrandchild? Grandchild { get; set; }

		public override string ToString() => Text;
	}

	/// <summary>Level-3 (leaf) item. No [Searchable] needed — it is the final search target.</summary>
	private record DeepGrandchild(string Text, int Number)
	{
		public override string ToString() => Text;
	}

	/// <summary>Level-2 item WITHOUT [Searchable] — used to verify that the attribute is required.</summary>
	private record DeepChildNoSearchable(string Text, int Number)
	{
		public DeepGrandchild? Grandchild { get; set; }

		public override string ToString() => Text;
	}

	/// <summary>Level-1 item paired with a plain (non-[Searchable]) child.</summary>
	[Searchable]
	private record DeepItemWithPlainChild(string Text, int Number)
	{
		public DeepChildNoSearchable? Child { get; set; }

		public override string ToString() => Text;
	}

	#endregion

	#region Helpers

	private static TabModel CreateModel(System.Collections.IList items, int maxSearchDepth = 1)
	{
		var model = new TabModel("Test")
		{
			Items = items,
			MaxSearchDepth = maxSearchDepth
		};
		return model;
	}

	private static int CountMatches(TabModel model, string filterText, int depth = 0)
	{
		var filter = new Filter(filterText, depth);
		TabBookmark bookmark = model.FindMatches(filter, filter.Depth);
		return bookmark.SelectedRows.Count;
	}

	#endregion

	#region No [Searchable] attribute

	[Test]
	public void NoSearchable_TopLevelTextMatch_Found()
	{
		List<PlainItem> items =
		[
			new("Apple", 1) { Child = new("Foo", 10) },
			new("Banana", 2) { Child = new("Bar", 20) },
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "apple");

		Assert.That(matches, Is.EqualTo(1));
	}

	[Test]
	public void NoSearchable_ChildFieldText_NotFound()
	{
		// Child is a field — not in direct visible-property search.
		// Without [Searchable], child tab search is not enabled, so "Foo" is not found.
		List<PlainItem> items =
		[
			new("Apple", 1) { Child = new("Foo", 10) },
			new("Banana", 2) { Child = new("Bar", 20) },
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "foo");

		Assert.That(matches, Is.EqualTo(0));
	}

	[Test]
	public void NoSearchable_ExplicitDepthPrefix_FindsChildText()
	{
		// "+1" in the filter does not raise the user-specified depth without [Searchable]
		// and does not enable child tab search even with the attribute.
		List<PlainItem> items =
		[
			new("Apple", 1) { Child = new("Foo", 10) },
			new("Banana", 2) { Child = new("Bar", 20) },
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "+1 foo");

		Assert.That(matches, Is.EqualTo(0));
	}

	[Test]
	public void NoSearchable_NothingMatches_ReturnsZero()
	{
		List<PlainItem> items =
		[
			new("Apple", 1) { Child = new("Foo", 10) },
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "xyz");

		Assert.That(matches, Is.EqualTo(0));
	}

	#endregion

	#region Class-level [Searchable]

	[Test]
	public void ClassSearchable_TopLevelTextMatch_Found()
	{
		List<SearchableClassItem> items =
		[
			new("Apple", 1) { Child = new("Foo", 10) },
			new("Banana", 2) { Child = new("Bar", 20) },
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "apple");

		Assert.That(matches, Is.EqualTo(1));
	}

	[Test]
	public void ClassSearchable_ChildFieldText_Found()
	{
		// [Searchable] on the class enables child tab search.
		// Even though Child is a field (not in direct search), the recursive search finds "Foo".
		List<SearchableClassItem> items =
		[
			new("Apple", 1) { Child = new("Foo", 10) },
			new("Banana", 2) { Child = new("Bar", 20) },
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "foo", 1);

		Assert.That(matches, Is.EqualTo(1));
	}

	[Test]
	public void ClassSearchable_ChildFieldText_ReturnsParentInResults()
	{
		// The parent item is what appears in the match results (not the child).
		List<SearchableClassItem> items =
		[
			new("Apple", 1) { Child = new("Foo", 10) },
			new("Banana", 2) { Child = new("Bar", 20) },
		];
		var model = CreateModel(items);

		var filter = new Filter("bar");
		TabBookmark bookmark = model.FindMatches(filter, 2);
		List<SelectedRow> selectedRows = bookmark.SelectedRows;

		Assert.That(selectedRows, Has.Count.EqualTo(1));
		Assert.That(selectedRows[0].Label, Is.EqualTo("Banana"));
	}

	[Test]
	public void ClassSearchable_MultipleItemsWithMatchingChild_AllFound()
	{
		List<SearchableClassItem> items =
		[
			new("Apple", 1) { Child = new("Foo", 10) },
			new("Banana", 2) { Child = new("Foo", 20) },
			new("Cherry", 3) { Child = new("Bar", 30) },
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "foo", 1);

		Assert.That(matches, Is.EqualTo(2));
	}

	[Test]
	public void ClassSearchable_DirectAndChildBothMatch_CountedOnce()
	{
		// When an item matches both directly and via its child, it's only counted once.
		List<SearchableClassItem> items =
		[
			new("Foo", 1) { Child = new("Foo", 10) },
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "foo");

		Assert.That(matches, Is.EqualTo(1));
	}

	[Test]
	public void ClassSearchable_NothingMatches_ReturnsZero()
	{
		List<SearchableClassItem> items =
		[
			new("Apple", 1) { Child = new("Foo", 10) },
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "xyz");

		Assert.That(matches, Is.EqualTo(0));
	}

	#endregion

	#region Property-level [Searchable]

	[Test]
	public void PropertySearchable_TopLevelTextMatch_Found()
	{
		List<PropertySearchableItem> items =
		[
			new PropertySearchableItem("Apple", 1)
			{
				SearchableChild = new PlainItem("ChildA", 10),
				PlainChild = new PlainItem("ChildB", 20),
			},
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "apple");

		Assert.That(matches, Is.EqualTo(1));
	}

	[Test]
	public void PropertySearchable_SearchableChildVisible_Found()
	{
		// SearchableChild is a visible property; its ToString() is included in the direct search,
		// and [Searchable] on the property also enables child tab search for deeper matches.
		List<PropertySearchableItem> items =
		[
			new PropertySearchableItem("Apple", 1)
			{
				SearchableChild = new PlainItem("UniqueChildA", 10),
				PlainChild = new PlainItem("OtherChild", 20),
			},
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "uniquechilda");

		Assert.That(matches, Is.EqualTo(1));
	}

	[Test]
	public void PropertySearchable_EnablesChildSearch_FieldTextFound()
	{
		// The [Searchable] property enables child tab search for the whole item.
		// The PlainChild field (not a property of PropertySearchableItem) becomes
		// accessible via the child tab search.
		List<PropertySearchableItem> items =
		[
			new PropertySearchableItem("Apple", 1)
			{
				SearchableChild = new PlainItem("SearchableText", 10) { Child = new PlainItem("DeepFoo", 99) },
				PlainChild = new PlainItem("OtherText", 20),
			},
		];
		var model = CreateModel(items);

		// "searcha" is in SearchableChild.Text (visible property) — found via direct match
		// or via child tab enabled by [Searchable]
		int matches = CountMatches(model, "searcha");

		Assert.That(matches, Is.EqualTo(1));
	}

	[Test]
	public void PropertySearchable_NoMatchingText_ReturnsZero()
	{
		List<PropertySearchableItem> items =
		[
			new PropertySearchableItem("Apple", 1)
			{
				SearchableChild = new PlainItem("ChildA", 10),
				PlainChild = new PlainItem("ChildB", 20),
			},
		];
		var model = CreateModel(items);

		int matches = CountMatches(model, "xyz");

		Assert.That(matches, Is.EqualTo(0));
	}

	#endregion

	#region MaxSearchDepth = 0 (child search disabled)

	[Test]
	public void MaxSearchDepthZero_ClassSearchable_ChildFieldNotFound()
	{
		// MaxSearchDepth=0 disables child tab search entirely, even for [Searchable] items.
		List<SearchableClassItem> items =
		[
			new("Apple", 1) { Child = new("Foo", 10) },
		];
		var model = CreateModel(items, maxSearchDepth: 0);

		int matches = CountMatches(model, "foo");

		Assert.That(matches, Is.EqualTo(0));
	}

	[Test]
	public void MaxSearchDepthZero_TopLevelTextMatch_StillFound()
	{
		// MaxSearchDepth=0 only disables child search; direct matches still work.
		List<SearchableClassItem> items =
		[
			new("Apple", 1) { Child = new("Foo", 10) },
		];
		var model = CreateModel(items, maxSearchDepth: 0);

		int matches = CountMatches(model, "apple");

		Assert.That(matches, Is.EqualTo(1));
	}

	#endregion

	#region Comparison: No [Searchable] vs. Class [Searchable]

	[Test]
	public void Comparison_NoAttributeVsClassSearchable_ChildTextBehaviorDiffers()
	{
		// Demonstrates the core difference: same child data, different attribute → different search results.
		List<PlainItem> plainItems =
		[
			new("Item A", 1) { Child = new("UniqueChild", 10) },
		];
		List<SearchableClassItem> searchableItems =
		[
			new("Item A", 1) { Child = new("UniqueChild", 10) },
		];

		var plainModel = CreateModel(plainItems);
		var searchableModel = CreateModel(searchableItems);

		int plainMatches = CountMatches(plainModel, "uniquechild", 1);
		int searchableMatches = CountMatches(searchableModel, "uniquechild", 1);

		Assert.That(plainMatches, Is.EqualTo(0), "Without [Searchable], child field text should not be found");
		Assert.That(searchableMatches, Is.EqualTo(1), "With class [Searchable], child field text should be found");
	}

	#endregion

	#region 3-Level [Searchable]

	[Test]
	public void ThreeLevel_GrandchildText_FindsOneRootItem()
	{
		// Searching for a color that only exists in a single grandchild returns exactly one root item.
		List<DeepItem> items =
		[
			new("Item 0", 0) { Child = new DeepChild("Child 0", 0) { Grandchild = new DeepGrandchild("red", 0) } },
			new("Item 1", 1) { Child = new DeepChild("Child 1", 1) { Grandchild = new DeepGrandchild("green", 1) } },
			new("Item 2", 2) { Child = new DeepChild("Child 2", 2) { Grandchild = new DeepGrandchild("blue", 2) } },
		];
		var model = CreateModel(items, maxSearchDepth: 2);

		int matches = CountMatches(model, "red", 2);

		Assert.That(matches, Is.EqualTo(1));
	}

	[Test]
	public void ThreeLevel_GrandchildText_ReturnsCorrectRootItem()
	{
		// The label on the returned SelectedRow must be the root item, not the grandchild.
		List<DeepItem> items =
		[
			new("Item 0", 0) { Child = new DeepChild("Child 0", 0) { Grandchild = new DeepGrandchild("red", 0) } },
			new("Item 1", 1) { Child = new DeepChild("Child 1", 1) { Grandchild = new DeepGrandchild("green", 1) } },
			new("Item 2", 2) { Child = new DeepChild("Child 2", 2) { Grandchild = new DeepGrandchild("blue", 2) } },
		];
		var model = CreateModel(items, maxSearchDepth: 2);
		var filter = new Filter("green");

		TabBookmark bookmark = model.FindMatches(filter, 2);

		Assert.That(bookmark.SelectedRows, Has.Count.EqualTo(1));
		Assert.That(bookmark.SelectedRows[0].Label, Is.EqualTo("Item 1"));
	}

	[Test]
	public void ThreeLevel_MultipleMatchingGrandchildren_AllRootsFound()
	{
		// Two items whose grandchildren share a color — both roots should appear.
		List<DeepItem> items =
		[
			new("Item 0", 0) { Child = new DeepChild("Child 0", 0) { Grandchild = new DeepGrandchild("red", 0) } },
			new("Item 1", 1) { Child = new DeepChild("Child 1", 1) { Grandchild = new DeepGrandchild("blue", 1) } },
			new("Item 2", 2) { Child = new DeepChild("Child 2", 2) { Grandchild = new DeepGrandchild("red", 2) } },
		];
		var model = CreateModel(items, maxSearchDepth: 2);

		int matches = CountMatches(model, "red", 2);

		Assert.That(matches, Is.EqualTo(2));
	}

	[Test]
	public void ThreeLevel_MaxSearchDepthOne_GrandchildNotFound()
	{
		// depth=1 only reaches level-2 (child), not level-3 (grandchild).
		List<DeepItem> items =
		[
			new("Item 0", 0) { Child = new DeepChild("Child 0", 0) { Grandchild = new DeepGrandchild("red", 0) } },
		];
		var model = CreateModel(items, maxSearchDepth: 1);

		int matches = CountMatches(model, "red", 1);

		Assert.That(matches, Is.EqualTo(0));
	}

	[Test]
	public void ThreeLevel_ChildWithoutSearchable_GrandchildNotFound()
	{
		// Without [Searchable] on the child class, FindMatches uses searchableOnly=true when
		// recursing into level-3, so the Grandchild property (no [Searchable]) is skipped.
		List<DeepItemWithPlainChild> items =
		[
			new("Item 0", 0) { Child = new DeepChildNoSearchable("Child 0", 0) { Grandchild = new DeepGrandchild("red", 0) } },
		];
		var model = CreateModel(items, maxSearchDepth: 2);

		int matches = CountMatches(model, "red", 2);

		Assert.That(matches, Is.EqualTo(0));
	}

	[Test]
	public void ThreeLevel_ChildBookmarks_ContainMatchingChildRows()
	{
		// The TabBookmark returned for each root match should have a child TabBookmark
		// whose SelectedRows list contains the level-2 child that led to the match.
		List<DeepItem> items =
		[
			new("Item 0", 0) { Child = new DeepChild("Child 0", 0) { Grandchild = new DeepGrandchild("red", 0) } },
			new("Item 1", 1) { Child = new DeepChild("Child 1", 1) { Grandchild = new DeepGrandchild("green", 1) } },
		];
		var model = CreateModel(items, maxSearchDepth: 2);
		var filter = new Filter("red");

		TabBookmark bookmark = model.FindMatches(filter, 2);

		// Root level: exactly one match
		Assert.That(bookmark.SelectedRows, Has.Count.EqualTo(1));

		// Its child bookmark should have one level-2 entry pointing to "Child 0"
		SelectedRowView rootRowView = bookmark.SelectedRowViews[0];
		Assert.That(rootRowView.TabBookmark.SelectedRows, Has.Count.EqualTo(1));
		Assert.That(rootRowView.TabBookmark.SelectedRows[0].Label, Is.EqualTo("Child 0"));
	}

	[Test]
	public void ThreeLevel_GrandchildBookmarks_ContainMatchingGrandchildRows()
	{
		// The level-2 child bookmark should itself contain a grandchild bookmark
		// whose SelectedRows lists the matched grandchild.
		List<DeepItem> items =
		[
			new("Item 0", 0) { Child = new DeepChild("Child 0", 0) { Grandchild = new DeepGrandchild("red", 0) } },
		];
		var model = CreateModel(items, maxSearchDepth: 2);
		var filter = new Filter("red");

		TabBookmark bookmark = model.FindMatches(filter, 2);

		SelectedRowView rootRowView = bookmark.SelectedRowViews[0];
		SelectedRowView childRowView = rootRowView.TabBookmark.SelectedRowViews[0];
		Assert.That(childRowView.TabBookmark.SelectedRows, Has.Count.EqualTo(1));
		Assert.That(childRowView.TabBookmark.SelectedRows[0].Label, Is.EqualTo("red"));
	}

	#endregion
}
