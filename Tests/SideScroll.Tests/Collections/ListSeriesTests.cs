using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Collections;
using System.Collections;
using System.Reflection;

namespace SideScroll.Tests.Collections;

[Category("Collections")]
public class ListSeriesTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ListSeries");
	}

	public class Point
	{
		[XAxis]
		public DateTime Time { get; set; }

		[YAxis]
		public double Amount { get; set; }
	}

	private static readonly PropertyInfo XProperty = typeof(Point).GetProperty(nameof(Point.Time))!;
	private static readonly PropertyInfo YProperty = typeof(Point).GetProperty(nameof(Point.Amount))!;

	[Test, Description(
		"List is non-nullable, so returning early on a null one left it null behind a [MemberNotNull] " +
		"that suppressed every warning, and readers failed somewhere unrelated instead")]
	public void ConstructorsRejectANullList()
	{
		Assert.Throws<ArgumentNullException>(() => new ListSeries(null!));
		Assert.Throws<ArgumentNullException>(() => new ListSeries("name", null!));
		Assert.Throws<ArgumentNullException>(() => new ListSeries(null!, XProperty, YProperty));
		Assert.Throws<ArgumentNullException>(() => new ListSeries("name", null!, nameof(Point.Time)));
	}

	[Test, Description("Control: a real list still maps its axes from the attributes")]
	public void ConstructorLoadsTheAxisProperties()
	{
		IList list = new List<Point> { new() { Amount = 1 } };

		var series = new ListSeries(list);

		Assert.That(series.List, Is.SameAs(list));
		Assert.That(series.XPropertyInfo, Is.EqualTo(XProperty));
		Assert.That(series.YPropertyInfo, Is.EqualTo(YProperty));
	}
}
