using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Tests.Extensions;

[Category("Formatting")]
[SetCulture("en-US")] // Formatting assertions depend on '.' decimal and ',' group separators
public class FormatTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Format");
	}

	[Test]
	public void TimeSpanFormattedShort()
	{
		Assert.That(TimeSpan.FromSeconds(1.5).FormattedShort(), Is.EqualTo("1.5"));
		Assert.That(new TimeSpan(0, 0, 1, 23).FormattedShort(), Is.EqualTo("1:23"));
		Assert.That(new TimeSpan(0, 1, 2, 3).FormattedShort(), Is.EqualTo("1:02:03"));
		Assert.That(new TimeSpan(1, 2, 3, 4).FormattedShort(), Is.EqualTo("1:2:03:04"));
		Assert.That(new TimeSpan(1, 2, 3, 4, 125).FormattedShort(), Is.EqualTo("1:2:03:04.125"));
		Assert.That(new TimeSpan(1, 2, 3, 4, 5).FormattedShort(), Is.EqualTo("1:2:03:04.005"));
	}

	[Test]
	public void TimeSpanFormattedShortNegative()
	{
		Assert.That(TimeSpan.FromSeconds(-1.5).FormattedShort(), Is.EqualTo("-1.5"));
		Assert.That(TimeSpan.FromSeconds(-90).FormattedShort(), Is.EqualTo("-1:30"));
		Assert.That(new TimeSpan(0, -1, -2, -3).FormattedShort(), Is.EqualTo("-1:02:03"));
		Assert.That(new TimeSpan(-1, -2, -3, -4).FormattedShort(), Is.EqualTo("-1:2:03:04"));
	}

	[Test, Description(
		"MinValue has no positive counterpart, so Duration() overflows on it. Formatted() reaches " +
		"this for every TimeSpan shown in a row or cell, so it can't throw")]
	public void TimeSpanFormattedShortMinValue()
	{
		string? result = null;

		Assert.DoesNotThrow(() => result = TimeSpan.MinValue.FormattedShort());

		Assert.That(result, Does.StartWith("-"));
		Assert.That(TimeSpan.MinValue.Formatted(), Does.StartWith("-"));
	}

	[Test, Description("MinValue formats the same as the tick above it, which Duration() can handle")]
	public void TimeSpanFormattedShortMinValue_MatchesItsNeighbor()
	{
		var neighbor = new TimeSpan(TimeSpan.MinValue.Ticks + 1);

		Assert.That(TimeSpan.MinValue.FormattedShort(), Is.EqualTo(neighbor.FormattedShort()));
	}

	[Test, Description(
		"Casting TotalMinutes to int wrapped for durations past ~4,000 years, so the minutes segment " +
		"was silently dropped (MaxValue rendered as 10675199:2:5.477, missing its 48 minutes)")]
	public void TimeSpanFormattedShortMaxValue()
	{
		Assert.That(TimeSpan.MaxValue.FormattedShort(), Is.EqualTo("10675199:2:48:05.477"));
	}

	[Test]
	public void TimeSpanFormattedShortMinValue_IncludesEveryUnit()
	{
		Assert.That(TimeSpan.MinValue.FormattedShort(), Is.EqualTo("-10675199:2:48:05.477"));
	}

	[Test]
	public void DoubleFormattedShortDecimal()
	{
		Assert.That(1.123.FormattedShortDecimal(), Is.EqualTo("1.123"));
		Assert.That(0.999_998.FormattedShortDecimal(6), Is.EqualTo("0.999998"));
		Assert.That(0.000_002.FormattedShortDecimal(6), Is.EqualTo("0.000002"));
		Assert.That(9_999.998.FormattedShortDecimal(6), Is.EqualTo("9.999998 K"));
	}

	[Test]
	[Ignore("todo: fix")]
	public void DecimalToString()
	{
		decimal d = 123456.1234M;
		string text = d.Formatted()!;

		Assert.That(text, Is.EqualTo("123,456.1234"));
	}
}
