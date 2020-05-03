using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Atlas.Core.Test
{
	[Category("Performance")]
	public class TestPerformance : TestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
		}

		[Test, Description("DictionaryTest")]
		public void DictionaryTest()
		{
			var items = new Dictionary<int, int>();

			for (int i = 0; i < 100000; i++)
			{
				items[i] = i;
			}

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			int count = items.Values.Count;

			stopwatch.Stop();
		}
	}
}
