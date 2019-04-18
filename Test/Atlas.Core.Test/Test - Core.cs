using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Atlas.Core;
using Atlas.Extensions;
using NUnit.Framework;

namespace Atlas.Core.Test
{
	[Category("Core")]
	public class TestCore : TestBase
	{
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("Core");
		}

		[Test, Description("DecimalToString")]
		public void DecimalToString()
		{
			decimal d = 123456.1234M;
			string text = d.ObjectToString();

			Assert.AreEqual("123,456.123", text);
		}
	}
}