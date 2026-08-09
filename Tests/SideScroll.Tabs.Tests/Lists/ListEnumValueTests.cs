using NUnit.Framework;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Tests;

public class ListEnumValueTests
{
	[Flags]
	private enum ULongEnum : ulong
	{
		None = 0,
		Max = ulong.MaxValue
	}

	[Test]
	public void Create_HandlesULongWithoutOverflow()
	{
		// Should not throw OverflowException
		var list = ListEnumValue.Create(ULongEnum.Max);

		Assert.That(list, Has.Count.EqualTo(2));
		
		var maxItem = list.Find(i => i.Name == "Max")!;
		Assert.That(maxItem, Is.Not.Null);
		Assert.That(maxItem.Selected, Is.True);
		Assert.That(maxItem.Hex, Is.EqualTo("FFFFFFFFFFFFFFFF"));
		Assert.That(maxItem.Value, Is.EqualTo(ulong.MaxValue));
	}

	[Flags]
	private enum Access
	{
		None = 0,
		Read = 1,
		Write = 2,
	}

	private static bool IsSelected(List<ListEnumValue> values, string name) =>
		values.Find(v => v.Name == name)!.Selected;

	[Test, Description("HasFlag(0) is always true, so a None = 0 row showed as selected alongside the real flag")]
	public void Create_ZeroFlagIsOnlySelectedForZero()
	{
		List<ListEnumValue> values = ListEnumValue.Create(Access.Read);

		Assert.That(IsSelected(values, nameof(Access.None)), Is.False);
		Assert.That(IsSelected(values, nameof(Access.Read)), Is.True);
		Assert.That(IsSelected(values, nameof(Access.Write)), Is.False);
	}

	[Test, Description("Control: a zero value still selects the zero flag and nothing else")]
	public void Create_ZeroValueSelectsTheZeroFlag()
	{
		List<ListEnumValue> values = ListEnumValue.Create(Access.None);

		Assert.That(IsSelected(values, nameof(Access.None)), Is.True);
		Assert.That(IsSelected(values, nameof(Access.Read)), Is.False);
	}

	[Test, Description("Control: combined flags still both select")]
	public void Create_CombinedFlagsAreBothSelected()
	{
		List<ListEnumValue> values = ListEnumValue.Create(Access.Read | Access.Write);

		Assert.That(IsSelected(values, nameof(Access.None)), Is.False);
		Assert.That(IsSelected(values, nameof(Access.Read)), Is.True);
		Assert.That(IsSelected(values, nameof(Access.Write)), Is.True);
	}
}
