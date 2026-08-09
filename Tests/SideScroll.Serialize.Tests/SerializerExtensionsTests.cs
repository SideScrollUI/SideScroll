using NUnit.Framework;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class SerializerExtensionsTests
{
	private sealed class IndexedObject
	{
		public string Value { get; set; } = string.Empty;
		public string this[int index]
		{
			get => $"{Value}:{index}";
			set => Value = value;
		}
	}

	[Test, Description("Indexers require arguments and are not ordinary properties to shallow-copy")]
	public void ShallowCloneSkipsIndexers()
	{
		var source = new IndexedObject { Value = "copied" };
		var destination = new IndexedObject { Value = "original" };

		Assert.DoesNotThrow(() => destination.ShallowClone(source));
		Assert.That(destination.Value, Is.EqualTo("copied"));
	}
}
