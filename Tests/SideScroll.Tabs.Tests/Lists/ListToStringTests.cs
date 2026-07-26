using NUnit.Framework;
using SideScroll.Tabs.Lists;
using System.Collections.Generic;

namespace SideScroll.Tabs.Tests;

public class ListToStringTests
{
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
}
