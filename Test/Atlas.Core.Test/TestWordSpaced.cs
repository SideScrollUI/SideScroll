using Atlas.Extensions;
using NUnit.Framework;
using System;

namespace Atlas.Core.Test
{
	[Category("Core")]
	public class TestWordSpaced : TestBase
	{
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("WordSpaced");
		}

		[Test, Description("WordSpaced")]
		public void WordSpaced()
		{
			string text = "CPUUtilization".WordSpaced();

			Assert.AreEqual("CPU Utilization", text);
		}

		[Test, Description("WordSpaced 5XX")]
		public void WordSpaced5xx()
		{
			string text = "Http5XXErrors".WordSpaced();

			Assert.AreEqual("Http 5XX Errors", text); // 5xx would be better though?
		}

		[Test, Description("WordSpaced 5XXs")]
		public void WordSpaced5xxs()
		{
			string text = "Http5XXsErrors".WordSpaced();

			Assert.AreEqual("Http 5XXs Errors", text);
		}

		[Test, Description("WordSpaced 2APIs")]
		public void WordSpaced2Apis()
		{
			string text = "2APIs".WordSpaced();

			Assert.AreEqual("2 APIs", text);
		}

		[Test, Description("WordSpaced P99.9")]
		public void WordSpacedP99()
		{
			string input = "P99.9";
			string text = input.WordSpaced();

			Assert.AreEqual(input, text);
		}
	}
}