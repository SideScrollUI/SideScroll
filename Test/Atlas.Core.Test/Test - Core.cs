﻿using Atlas.Extensions;
using NUnit.Framework;
using System;

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
			string text = d.Formatted();

			Assert.AreEqual("123,456.1234", text);
		}

		[Test, Description("WordSpaced")]
		public void WordSpaced()
		{
			string text = "CPUUtilization".WordSpaced();

			Assert.AreEqual("CPU Utilization", text);
		}

		[Test]
		public void TestBookmarkUri()
		{
			BookmarkUri uri = BookmarkUri.Parse("atlas://type/v3.1/id");

			Assert.AreEqual("atlas", uri.Prefix);
			Assert.AreEqual("type", uri.Type);
			Assert.AreEqual(new Version(3, 1, 0, 0), uri.Version);
			Assert.AreEqual("id", uri.Id);
		}
	}
}