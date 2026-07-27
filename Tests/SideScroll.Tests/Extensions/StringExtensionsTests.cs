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

	[Test, SetCulture("tr-TR"), Description(
		"Casing is invariant, tr-TR uppercases 'i' to 'İ' (U+0130) and lowercases 'I' to 'ı' (U+0131)")]
	public void CamelCasedIsCultureInvariant()
	{
		Assert.That("item".CamelCased(), Is.EqualTo("Item"), "Would be 'İtem' with culture casing.");
		Assert.That("ITEM".CamelCased(), Is.EqualTo("Item"), "Would be 'Item' via the dotless 'ı'.");
		Assert.That("Windows".CamelCased(), Is.EqualTo("Windows"));
	}

	[Test]
	public void Range_MaximumEnd_ReturnsThroughEndOfString()
	{
		Assert.That("abcdef".Range(2, int.MaxValue), Is.EqualTo("cdef"));
	}
}
