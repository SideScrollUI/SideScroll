using Atlas.Core;
using NUnit.Framework;
using System.IO;

namespace Atlas.Serialize.Test
{
	[Category("SerializeLazy")]
	public class SerializeLazy : TestSerializeBase
	{
		private SerializerFile serializerFile;
		
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("SerializeLazy");

			string basePath = Paths.Combine(TestPath, "SerializeLazy");

			Directory.CreateDirectory(basePath);

			string filePath = Paths.Combine(basePath, "Data.atlas");
			serializerFile = new SerializerFileAtlas(filePath);
		}

		[Test, Description("Serialize Lazy Base")]
		public void SerializeLazyBase()
		{
			Parent input = new Parent();
			input.child = new Child();
			input.child.uintTest = 2;

			serializerFile.Save(Call, input);
			Parent output = serializerFile.Load<Parent>(Call, true);

			Assert.AreEqual(output.child.uintTest, input.child.uintTest);
		}

		[Test, Description("Serialize Lazy Null Properties")]
		public void SerializeLazyNullProperties()
		{
			Parent input = new Parent();

			serializerFile.Save(Call, input);
			Parent output = serializerFile.Load<Parent>(Call, true);

			Assert.AreEqual(output.child, input.child);
		}

		[Test, Description("Serialize Lazy Write Then Read")]
		public void SerializeLazyWriteThenRead()
		{
			var input = new WriteRead();

			serializerFile.Save(Call, input);
			WriteRead output = serializerFile.Load<WriteRead>(Call, true);
			output.stringTest = "abc";
			string temp = output.stringTest;

			Assert.AreEqual(output.stringTest, "abc");
		}

		[Test, Description("Serialize Lazy Constructor")]
		public void SerializeLazyConstructor()
		{
			var input = new Container();

			serializerFile.Save(Call, input);
			Container output = serializerFile.Load<Container>(Call, true);

			Assert.NotNull(output.id);
		}

		public class Container
		{
			public virtual string id { get; set; } = "5";
			public string result { get; set; }

			public Container()
			{
				result = id;
			}
		}

		public class Parent
		{
			public virtual Child child { get; set; } //= new Child();
		}

		public class Child
		{
			public uint uintTest { get; set; } = 1;
			public double doubleTest { get; set; } = 2.3;
			public string stringTest { get; set; } = "mystring";
		}

		public class WriteRead
		{
			public virtual string stringTest { get; set; } = "mystring";
		}
	}
}
