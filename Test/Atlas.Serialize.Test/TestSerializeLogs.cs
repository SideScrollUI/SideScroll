using Atlas.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Atlas.Serialize.Test;

[Category("Serialize")]
public class TestSerializeLogs : TestSerializeBase
{
	private SerializerMemory _serializer;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Serialize");
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemoryAtlas();
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
		var input = new TestLogBig();
		input.Child("test");
		_serializer.Save(Call, input);
		TestLogBig output = _serializer.Load<TestLogBig>(Call);
	}

	[Test, Description("Serialize Test Log")]
	public void SerializeTestLog()
	{
		var input = new TestLog();
		_serializer.Save(Call, input);
		TestLog output = _serializer.Load<TestLog>(Call);
	}

	[Test, Description("Serialize Log Timer 2")]
	public void SerializeLogTimer2()
	{
		var input = new Log();
		using (input.Timer("timing"))
			input.Add("child");
		_serializer.Save(Call, input);
		Log output = _serializer.Load<Log>(Call);
	}

	[Test, Description("Serialize Log Entry")]
	public void SerializeLogEntry()
	{
		var input = new LogEntry();
		_serializer.Save(Call, input);
		LogEntry output = _serializer.Load<LogEntry>(Call);
	}

	[Test, Description("Serialize Log")]
	public void SerializeLog()
	{
		var input = new Log();
		_serializer.Save(Call, input);
		Log output = _serializer.Load<Log>(Call);
	}

	[Test, Description("Serialize Log Unknown")]
	public void SerializeLogUnknown()
	{
		var input = new LogUnknown();
		_serializer.Save(Call, input);
		LogUnknown output = _serializer.Load<LogUnknown>(Call);
	}

	[Test, Description("Serialize Log Child")]
	public void SerializeLogChild()
	{
		var input = new Log();
		input.Call("test");

		_serializer.Save(Call, input);
		Log output = _serializer.Load<Log>(Call);
	}

	[Test, Description("Serialize Log Timer")]
	public void SerializeLogTimer()
	{
		var input = new LogTimer();

		_serializer.Save(Call, input);
		LogTimer output = _serializer.Load<LogTimer>(Call);
	}

	private class MultipleArrays
	{
		public int[] array1 = { 1, 2 };
		//public int[] array2 = { 3, 4 };
	}

	[Test, Description("Serialize Log Entry Tags")]
	public void SerializeLogEntryTags()
	{
		var input = new LogEntryTest2
		{
			Tags = new Tag[] { new Tag("abc", 123) }
		};

		_serializer.Save(Call, input);
		LogEntryTest2 output = _serializer.Load<LogEntryTest2>(Call);
	}

	[Test, Description("Serialize Log Timer Child Unknown")]
	public void SerializeLogTimerChildUnknown()
	{
		var input = new LogTest2();
		input.Add(new Tag("abc", 123));

		_serializer.Save(Call, input);
		LogTest2 output = _serializer.Load<LogTest2>(Call);
	}

	[Test, Description("Serialize Log Timer Child")]
	public void SerializeLogTimerChild()
	{
		var input = new Log();
		using (input.Timer("test")) { }

		_serializer.Save(Call, input);
		Log output = _serializer.Load<Log>(Call);
	}

	public class SelectedItem
	{
		public string Label;
		public bool Pinned;
	}

	public class TabInstanceConfiguration
	{
		public HashSet<SelectedItem> Selected = new();
		public int? SplitterDistance;
		public int NumColumns;
	}

	public class LogEntryUnknown
	{
		public string Type { get; set; }
	}

	public class LogUnknown : LogEntryUnknown
	{
		public List<LogEntryUnknown> Items = new();
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
		public List<LogEntryTest2> Items = new();

		public LogTest2()
		{
		}

		public void Add(params Tag[] tags)
		{
			var logEntry = new LogEntryTest2(tags);
			Items.Add(logEntry);
		}
	}
}
