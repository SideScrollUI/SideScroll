using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Tests.Extensions;

[Category("Core")]
public class TimeSpanExtensionsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TimeSpanExtensions");
	}

	[TestCase(0)]
	[TestCase(-1)]
	public void PeriodDurationRejectsNonPositivePeriodCount(int numPeriods)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			TimeSpan.FromHours(1).PeriodDuration(numPeriods));
	}

	[TestCase(0)]
	[TestCase(-1)]
	public void RoundingRejectsNonPositiveIntervals(long ticks)
	{
		TimeSpan value = TimeSpan.FromSeconds(1);

		Assert.Throws<ArgumentOutOfRangeException>(() => value.Trim(ticks));
		Assert.Throws<ArgumentOutOfRangeException>(() => value.Ceil(ticks));
	}
}
