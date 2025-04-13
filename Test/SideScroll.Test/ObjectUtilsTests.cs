using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Test;

[Category("Core")]
public class ObjectUtilsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ObjectUtils");
	}

	[Test]
	public void AreEqual()
	{
		Assert.That(ObjectUtils.AreEqual(1, 1u));
	}

	[Test]
	public void ArrayAreEqual()
	{
		Assert.That(ObjectUtils.AreEqual(
			new int[] { 0 },
			new int[] { 0 }
			));
	}

	[Test]
	public void ArrayAreNotEqual()
	{
		Assert.That(ObjectUtils.AreEqual(
			new int[] { 0 },
			new int[] { 1 }
			), Is.False);
	}

	[Test]
	public void SubArrayAreEqual()
	{
		Assert.That(ObjectUtils.AreEqual(
			new int[][] { [0, 0] },
			new int[][] { [0, 0] }
			));
	}

	[Test]
	public void SubArrayAreNotEqual()
	{
		Assert.That(ObjectUtils.AreEqual(
			new int[][] { [0, 0] },
			new int[][] { [0, 1] }
			), Is.False);
	}

	[Test]
	public void SubArrayAreNotEqualMaxDepth()
	{
		Assert.Throws<TaggedException>(() => ObjectUtils.AreEqual(
			new int[][] { [0, 0] },
			new int[][] { [0, 0] },
			1));
	}
}
