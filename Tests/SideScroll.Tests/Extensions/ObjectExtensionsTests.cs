using NUnit.Framework;
using SideScroll.Extensions;

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
}
