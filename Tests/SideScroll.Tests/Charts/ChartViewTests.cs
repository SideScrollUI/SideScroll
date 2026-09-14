using NUnit.Framework;
using SideScroll.Charts;

namespace SideScroll.Tests.Charts;

[Category("Charts")]
public class ChartViewTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ChartView");
	}

	private sealed class Sample
	{
		public double X { get; set; }
		public double Y { get; set; }
		public string? Group { get; set; }
	}

	[Test]
	public void AddDimensionsReportsMissingProperty()
	{
		var chart = new ChartView();
		var samples = new List<Sample> { new() };

		ArgumentException exception = Assert.Throws<ArgumentException>(
			() => chart.AddDimensions(samples, nameof(Sample.X), nameof(Sample.Y), "Missing"))!;

		Assert.That(exception.ParamName, Is.EqualTo("dimensionPropertyNames"));
		Assert.That(exception.Message, Does.Contain("Missing"));
		Assert.That(exception.Message, Does.Contain(typeof(Sample).FullName));
	}

	[Test, Description(
		"Each dimension's list was created as an instance of the source list's own type, which an " +
		"array can't be, so an array source threw from the first value")]
	public void AddDimensionsAcceptsAnArraySource()
	{
		var chart = new ChartView();
		Sample[] samples =
		[
			new() { X = 1, Y = 10, Group = "a" },
			new() { X = 2, Y = 20, Group = "b" },
			new() { X = 3, Y = 30, Group = "a" },
		];

		chart.AddDimensions(samples, nameof(Sample.X), nameof(Sample.Y), nameof(Sample.Group));

		Assert.That(chart.Series.Select(series => series.Name), Is.EquivalentTo(new[] { "a", "b" }));
		Assert.That(chart.Series.Single(series => series.Name == "a").List, Has.Count.EqualTo(2));
	}
}
