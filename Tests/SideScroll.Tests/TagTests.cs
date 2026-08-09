using NUnit.Framework;

namespace SideScroll.Tests;

[Category("Core")]
[NonParallelizable] // MaxValueLength is static
public class TagTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Tag");
	}

	[TestCase(-1)]
	[TestCase(int.MinValue)]
	[Description(
		"A negative maximum reaches Formatted() while a tag is being rendered, which throws for it. " +
		"Rejecting it at the assignment names the setting instead of failing later somewhere else")]
	public void RejectsNegativeMaxValueLength(int value)
	{
		int original = Tag.MaxValueLength;
		try
		{
			ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
				() => Tag.MaxValueLength = value)!;

			Assert.That(exception.ParamName, Is.EqualTo(nameof(Tag.MaxValueLength)));
			Assert.That(Tag.MaxValueLength, Is.EqualTo(original), "The rejected value isn't stored.");
		}
		finally
		{
			Tag.MaxValueLength = original;
		}
	}

	[Test, Description("Control: zero truncates to an empty value rather than being invalid")]
	public void AllowsZeroMaxValueLength()
	{
		int original = Tag.MaxValueLength;
		try
		{
			Tag.MaxValueLength = 0;

			Assert.That(Tag.MaxValueLength, Is.Zero);
			Assert.That(new Tag("Name", "a long value").ToString(), Does.Not.Contain("long"));
		}
		finally
		{
			Tag.MaxValueLength = original;
		}
	}

	[Test, Description("Control: an ordinary maximum still formats the value")]
	public void FormatsWithinMaxValueLength()
	{
		Assert.That(new Tag("Name", "value").ToString(), Does.Contain("value"));
	}
}
