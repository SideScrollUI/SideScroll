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
}
