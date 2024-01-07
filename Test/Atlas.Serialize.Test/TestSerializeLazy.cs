using Atlas.Core;
using NUnit.Framework;

namespace Atlas.Serialize.Test;

[Category("SerializeLazy")]
public class TestSerializeLazy : TestSerializeBase
{
	private SerializerFile? _serializerFile;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeLazy");

		string basePath = Paths.Combine(TestPath, "SerializeLazy");

		Directory.CreateDirectory(basePath);

		string filePath = Paths.Combine(basePath, "Data.atlas");
		_serializerFile = new SerializerFileAtlas(filePath);
	}

	[Test, Description("Serialize Lazy Base")]
	public void SerializeLazyBase()
	{
		var input = new Parent
		{
			Child = new Child
			{
				UintTest = 2,
			}
		};

		_serializerFile!.Save(Call, input);
		Parent output = _serializerFile.Load<Parent>(Call, true)!;

		Assert.AreEqual(input.Child!.UintTest, output.Child.UintTest);
	}

	[Test, Description("Serialize Lazy Null Properties")]
	public void SerializeLazyNullProperties()
	{
		var input = new Parent();

		_serializerFile!.Save(Call, input);
		Parent output = _serializerFile.Load<Parent>(Call, true)!;

		Assert.AreEqual(input.Child, output.Child);
	}

	[Test, Description("Serialize Lazy Write Then Read")]
	public void SerializeLazyWriteThenRead()
	{
		var input = new WriteRead();

		_serializerFile!.Save(Call, input);
		WriteRead output = _serializerFile.Load<WriteRead>(Call, true)!;
		output.StringTest = "abc";
		string temp = output.StringTest;

		Assert.AreEqual("abc", output.StringTest);
	}

	[Test, Description("Serialize Lazy Constructor")]
	[Ignore("Not Working")]
	public void SerializeLazyConstructor()
	{
		var input = new Container();

		_serializerFile!.Save(Call, input);
		Container output = _serializerFile.Load<Container>(Call, true)!;

		Assert.NotNull(output.Id);
	}

	public class Container
	{
		public virtual string Id { get; set; } = "5";
		public string Result { get; set; }

		public Container()
		{
			Result = Id;
		}
	}

	public class Parent
	{
		public virtual Child? Child { get; set; } //= new Child();
	}

	public class Child
	{
		public uint UintTest { get; set; } = 1;
		public double DoubleTest { get; set; } = 2.3;
		public string StringTest { get; set; } = "mystring";
	}

	public class WriteRead
	{
		public virtual string StringTest { get; set; } = "mystring";
	}
}
