using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Tests.Utilities;

[Category("Core")]
[SetCulture("en-US")] // Formatting assertions depend on '.' decimal and ',' group separators
public class ByteFormatterTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	[Test]
	public void Format_Positive()
	{
		Assert.That(ByteFormatter.Format(0), Is.EqualTo("0 bytes"));
		Assert.That(ByteFormatter.Format(1023), Is.EqualTo("1,023 bytes"));
		Assert.That(ByteFormatter.Format(1536), Is.EqualTo("1.5 KB"));
		Assert.That(ByteFormatter.Format(1536, 2), Is.EqualTo("1.50 KB"));
	}

	[Test]
	public void Format_Negative_KeepsDecimalPlaces()
	{
		Assert.That(ByteFormatter.Format(-1536), Is.EqualTo("-1.5 KB"));
		Assert.That(ByteFormatter.Format(-1536, 2), Is.EqualTo("-1.50 KB"));
	}

	[Test, Description(
		"Negating long.MinValue as a long overflows back onto itself, which used to recurse until " +
		"the stack overflowed and took the process down")]
	public void Format_LongMinValue()
	{
		Assert.That(ByteFormatter.Format(long.MinValue), Is.EqualTo("-8.0 EB"));
		Assert.That(ByteFormatter.Format(long.MinValue + 1), Is.EqualTo("-8.0 EB"));
		Assert.That(ByteFormatter.Format(long.MaxValue), Is.EqualTo("8.0 EB"));
	}
}
