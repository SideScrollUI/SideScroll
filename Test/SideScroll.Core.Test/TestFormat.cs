using SideScroll.Extensions;
using NUnit.Framework;

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
		Assert.AreEqual("1.5", TimeSpan.FromSeconds(1.5).FormattedShort());
		Assert.AreEqual("1:23", new TimeSpan(0, 0, 1, 23).FormattedShort());
		Assert.AreEqual("1:02:03", new TimeSpan(0, 1, 2, 3).FormattedShort());
		Assert.AreEqual("1:2:03:04", new TimeSpan(1, 2, 3, 4).FormattedShort());
		Assert.AreEqual("1:2:03:04.125", new TimeSpan(1, 2, 3, 4, 125).FormattedShort());
		Assert.AreEqual("1:2:03:04.005", new TimeSpan(1, 2, 3, 4, 5).FormattedShort());
	}
}
