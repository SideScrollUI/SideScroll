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

	[Test, Description(
		"The guard was ThrowIfNullOrWhiteSpace, so searching for a space threw. Only an empty value " +
		"loops forever, IndexOf() keeps returning the same index and the step is zero")]
	public void AllIndexesOfFindsWhitespace()
	{
		Assert.That("a b c".AllIndexesOf(" "), Is.EqualTo(new[] { 1, 3 }));
		Assert.That("a\tb".AllIndexesOf("\t"), Is.EqualTo(new[] { 1 }));
		Assert.That("a  b  c".AllIndexesOf("  "), Is.EqualTo(new[] { 1, 4 }));

		Assert.That("a b c".AllIndexesOfYield(" "), Is.EqualTo(new[] { 1, 3 }));
	}

	[Test, Description("Control: an empty value is still rejected, it would loop forever")]
	public void AllIndexesOfRejectsAnEmptyValue()
	{
		Assert.Throws<ArgumentException>(() => "abc".AllIndexesOf(""));
		Assert.Throws<ArgumentNullException>(() => "abc".AllIndexesOf(null!));

		// The yielding overload validates on the first MoveNext(), not at the call
		Assert.Throws<ArgumentException>(() => "abc".AllIndexesOfYield("").ToList());
	}

	[Test, Description("Control: a normal value is unaffected, and matches don't overlap")]
	public void AllIndexesOfFindsText()
	{
		Assert.That("abcabc".AllIndexesOf("abc"), Is.EqualTo(new[] { 0, 3 }));
		Assert.That("aaaa".AllIndexesOf("aa"), Is.EqualTo(new[] { 0, 2 }));
		Assert.That("abc".AllIndexesOf("z"), Is.Empty);
	}

	[Test, Description("Control: an ordinary postfix is removed, and a missing one leaves the input alone")]
	public void TrimEndRemovesThePostfix()
	{
		Assert.That("LoadAsync".TrimEnd("Async"), Is.EqualTo("Load"));
		Assert.That("Load".TrimEnd("Async"), Is.EqualTo("Load"));
	}
}
