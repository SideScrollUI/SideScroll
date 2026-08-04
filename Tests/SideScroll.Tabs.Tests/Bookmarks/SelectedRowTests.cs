using NUnit.Framework;
using SideScroll.Serialize;
using SideScroll.Tabs.Bookmarks.Models;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class SelectedRowTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SelectedRow");
	}

	private static SelectedRow Row(int? rowIndex) => new()
	{
		Label = "Item",
		RowIndex = rowIndex,
	};

	[Test, Description(
		"A row without a RowIndex used to wildcard-equal an indexed one, so a HashSet dropped whichever " +
		"was added second. Set membership can't depend on insertion order.")]
	public void HashSet_MissingRowIndex_KeepsBothRowsInEitherOrder()
	{
		var missingFirst = new HashSet<SelectedRow> { Row(null), Row(5) };
		var indexedFirst = new HashSet<SelectedRow> { Row(5), Row(null) };

		Assert.That(missingFirst, Has.Count.EqualTo(2), "Adding the row without an index first.");
		Assert.That(indexedFirst, Has.Count.EqualTo(2), "Adding the indexed row first.");
	}

	[Test, Description("Two rows differing only by RowIndex are distinct")]
	public void HashSet_DifferentRowIndexes_AreDistinct()
	{
		var rows = new HashSet<SelectedRow> { Row(2), Row(5) };

		Assert.That(rows, Has.Count.EqualTo(2));
	}

	[Test, Description("Identical rows still collapse, so selections don't accumulate duplicates")]
	public void HashSet_IdenticalRows_Collapse()
	{
		var rows = new HashSet<SelectedRow> { Row(5), Row(5) };

		Assert.That(rows, Has.Count.EqualTo(1));
	}

	[Test, Description(
		"DataValue was compared by reference but hashed by value, so equal rows could disagree. " +
		"A deserialized row is always a different instance than the live one.")]
	public void Equals_DataValue_ComparedByValue()
	{
		var a = new SelectedRow { Label = "Item", DataValue = new string("abc".ToCharArray()) };
		var b = new SelectedRow { Label = "Item", DataValue = new string("abc".ToCharArray()) };

		Assert.That(a, Is.EqualTo(b), "Equal content should be equal even from different instances.");
		Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()), "Equal rows must hash the same.");
		Assert.That(new HashSet<SelectedRow> { a, b }, Has.Count.EqualTo(1));
	}

	[Test, Description(
		"Serializer.Clone() keys its clone map on the object, so an intransitive Equals aliased two " +
		"distinct rows into one instance and lost the RowIndex. This runs whenever a link is opened.")]
	public void DeepClone_DistinctRows_StayDistinct()
	{
		var rows = new List<SelectedRow> { Row(null), Row(5) };

		List<SelectedRow> clone = rows.DeepClone(Call)!;

		Assert.That(clone, Has.Count.EqualTo(2));
		Assert.That(ReferenceEquals(clone[0], clone[1]), Is.False,
			"Distinct rows must not clone to the same instance.");
		Assert.That(clone[0].RowIndex, Is.Null);
		Assert.That(clone[1].RowIndex, Is.EqualTo(5), "The RowIndex must survive the clone.");
	}

	[Test, Description(
		"Matches() keeps the wildcard that lookups depend on: TabItemCollection and TabDataGrid both " +
		"probe with a row built from an object, which never has a RowIndex.")]
	public void Matches_MissingRowIndex_IsAWildcard()
	{
		Assert.That(Row(null).Matches(Row(5)), Is.True, "A missing index on the left matches.");
		Assert.That(Row(5).Matches(Row(null)), Is.True, "A missing index on the right matches.");
		Assert.That(Row(5).Matches(Row(5)), Is.True);
	}

	[Test, Description("Matches() still disambiguates rows that both carry an index")]
	public void Matches_DifferentRowIndexes_DoNotMatch()
	{
		Assert.That(Row(2).Matches(Row(5)), Is.False);
	}

	[Test, Description("Matches() compares the rest of the identity, not just the index")]
	public void Matches_DifferentLabels_DoNotMatch()
	{
		var other = new SelectedRow { Label = "Other", RowIndex = null };

		Assert.That(Row(null).Matches(other), Is.False);
	}

	// No [DataKey], so identical rows are indistinguishable except by their index
	public class UnkeyedItem(string name)
	{
		public string Name => name;

		public override string ToString() => name;
	}

	[Test, Description(
		"GetMatchingObject()'s RowIndex path builds a probe row with no RowIndex, so it only resolves " +
		"if the comparison wildcards it. Without it, duplicate rows fall through to the key lookup " +
		"and resolve to the wrong instance.")]
	public void GetMatchingObject_DuplicateRows_ResolvesThroughTheRowIndexPath()
	{
		var items = new List<UnkeyedItem> { new("dup"), new("dup") };
		TabItemCollection collection = new(items);

		var selectedRow = new SelectedRow(items[1])
		{
			Object = null, // Force the RowIndex path instead of the reference shortcut
			RowIndex = 1,
		};

		Assert.That(collection.GetMatchingObject(selectedRow), Is.SameAs(items[1]),
			"The second duplicate row should resolve to itself, not the first.");
	}
}
