using NUnit.Framework;
using SideScroll.Tabs;
using System.Collections.Generic;
using System.Linq;

namespace SideScroll.Tabs.Tests;

public class TabDataColumnsTests
{
	private class TestData
	{
		public int B { get; set; }
		public int A { get; set; }
		public int D { get; set; }
		public int C { get; set; }
	}

	[Test]
	public void GetPropertyColumns_MaintainsRemainingOrder()
	{
		var columns = new TabDataColumns(new List<string> { "C" });
		
		var propertyColumns = columns.GetPropertyColumns(typeof(TestData));
		
		Assert.That(propertyColumns, Has.Count.EqualTo(4));
		
		// "C" should be first because of ColumnNameOrder
		Assert.That(propertyColumns[0].PropertyInfo.Name, Is.EqualTo("C"));
		
		// The rest should maintain their original reflection order (B, A, D)
		var remainingNames = propertyColumns.Skip(1).Select(p => p.PropertyInfo.Name).ToList();
		
		Assert.That(remainingNames, Is.EqualTo(new List<string> { "B", "A", "D" }));
	}
}
