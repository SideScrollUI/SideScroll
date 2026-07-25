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
}
