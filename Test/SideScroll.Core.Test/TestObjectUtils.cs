using SideScroll.Core.Utilities;
using NUnit.Framework;

namespace SideScroll.Core.Test;

[Category("Core")]
public class TestObjectUtils : TestBase
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ObjectUtils");
	}

	[Test]
	public void AreEqual()
	{
		Assert.IsTrue(ObjectUtils.AreEqual(1, 1u));
	}

	[Test]
	public void ArrayAreEqual()
	{
		Assert.IsTrue(ObjectUtils.AreEqual(
			new int[] { 0 }, 
			new int[] { 0 }
			));
	}

	[Test]
	public void ArrayAreNotEqual()
	{
		Assert.IsFalse(ObjectUtils.AreEqual(
			new int[] { 0 }, 
			new int[] { 1 }
			));
	}

	[Test]
	public void SubArrayAreEqual()
	{
		Assert.IsTrue(ObjectUtils.AreEqual(
			new int[][] { [0, 0] },
			new int[][] { [0, 0] }
			));
	}

	[Test]
	public void SubArrayAreNotEqual()
	{
		Assert.IsFalse(ObjectUtils.AreEqual(
			new int[][] { [0, 0] },
			new int[][] { [0, 1] }
			));
	}

	[Test]
	public void SubArrayAreNotEqualMaxDepth()
	{
		Assert.Throws<Exception>(() => ObjectUtils.AreEqual(
			new int[][] { [0, 0] },
			new int[][] { [0, 0] },
			1));
	}
}
