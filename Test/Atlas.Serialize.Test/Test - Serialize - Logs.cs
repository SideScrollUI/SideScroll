using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Atlas.Core;
using NUnit.Framework;

namespace Atlas.Serialize.Test
{
	[Category("Serialize")]
	public class SerializeLogs : TestSerializeBase
	{
		private SerializerFile interFace;
		private Log log;
		
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("Serialize");
			log = call.log;

			string basePath = Paths.Combine(TestPath, "Serialize");

			Directory.CreateDirectory(basePath);

			string filePath = Paths.Combine(basePath, "Data.atlas");
			interFace = new SerializerFile(filePath);
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
			TestLogBig testLog = new TestLogBig();
			testLog.Child("test");
			interFace.Save(call, testLog);
			TestLogBig output = interFace.Load<TestLogBig>(call);
		}

		[Test, Description("Serialize Test Log")]
		public void SerializeTestLog()
		{
			TestLog testLog = new TestLog();
			interFace.Save(call, testLog);
			TestLog output = interFace.Load<TestLog>(call);
		}

		[Test, Description("Serialize Log Timer 2")]
		public void SerializeLogTimer2()
		{
			Log testLog = new Log();
			using (testLog.Timer("timing"))
				testLog.Add("child");
			interFace.Save(call, testLog);
			Log output = interFace.Load<Log>(call);
		}

		[Test, Description("Serialize Log Entry")]
		public void SerializeLogEntry()
		{
			LogEntry input = new LogEntry();
			interFace.Save(call, input);
			LogEntry output = interFace.Load<LogEntry>(call);
		}

		[Test, Description("Serialize Log")]
		public void SerializeLog()
		{
			Log testLog = new Log();
			interFace.Save(call, testLog);
			Log output = interFace.Load<Log>(call);
		}

		[Test, Description("Serialize Log Unknown")]
		public void SerializeLogUnknown()
		{
			LogUnknown testLog = new LogUnknown();
			interFace.Save(call, testLog);
			LogUnknown output = interFace.Load<LogUnknown>(call);
		}

		[Test, Description("Serialize Log Child")]
		public void SerializeLogChild()
		{
			Log testLog = new Log();
			testLog.Call("test");

			interFace.Save(call, testLog);
			Log output = interFace.Load<Log>(call);
		}

		[Test, Description("Serialize Log Timer")]
		public void SerializeLogTimer()
		{
			LogTimer testLog = new LogTimer();

			interFace.Save(call, testLog);
			LogTimer output = interFace.Load<LogTimer>(call);
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
			testLog.tags = new Tag[] { new Tag("abc", 123) };

			interFace.Save(call, testLog);
			LogEntryTest2 output = interFace.Load<LogEntryTest2>(call);
		}

		[Test, Description("Serialize Log Timer Child Unknown")]
		public void SerializeLogTimerChildUnknown()
		{
			LogTest2 testLog = new LogTest2();
			testLog.Add(new Tag("abc", 123));

			interFace.Save(call, testLog);
			LogTest2 output = interFace.Load<LogTest2>(call);
		}

		[Test, Description("Serialize Log Timer Child")]
		public void SerializeLogTimerChild()
		{
			Log testLog = new Log();
			using (testLog.Timer("test")) { }

			interFace.Save(call, testLog);
			Log output = interFace.Load<Log>(call);
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
			public Tag[] tags;

			public LogEntryTest2()
			{
			}

			public LogEntryTest2(Tag[] tags)
			{
				this.tags = tags;
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
/*
	
*/
