using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Test;

[Category("Formatting")]
public class TestFormat : TestBase
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
}
