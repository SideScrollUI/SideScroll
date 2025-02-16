using NUnit.Framework;
using SideScroll.Serialize.Json;
using System.Text.Json;

namespace SideScroll.Serialize.Test;

public class TestSerializeJson
{
	/*[Test, Description("Serialize Lazy Base")]
	public void SerializeJsonBase()
	{
		var input = new Parent();
		input.child = new Child();
		input.child.uintTest = 2;

		var serializer = new SerializerMemoryJson(
		serializerFile.Save(Call, input);
		Parent output = serializerFile.Load<Parent>(Call, true);

		Assert.AreEqual(input.child.uintTest, output.child.uintTest);
	}*/

	public class ChildClass
	{
		public int Number { get; set; }
	}

	public class ReadOnlyProperties
	{
		public string ReadWriteProperty { get; set; } = "ReadWriteProperty";

		public string ReadOnlyProperty => "ReadOnlyProperty";

		public List<string> ReadOnlyList => ["ReadOnlyList"];

		public ChildClass ChildClass => new();
	}

	[Test]
	public void TestReadOnlyProperties()
	{
		ReadOnlyProperties input = new();

		string output = JsonSerializer.Serialize(input, JsonConverters.PublicJsonSerializerOptions);

		Assert.That(output, Does.Contain("ReadWriteProperty"));
		Assert.That(output, Does.Not.Contain("ReadOnlyProperty"));
		Assert.That(output, Does.Not.Contain("ChildClass"));
		Assert.That(output, Does.Not.Contain("ReadOnlyList")); // Fails
	}
}
