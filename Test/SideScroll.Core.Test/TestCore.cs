using SideScroll.Extensions;
using NUnit.Framework;

namespace SideScroll.Test;

[Category("Core")]
public class TestCore : TestBase
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	[Test, Description("DecimalToString")]
	[Ignore("todo: fix")]
	public void DecimalToString()
	{
		decimal d = 123456.1234M;
		string text = d.Formatted()!;

		Assert.AreEqual("123,456.1234", text);
	}
}
