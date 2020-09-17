using Atlas.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Atlas.Serialize.Test
{
	[Category("Serialize")]
	public class TestSerializeLogs : TestSerializeBase
	{
		private SerializerMemory serializer;
		
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("Serialize");
		}

		[SetUp]
		public void Setup()
		{
			serializer = new SerializerMemoryAtlas();
		}

		class TestLog
		{
			public enum LogType
			{
				Debug,
				Tab,
				Call,
				Info,
				Warn,
				Error,
				Alert
			}
			//public event PropertyChangedEventHandler PropertyChanged;
			//private Settings settings = new Settings();
			// Change everything to tags? const for created/message/childLog? harder to use then
			public DateTime Created { get; set; } = DateTime.Now;
			public LogType Type { get; set; }
			public string Text = "Log";// { get; set; }
			public int Entries { get; set; }
		}

		[Test, Description("Serialize Test Log Big")]
		public void SerializeTestLogBig()
		{
			var testLog = new TestLogBig();
			testLog.Child("test");
			serializer.Save(Call, testLog);
			TestLogBig output = serializer.Load<TestLogBig>(Call);
		}

		[Test, Description("Serialize Test Log")]
		public void SerializeTestLog()
		{
			TestLog testLog = new TestLog();
			serializer.Save(Call, testLog);
			TestLog output = serializer.Load<TestLog>(Call);
		}

		[Test, Description("Serialize Log Timer 2")]
		public void SerializeLogTimer2()
		{
			Log testLog = new Log();
			using (testLog.Timer("timing"))
				testLog.Add("child");
			serializer.Save(Call, testLog);
			Log output = serializer.Load<Log>(Call);
		}

		[Test, Description("Serialize Log Entry")]
		public void SerializeLogEntry()
		{
			LogEntry input = new LogEntry();
			serializer.Save(Call, input);
			LogEntry output = serializer.Load<LogEntry>(Call);
		}

		[Test, Description("Serialize Log")]
		public void SerializeLog()
		{
			Log testLog = new Log();
			serializer.Save(Call, testLog);
			Log output = serializer.Load<Log>(Call);
		}

		[Test, Description("Serialize Log Unknown")]
		public void SerializeLogUnknown()
		{
			LogUnknown testLog = new LogUnknown();
			serializer.Save(Call, testLog);
			LogUnknown output = serializer.Load<LogUnknown>(Call);
		}

		[Test, Description("Serialize Log Child")]
		public void SerializeLogChild()
		{
			Log testLog = new Log();
			testLog.Call("test");

			serializer.Save(Call, testLog);
			Log output = serializer.Load<Log>(Call);
		}

		[Test, Description("Serialize Log Timer")]
		public void SerializeLogTimer()
		{
			LogTimer testLog = new LogTimer();

			serializer.Save(Call, testLog);
			LogTimer output = serializer.Load<LogTimer>(Call);
		}

		private class MultipleArrays
		{
			public int[] array1 = { 1, 2 };
			//public int[] array2 = { 3, 4 };
		}

		[Test, Description("Serialize Log Entry Tags")]
		public void SerializeLogEntryTags()
		{
			LogEntryTest2 testLog = new LogEntryTest2();
			testLog.Tags = new Tag[] { new Tag("abc", 123) };

			serializer.Save(Call, testLog);
			LogEntryTest2 output = serializer.Load<LogEntryTest2>(Call);
		}

		[Test, Description("Serialize Log Timer Child Unknown")]
		public void SerializeLogTimerChildUnknown()
		{
			var testLog = new LogTest2();
			testLog.Add(new Tag("abc", 123));

			serializer.Save(Call, testLog);
			LogTest2 output = serializer.Load<LogTest2>(Call);
		}

		[Test, Description("Serialize Log Timer Child")]
		public void SerializeLogTimerChild()
		{
			Log testLog = new Log();
			using (testLog.Timer("test")) { }

			serializer.Save(Call, testLog);
			Log output = serializer.Load<Log>(Call);
		}

		public class SelectedItem
		{
			public string label;
			public bool pinned;
		}

		public class TabInstanceConfiguration
		{
			public HashSet<SelectedItem> selected = new HashSet<SelectedItem>();
			public int? SplitterDistance;
			public int NumColumns;
		}

		public class LogEntryUnknown
		{
			public string Type { get; set; }
		}

		public class LogUnknown : LogEntryUnknown
		{
			public List<LogEntryUnknown> Items = new List<LogEntryUnknown>();
		}
		

		public class LogEntryTest2
		{
			public Tag[] Tags;

			public LogEntryTest2()
			{
			}

			public LogEntryTest2(Tag[] tags)
			{
				Tags = tags;
			}
		}

		public class LogTest2
		{
			public List<LogEntryTest2> Items = new List<LogEntryTest2>();

			public LogTest2()
			{
			}

			public void Add(params Tag[] tags)
			{
				LogEntryTest2 logEntry = new LogEntryTest2(tags);
				Items.Add(logEntry);
			}
		}
	}
}
