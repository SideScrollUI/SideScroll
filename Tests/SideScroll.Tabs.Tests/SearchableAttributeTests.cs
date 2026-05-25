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
}
