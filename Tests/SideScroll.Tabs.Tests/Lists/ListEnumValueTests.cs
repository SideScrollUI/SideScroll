using NUnit.Framework;
using SideScroll.Tabs.Lists;
using System;

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
}
