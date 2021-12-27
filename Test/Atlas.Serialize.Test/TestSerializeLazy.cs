using Atlas.Core;
using NUnit.Framework;
using System.IO;

namespace Atlas.Serialize.Test
{
	[Category("SerializeLazy")]
	public class TestSerializeLazy : TestSerializeBase
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
			var input = new Parent()
			{
				Child = new Child()
				{
					UintTest = 2,
				}
			};

			serializerFile.Save(Call, input);
			Parent output = serializerFile.Load<Parent>(Call, true);

			Assert.AreEqual(output.Child.UintTest, input.Child.UintTest);
		}

		[Test, Description("Serialize Lazy Null Properties")]
		public void SerializeLazyNullProperties()
		{
			var input = new Parent();

			serializerFile.Save(Call, input);
			Parent output = serializerFile.Load<Parent>(Call, true);

			Assert.AreEqual(output.Child, input.Child);
		}

		[Test, Description("Serialize Lazy Write Then Read")]
		public void SerializeLazyWriteThenRead()
		{
			var input = new WriteRead();

			serializerFile.Save(Call, input);
			WriteRead output = serializerFile.Load<WriteRead>(Call, true);
			output.StringTest = "abc";
			string temp = output.StringTest;

			Assert.AreEqual(output.StringTest, "abc");
		}

		[Test, Description("Serialize Lazy Constructor")]
		[Ignore("Not Working")]
		public void SerializeLazyConstructor()
		{
			var input = new Container();

			serializerFile.Save(Call, input);
			Container output = serializerFile.Load<Container>(Call, true);

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
			public virtual Child Child { get; set; } //= new Child();
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
}
