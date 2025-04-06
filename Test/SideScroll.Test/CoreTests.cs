using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Test;

[Category("Core")]
public class CoreTests : BaseTest
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

		Assert.That(text, Is.EqualTo("123,456.1234"));
	}
}
