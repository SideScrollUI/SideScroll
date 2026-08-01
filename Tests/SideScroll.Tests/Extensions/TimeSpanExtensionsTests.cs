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

	// ─── Ceil ────────────────────────────────────────────────────────────

	[Test, Description(
		"The last interval before MaxValue has nothing above it to round up to. Adding the interval " +
		"anyway wrapped the unchecked long into a large negative duration. DateTime.Ceil() saturates too")]
	public void CeilSaturatesNearMaxValue()
	{
		Assert.That(TimeSpan.MaxValue.Ceil(TimeSpan.TicksPerSecond), Is.EqualTo(TimeSpan.MaxValue));
		Assert.That(TimeSpan.MaxValue.Ceil(TimeSpan.TicksPerDay), Is.EqualTo(TimeSpan.MaxValue));

		// One tick short of a whole second, so it can't round up without passing MaxValue
		var nearMax = new TimeSpan(TimeSpan.MaxValue.Ticks - 1);
		Assert.That(nearMax.Ceil(TimeSpan.TicksPerSecond), Is.EqualTo(TimeSpan.MaxValue));
		Assert.That(nearMax.Ceil(TimeSpan.TicksPerSecond), Is.GreaterThan(TimeSpan.Zero), "Never wraps negative.");
	}

	[Test, Description("Values with room above them still round up normally")]
	public void CeilRoundsUpWithinRange()
	{
		Assert.That(TimeSpan.FromMilliseconds(1500).Ceil(TimeSpan.TicksPerSecond),
			Is.EqualTo(TimeSpan.FromSeconds(2)));
		Assert.That(TimeSpan.FromSeconds(2).Ceil(TimeSpan.TicksPerSecond),
			Is.EqualTo(TimeSpan.FromSeconds(2)), "An aligned value is unchanged.");
		Assert.That(TimeSpan.Zero.Ceil(TimeSpan.TicksPerSecond), Is.EqualTo(TimeSpan.Zero));
	}

	[Test, Description("Negative durations are already rounded up by truncation toward zero")]
	public void CeilRoundsNegativesTowardZero()
	{
		Assert.That(TimeSpan.FromMilliseconds(-1500).Ceil(TimeSpan.TicksPerSecond),
			Is.EqualTo(TimeSpan.FromSeconds(-1)));
		Assert.That(TimeSpan.MinValue.Ceil(TimeSpan.TicksPerSecond), Is.LessThan(TimeSpan.Zero),
			"MinValue rounds toward zero, so it can't overflow.");
	}
}
