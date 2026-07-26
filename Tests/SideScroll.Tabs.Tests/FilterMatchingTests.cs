using NUnit.Framework;
using SideScroll.Attributes;
using System.Collections;
using System.Reflection;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class FilterMatchingTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("FilterMatching");
	}

	public class Row(string name)
	{
		public string Name => name;

		public override string ToString() => name;
	}

	// ─── Matches(IList) ──────────────────────────────────────────────────

	[Test, Description(
		"Matches(IList) searches the items. It used to pass the list itself to the single object " +
		"overload, so it matched on the list's type name instead of its contents.")]
	public void MatchesList_SearchesTheItems()
	{
		var list = new List<Row> { new("alpha"), new("beta") };

		Assert.That(new Filter("alpha").Matches(list), Is.True, "A matching item should be found.");
		Assert.That(new Filter("beta").Matches(list), Is.True);
		Assert.That(new Filter("gamma").Matches(list), Is.False, "A term matching no item shouldn't match.");
	}

	[Test, Description("The list's own type name isn't searchable content")]
	public void MatchesList_DoesNotMatchTheListTypeName()
	{
		var list = new List<Row> { new("alpha") };

		Assert.That(new Filter("Row").Matches(list), Is.False,
			"'Row' only appears in the element type's name, not in any item.");
		Assert.That(new Filter("List").Matches(list), Is.False);
	}

	[Test, Description(
		"Arrays have no generic arguments, so resolving the element type by index threw an " +
		"IndexOutOfRangeException")]
	public void MatchesList_Array_Matches()
	{
		Row[] array = [new("alpha"), new("beta")];

		Assert.That(new Filter("alpha").Matches(array), Is.True);
		Assert.That(new Filter("gamma").Matches(array), Is.False);
	}

	[Test, Description("A non-generic list still matches on the items' text")]
	public void MatchesList_UntypedList_MatchesOnText()
	{
		var list = new ArrayList { new Row("alpha") };

		Assert.That(new Filter("alpha").Matches(list), Is.True);
		Assert.That(new Filter("gamma").Matches(list), Is.False);
	}

	[Test]
	public void MatchesList_EmptyList_DoesNotMatch()
	{
		Assert.That(new Filter("alpha").Matches(new List<Row>()), Is.False);
	}

	// ─── SearchFilter ────────────────────────────────────────────────────

	[TestCaseSource(nameof(Scalars))]
	[Description(
		"TabModel.Create() returns null for values with nothing to show, which used to be " +
		"dereferenced with a null forgiving operator")]
	public void IsMatch_Scalar_DoesNotThrow(object value, string term, bool expected)
	{
		var searchFilter = new SearchFilter { Filter = new Filter(term) };

		Assert.That(searchFilter.IsMatch(value), Is.EqualTo(expected));
	}

	public static IEnumerable<TestCaseData> Scalars()
	{
		yield return new TestCaseData(new DateTime(2026, 7, 25), "2026", true).SetName("DateTime matching");
		yield return new TestCaseData(new DateTime(2026, 7, 25), "1999", false).SetName("DateTime not matching");
		yield return new TestCaseData(5, "5", true).SetName("Int matching");
		yield return new TestCaseData(5, "7", false).SetName("Int not matching");
		yield return new TestCaseData("text", "ex", true).SetName("String matching");
		yield return new TestCaseData("text", "zz", false).SetName("String not matching");
	}

	[Test]
	public void IsMatch_Object_StillMatchesOnProperties()
	{
		var searchFilter = new SearchFilter { Filter = new Filter("alpha") };

		Assert.That(searchFilter.IsMatch(new Row("alpha")), Is.True);
		Assert.That(searchFilter.IsMatch(new Row("beta")), Is.False);
	}

	[Test, Description("No filter configured matches everything")]
	public void IsMatch_NoFilter_Matches()
	{
		Assert.That(new SearchFilter().IsMatch(new Row("alpha")), Is.True);
		Assert.That(new SearchFilter { Filter = new Filter("") }.IsMatch(new Row("alpha")), Is.True);
	}

	[Test, Description("FindMatches dereferenced the model and the filter without checking either")]
	public void FindMatches_NoFilter_ReturnsEmpty()
	{
		var searchFilter = new SearchFilter();

		Assert.That(searchFilter.FindMatches(new List<Row> { new("alpha") }).SelectedRows, Is.Empty);
	}

	[Test]
	public void FindMatches_MatchingItem_IsSelected()
	{
		var searchFilter = new SearchFilter { Filter = new Filter("alpha") };

		var tabBookmark = searchFilter.FindMatches(new List<Row> { new("alpha"), new("beta") });

		Assert.That(tabBookmark.SelectedRows, Has.Count.EqualTo(1));
		Assert.That(tabBookmark.SelectedRows[0].Label, Is.EqualTo("alpha"));
	}

	// ─── Culture ─────────────────────────────────────────────────────────

	private static bool MatchesRow(string filterText, string value)
	{
		List<PropertyInfo> properties = TabDataColumns.GetVisibleProperties(typeof(Row));
		return new Filter(filterText).Matches(new Row(value), properties);
	}

	[TestCase("ibm", "IBM Corp")]
	[TestCase("IBM", "ibm corp")]
	[TestCase("corp", "IBM Corp")]
	[SetCulture("tr-TR")]
	[Description(
		"Search uppercases both sides then compares ordinally. Turkish uppercases 'i' to 'İ' but " +
		"leaves 'I' alone, so culture casing broke case insensitive search for any term with an i.")]
	public void Matches_IsCaseInsensitive_UnderTurkishCulture(string term, string value)
	{
		Assert.That(MatchesRow(term, value), Is.True, $"'{term}' should match '{value}'.");
	}

	[Test, SetCulture("tr-TR")]
	[Description(
		"The tradeoff of invariant casing: the Turkish dotted capital İ no longer folds to 'i' the " +
		"way it did under culture casing. Searching is predictable across locales instead, which " +
		"matters more for the mostly ASCII text these grids hold.")]
	public void Matches_TurkishDottedCapital_DoesNotFoldToAscii()
	{
		Assert.That(MatchesRow("İstanbul", "istanbul"), Is.False);
		Assert.That(MatchesRow("İstanbul", "İSTANBUL"), Is.True, "It still matches itself case insensitively.");
	}

	[TestCase("ibm", "IBM Corp")]
	[TestCase("IBM", "ibm corp")]
	[Description("The same searches under the default culture")]
	public void Matches_IsCaseInsensitive(string term, string value)
	{
		Assert.That(MatchesRow(term, value), Is.True);
	}

	[Test, SetCulture("tr-TR")]
	public void Matches_NonMatchingTerm_StillDoesNotMatch()
	{
		Assert.That(MatchesRow("zebra", "IBM Corp"), Is.False);
	}

	// ─── Search text limits ──────────────────────────────────────────────

	public class NestedRow(string name, List<Row>? items = null)
	{
		public string Name => name;

		[InnerValue]
		public List<Row> Items { get; } = items ?? [];

		public override string ToString() => name;
	}

	[Test, Description(
		"Inner lists are enumerated in full at every nesting level for every row on every keystroke, " +
		"so the collected text is capped. Rows past the cap stop contributing search text.")]
	public void Matches_LargeInnerList_StopsAtTheValueLimit()
	{
		int original = Filter.MaxSearchTextValues;
		try
		{
			Filter.MaxSearchTextValues = 4;

			var items = Enumerable.Range(0, 50)
				.Select(i => new Row($"item{i:D2}"))
				.ToList();
			var row = new NestedRow("parent", items);

			List<PropertyInfo> properties = TabDataColumns.GetVisibleProperties(typeof(NestedRow));

			Assert.That(new Filter("item00").Matches(row, properties), Is.True,
				"Text within the limit is still searchable.");
			Assert.That(new Filter("item49").Matches(row, properties), Is.False,
				"Text past the limit is not collected.");
		}
		finally
		{
			Filter.MaxSearchTextValues = original;
		}
	}

	[Test, Description("The default limit is generous enough that normal nested lists still search")]
	public void Matches_SmallInnerList_SearchesEveryItem()
	{
		var items = Enumerable.Range(0, 20)
			.Select(i => new Row($"item{i:D2}"))
			.ToList();
		var row = new NestedRow("parent", items);

		List<PropertyInfo> properties = TabDataColumns.GetVisibleProperties(typeof(NestedRow));

		Assert.That(new Filter("item00").Matches(row, properties), Is.True);
		Assert.That(new Filter("item19").Matches(row, properties), Is.True);
		Assert.That(new Filter("item20").Matches(row, properties), Is.False);
	}

	// ─── Depth prefix ────────────────────────────────────────────────────

	[TestCase("+3 abc", 3)]
	[TestCase("+0 abc", 0)]
	[Description("A valid depth prefix is applied")]
	public void Constructor_DepthPrefix_IsParsed(string filterText, int expected)
	{
		Assert.That(new Filter(filterText).Depth, Is.EqualTo(expected));
	}

	[Test, Description(
		"The regex accepts unbounded digits, and the constructor runs for every keystroke in the " +
		"search box, so an oversized depth can't be allowed to throw")]
	public void Constructor_OversizedDepthPrefix_DoesNotThrow()
	{
		Filter filter = null!;

		Assert.DoesNotThrow(() => filter = new Filter("+99999999999 abc", depth: 2));

		Assert.That(filter.Depth, Is.EqualTo(2), "Falls back to the depth passed in.");
		Assert.That(filter.RootNode, Is.Not.Null, "The search term is still parsed.");
	}
}
