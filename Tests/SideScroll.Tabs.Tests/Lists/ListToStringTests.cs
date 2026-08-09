using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Tests;

public class ListToStringTests
{
	private class ThrowingToString
	{
		public override string ToString() => throw new InvalidOperationException("ToString failed");
	}

	private class ThrowingDataKey
	{
		[DataKey]
		public string Key => throw new InvalidOperationException("DataKey failed");

		public override string ToString() => "Item";
	}

	private class ThrowingDataValue
	{
		[DataKey]
		public string Key => "Key";

		[DataValue]
		public object Value => throw new InvalidOperationException("DataValue failed");

		public override string ToString() => "Item";
	}

	[Test]
	public void Create_RespectsLimit()
	{
		var enumerable = new List<int> { 1, 2, 3, 4, 5 };
		
		// The limit is exactly 3. It should return 3 items, not 4.
		var list = ListToString.Create(enumerable, limit: 3);

		Assert.That(list, Has.Count.EqualTo(3));
		Assert.That(list[0].Value, Is.EqualTo("1"));
		Assert.That(list[1].Value, Is.EqualTo("2"));
		Assert.That(list[2].Value, Is.EqualTo("3"));
	}

	[TestCase(0)]
	[TestCase(-1)]
	[Description("The cap is checked before adding, so a limit of zero or less creates no items")]
	public void Create_ZeroOrNegativeLimit_CreatesNoItems(int limit)
	{
		var enumerable = new List<int> { 1, 2, 3 };

		var list = ListToString.Create(enumerable, limit);

		Assert.That(list, Is.Empty);
	}

	[Test, Description(
		"A throwing ToString() used to propagate out of Create() and fail the whole collection, " +
		"leaving the tab unrendered instead of just that row")]
	public void Create_ItemWithThrowingToString_KeepsRemainingItems()
	{
		object[] enumerable = ["before", new ThrowingToString(), "after"];

		var list = ListToString.Create(enumerable);

		Assert.That(list, Has.Count.EqualTo(3));
		Assert.That(list[0].Value, Is.EqualTo("before"));
		Assert.That(list[2].Value, Is.EqualTo("after"));
	}

	[Test, Description("The failure is shown in place of the text, an empty row wouldn't say why")]
	public void Create_ItemWithThrowingToString_ShowsTheException()
	{
		var list = ListToString.Create(new object[] { new ThrowingToString() });

		Assert.That(list[0].Value, Does.Contain("ToString failed"));
	}

	[Test, Description(
		"A [DataKey] getter is read through reflection, so it threw a TargetInvocationException " +
		"rather than the getter's own exception")]
	public void Create_ItemWithThrowingDataKey_KeepsRemainingItems()
	{
		object[] enumerable = ["before", new ThrowingDataKey(), "after"];

		var list = ListToString.Create(enumerable);

		Assert.That(list, Has.Count.EqualTo(3));
		Assert.That(list[1].Value, Is.EqualTo("Item"), "The text is unaffected by the key failing");
		Assert.That(list[1].DataKey, Is.Null);
		Assert.That(list[2].Value, Is.EqualTo("after"));
	}

	[Test, Description("A throwing [DataValue] doesn't take the key that was read before it")]
	public void Create_ItemWithThrowingDataValue_KeepsTheDataKey()
	{
		var list = ListToString.Create(new object[] { new ThrowingDataValue() });

		Assert.That(list[0].DataKey, Is.EqualTo("Key"));
		Assert.That(list[0].DataValue, Is.Null);
	}
}
