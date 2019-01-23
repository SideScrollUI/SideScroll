using Atlas.Core;
using System;
using System.Collections.Generic;

namespace Atlas.Serialize.Test
{
	public class TestSerializeBase : TestBase
	{
		//public Project project;

		public new void Initialize(string name)
		{
			base.Initialize(name);
			//project = new Project(name);
			//project.settings = settings;
		}

		public string TestPath = @"D:\Atlas\Test\Serializer";
	}
}
