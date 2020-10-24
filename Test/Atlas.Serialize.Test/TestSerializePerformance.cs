using Atlas.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Atlas.Serialize.Test
{
	[Category("Performance")]
	public class TestSerializePerformance : TestSerializeBase
	{
		private SerializerMemory serializer;

		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("TestSerializePerformance");
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			serializer = new SerializerMemoryAtlas();
		}

		[Test, Description("DictionaryTest")]
		public void DictionaryTest()
		{
			var items = new Dictionary<int, int>();

			for (int i = 0; i < 100000; i++)
			{
				items[i] = i;
			}

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			int count = items.Values.Count;

			stopwatch.Stop();
		}

		[Test]
		public void TimeRangeValue()
		{
			var input = new List<TimeRangeValue>();

			for (int i = 0; i < 100000; i++)
			{
				var timeRangeValue = new TimeRangeValue()
				{
					StartTime = new DateTime(1980, 10, 23),
					EndTime = new DateTime(2020, 10, 24),
				};
				input.Add(timeRangeValue);
			}

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			serializer.Save(Call, input);
			var output = serializer.Load<List<TimeRangeValue>>(Call);

			stopwatch.Stop();
		}
	}
}
