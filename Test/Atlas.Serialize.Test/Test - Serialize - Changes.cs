using System;
using System.Collections.Generic;
using System.IO;
using Atlas.Core;
using NUnit.Framework;

namespace Atlas.Serialize.Test
{
	[Category("Serialize")]
	public class SerializeChanges : TestSerializeBase
	{
		private SerializerFile serializerFile;
		private Log log;
		
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("Serialize");
			log = call.log;

			string basePath = Paths.Combine(TestPath, "Serialize");

			Directory.CreateDirectory(basePath);

			string filePath = Paths.Combine(basePath, "Data.atlas");
			serializerFile = new SerializerFile(filePath);
		}

		[Test, Description("Serialize Property Type Missing")]
		public void SerializePropertyTypeMissing()
		{
			Parent testLog = new Parent();
			serializerFile.Save(call, testLog);
			Parent output = serializerFile.Load<Parent>(call);
		}

		public class Parent
		{
			public Child child;
		}

		public class Child
		{
			public int NumColumns;
		}
		
	}
}
