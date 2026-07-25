using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Tests.Extensions;

[Category("Core")]
public class StringExtensionsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	[Test]
	public void CamelCased()
	{
		Assert.That("".CamelCased(), Is.EqualTo(""));
		Assert.That("a".CamelCased(), Is.EqualTo("A"));
		Assert.That("hello".CamelCased(), Is.EqualTo("Hello"));
		Assert.That("HELLO world".CamelCased(), Is.EqualTo("Hello world"));
	}
}
