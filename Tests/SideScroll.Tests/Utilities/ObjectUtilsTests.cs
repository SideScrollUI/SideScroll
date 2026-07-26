using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Tests.Utilities;

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

	// ─── Same type ───────────────────────────────────────────────────────

	public class Plain
	{
		public string Name { get; set; } = "";
	}

	public enum Status { None, Active }
	public enum Other { None, Active }

	[Test, Description("Reference types compare with their own Equals rather than being converted")]
	public void AreEqual_ReferenceType_UsesEquals()
	{
		var plain = new Plain { Name = "a" };

		Assert.That(ObjectUtils.AreEqual(plain, plain), Is.True);
		Assert.That(ObjectUtils.AreEqual(new Plain { Name = "a" }, new Plain { Name = "a" }), Is.False,
			"Plain doesn't override Equals, so two instances aren't equal.");
	}

	[TestCase(true)]
	[TestCase(false)]
	public void AreEqual_Enum_ComparesByValue(bool equal)
	{
		Status other = equal ? Status.Active : Status.None;

		Assert.That(ObjectUtils.AreEqual(Status.Active, other), Is.EqualTo(equal));
	}

	[TestCase(true)]
	[TestCase(false)]
	public void AreEqual_Guid_ComparesByValue(bool equal)
	{
		var guid = Guid.NewGuid();
		Guid other = equal ? guid : Guid.NewGuid();

		Assert.That(ObjectUtils.AreEqual(guid, other), Is.EqualTo(equal));
	}

	// ─── Different types ─────────────────────────────────────────────────

	[Test, Description(
		"Convert.ChangeType throws for types it can't convert between. This runs while evaluating " +
		"[Hide] attributes, so an unconvertible pair has to compare unequal rather than break rendering")]
	public void AreEqual_UnconvertibleTypes_AreNotEqual()
	{
		Assert.That(ObjectUtils.AreEqual(Status.Active, "Active"), Is.False, "Enum to string.");
		Assert.That(ObjectUtils.AreEqual(Status.Active, Other.Active), Is.False, "Enum to a different enum.");
		Assert.That(ObjectUtils.AreEqual(Guid.Empty, Guid.Empty.ToString()), Is.False, "Guid to string.");
		Assert.That(ObjectUtils.AreEqual(new Plain(), "a"), Is.False, "Reference type to string.");
	}

	[Test, Description("A value that converts but doesn't parse isn't equal either")]
	public void AreEqual_UnparseableValue_IsNotEqual()
	{
		Assert.That(ObjectUtils.AreEqual(5, "abc"), Is.False);
	}

	[Test, Description("A value too large for the target type isn't equal either")]
	public void AreEqual_OverflowingValue_IsNotEqual()
	{
		Assert.That(ObjectUtils.AreEqual((byte)1, 300), Is.False);
	}

	[Test, Description("Cross type comparisons that do convert still work")]
	public void AreEqual_ConvertibleTypes_StillCompare()
	{
		Assert.That(ObjectUtils.AreEqual(5, "5"), Is.True);
		Assert.That(ObjectUtils.AreEqual("5", 5), Is.True);
		Assert.That(ObjectUtils.AreEqual(1, 1u), Is.True);
		Assert.That(ObjectUtils.AreEqual(5, "6"), Is.False);
	}
}
