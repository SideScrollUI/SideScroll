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
}
