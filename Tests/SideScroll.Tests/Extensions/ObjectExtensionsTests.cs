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
	}
}
