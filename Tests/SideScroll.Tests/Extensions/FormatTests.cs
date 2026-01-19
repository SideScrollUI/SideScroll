using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Tests.Extensions;

[Category("Formatting")]
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
	public void DoubleFormattedShortDecimal()
	{
		Assert.That(1.123.FormattedShortDecimal(), Is.EqualTo("1.123"));
		Assert.That(0.999_998.FormattedShortDecimal(6), Is.EqualTo("0.999998"));
		Assert.That(0.000_002.FormattedShortDecimal(6), Is.EqualTo("0.000002"));
		Assert.That(9_999.998.FormattedShortDecimal(6), Is.EqualTo("9.999998 K"));
	}
}
