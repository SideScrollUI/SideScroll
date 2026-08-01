using NUnit.Framework;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class ListByteTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ListByte");
	}

	[Test, Description(
		"A negative limit threw while allocating the read buffer in Load(), and made Create() " +
		"return nothing without an error")]
	public void NegativeMaxBytesIsClampedToZero()
	{
		int original = ListByte.MaxBytes;
		try
		{
			ListByte.MaxBytes = -1;
			Assert.That(ListByte.MaxBytes, Is.Zero);

			Assert.DoesNotThrow(() => ListByte.Create([1, 2, 3]));
		}
		finally
		{
			ListByte.MaxBytes = original;
		}
	}

	[Test, Description("A positive limit still truncates to it")]
	public void MaxBytesLimitsCreatedItems()
	{
		int original = ListByte.MaxBytes;
		try
		{
			ListByte.MaxBytes = 2;

			Assert.That(ListByte.Create([1, 2, 3, 4]), Has.Count.EqualTo(2));
		}
		finally
		{
			ListByte.MaxBytes = original;
		}
	}
}
