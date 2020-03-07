using System;
using System.Collections.Generic;
using System.IO;
using Atlas.Core;
using NUnit.Framework;

namespace Atlas.Serialize.Test
{
	[Category("SerializeLazy")]
	public class SerializeLazy : TestSerializeBase
	{
		private SerializerFile serializerFile;
		private Log log;
		
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("SerializeLazy");
			log = call.log;

			string basePath = Paths.Combine(TestPath, "SerializeLazy");

			Directory.CreateDirectory(basePath);

			string filePath = Paths.Combine(basePath, "Data.atlas");
			serializerFile = new SerializerFile(filePath);
		}

		[Test, Description("Serialize Lazy Base")]
		public void SerializeLazyBase()
		{
			Parent input = new Parent();
			input.child = new Child();
			input.child.uintTest = 2;

			serializerFile.Save(call, input);
			Parent output = serializerFile.Load<Parent>(call, true);

			Assert.AreEqual(output.child.uintTest, input.child.uintTest);
		}

		[Test, Description("Serialize Lazy Null Properties")]
		public void SerializeLazyNullProperties()
		{
			Parent input = new Parent();

			serializerFile.Save(call, input);
			Parent output = serializerFile.Load<Parent>(call, true);

			Assert.AreEqual(output.child, input.child);
		}

		[Test, Description("Serialize Lazy Write Then Read")]
		public void SerializeLazyWriteThenRead()
		{
			WriteRead input = new WriteRead();

			serializerFile.Save(call, input);
			WriteRead output = serializerFile.Load<WriteRead>(call, true);
			output.stringTest = "abc";
			string temp = output.stringTest;

			Assert.AreEqual(output.stringTest, "abc");
		}

		[Test, Description("Serialize Lazy Constructor")]
		public void SerializeLazyConstructor()
		{
			Container input = new Container();

			serializerFile.Save(call, input);
			Container output = serializerFile.Load<Container>(call, true);

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
