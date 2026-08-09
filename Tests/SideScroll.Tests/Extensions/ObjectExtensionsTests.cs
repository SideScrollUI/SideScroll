using NUnit.Framework;
using SideScroll.Extensions;
using System.Collections;

namespace SideScroll.Tests.Extensions;

[Category("Core")]
public class ObjectExtensionsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ObjectExtensions");
	}

	private class ClassWithIndexer
	{
		public string this[int index] => "value";
		public string Name { get; set; } = "Test";
	}

	private class ClassWithWriteOnlyProperty
	{
		public string WriteOnly
		{
			set { }
		}
		public string Name { get; set; } = "Test";
	}

	[Test]
	public void ToUniqueString_WithIndexer_DoesNotThrow()
	{
		var obj = new ClassWithIndexer();
		string? result = obj.ToUniqueString();
		Assert.That(result, Is.EqualTo("Test"));
	}

	[Test]
	public void ToUniqueString_WithWriteOnlyProperty_DoesNotThrow()
	{
		var obj = new ClassWithWriteOnlyProperty();
		string? result = obj.ToUniqueString();
		Assert.That(result, Is.EqualTo("Test"));
	}

	[Test]
	public void ToUniqueString_DistinctDoublesRemainDistinct()
	{
		Assert.That(1.001.ToUniqueString(), Is.Not.EqualTo(1.002.ToUniqueString()));
	}

	[Test]
	[SetCulture("de-DE")]
	public void ToUniqueString_NumericValueIsCultureInvariant()
	{
		Assert.That(1.5.ToUniqueString(), Is.EqualTo("1.5"));
		Assert.That(1.5m.ToUniqueString(), Is.EqualTo("1.5"));
	}

	[Test]
	public void ToUniqueString_DefaultDictionaryEntry_ReturnsNull()
	{
		Assert.That(default(DictionaryEntry).ToUniqueString(), Is.Null);
	}

	[Test]
	public void Formatted_RejectsNegativeMaximumLength()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => "value".Formatted(-1));
		Assert.Throws<ArgumentOutOfRangeException>(() => 123.Formatted(-1));
	}

	[TestCase(-1)]
	[TestCase(int.MinValue)]
	[NonParallelizable] // DefaultMaxFormattedLength is static
	[Description(
		"Formatted() uses this whenever no length is passed, which is nearly every call, so a " +
		"negative one makes formatting throw everywhere instead of at the assignment")]
	public void RejectsNegativeDefaultMaxFormattedLength(int value)
	{
		int original = ObjectExtensions.DefaultMaxFormattedLength;
		try
		{
			ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
				() => ObjectExtensions.DefaultMaxFormattedLength = value)!;

			Assert.That(exception.ParamName, Is.EqualTo(nameof(ObjectExtensions.DefaultMaxFormattedLength)));
			Assert.That(ObjectExtensions.DefaultMaxFormattedLength, Is.EqualTo(original), "The rejected value isn't stored.");
			Assert.That("still formats".Formatted(), Is.EqualTo("still formats"));
		}
		finally
		{
			ObjectExtensions.DefaultMaxFormattedLength = original;
		}
	}

	[Test, NonParallelizable, Description("Control: zero truncates to an empty string rather than being invalid")]
	public void AllowsZeroDefaultMaxFormattedLength()
	{
		int original = ObjectExtensions.DefaultMaxFormattedLength;
		try
		{
			ObjectExtensions.DefaultMaxFormattedLength = 0;

			Assert.That("value".Formatted(), Is.Empty);
		}
		finally
		{
			ObjectExtensions.DefaultMaxFormattedLength = original;
		}
	}

	private class ThrowingGetters
	{
		public string Throws => throw new InvalidOperationException("getter");
		public string Works => "identity";
	}

	private class OnlyThrowingGetters
	{
		public string Throws => throw new InvalidOperationException("getter");
	}

	[Test, Description(
		"ObjectUtils.GetObjectId() builds on this and bookmarking and row identity build on that, " +
		"so one throwing member took down rendering for the whole row instead of skipping it")]
	public void ToUniqueString_SkipsThrowingMembers()
	{
		Assert.That(new ThrowingGetters().ToUniqueString(), Is.EqualTo("identity"));
	}

	[Test, Description("Nothing readable left means no identity, not a propagated exception")]
	public void ToUniqueString_AllMembersThrowing_ReturnsNull()
	{
		Assert.That(new OnlyThrowingGetters().ToUniqueString(), Is.Null);
	}

	private class ThrowingField
	{
#pragma warning disable CS0649 // assigned via reflection only
		public OnlyThrowingGetters? Nested;
#pragma warning restore CS0649
	}

	[Test, Description("Control: the field loop is guarded the same way as the property loop")]
	public void ToUniqueString_SkipsThrowingFields()
	{
		var item = new ThrowingField { Nested = new OnlyThrowingGetters() };

		Assert.That(item.ToUniqueString(), Is.Null);
	}
}
