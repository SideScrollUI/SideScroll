﻿using Atlas.Core;
using NUnit.Framework;
using System.IO;

namespace Atlas.Serialize.Test
{
	[Category("Serialize")]
	public class SerializeChanges : TestSerializeBase
	{
		private SerializerFile serializerFile;
		
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("Serialize");

			string basePath = Paths.Combine(TestPath, "Serialize");

			Directory.CreateDirectory(basePath);

			string filePath = Paths.Combine(basePath, "Data.atlas");
			serializerFile = new SerializerFile(filePath);
		}

		[Test, Description("Serialize Property Type Missing")]
		public void SerializePropertyTypeMissing()
		{
			Parent testLog = new Parent();
			serializerFile.Save(Call, testLog);
			Parent output = serializerFile.Load<Parent>(Call);
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
