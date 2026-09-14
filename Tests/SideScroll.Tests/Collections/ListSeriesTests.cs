using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Time;
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

	public class NullableTimePoint
	{
		[XAxis]
		public DateTime? Time { get; set; }

		[YAxis]
		public double Amount { get; set; }
	}

	[Test, Description(
		"The X property had to be exactly DateTime, so a DateTime? axis wasn't a time series: no " +
		"time window, no period grouping, and the chart drew it as non-temporal")]
	public void TimeRangeValues_NullableDateTimeXAxis_IsATimeSeries()
	{
		DateTime start = new(2026, 1, 1);
		var points = new List<NullableTimePoint>
		{
			new() { Time = start.AddMinutes(1), Amount = 2 },
			new() { Time = null, Amount = 5 },
			new() { Time = start, Amount = 1 },
		};
		var series = new ListSeries("Nullable", points) { PeriodDuration = TimeSpan.FromMinutes(1) };

		List<TimeRangeValue>? values = series.TimeRangeValues;

		Assert.That(values, Is.Not.Null);
		Assert.That(values!.Select(value => value.Value), Is.EqualTo(new[] { 1.0, 2.0 }), "ordered by time, the null skipped");
		Assert.That(series.GetTimeWindow(), Is.Not.Null);
	}
}
