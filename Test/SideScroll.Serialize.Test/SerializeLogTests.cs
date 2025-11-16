using NUnit.Framework;
using SideScroll.Logs;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Test;

[Category("Serialize")]
public class SerializeLogTests : SerializeBaseTest
{
	private SerializerMemory _serializer = new SerializerMemoryAtlas();

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
			Info,
			Warn,
			Error,
		}
		public DateTime Created { get; set; } = DateTime.Now;
		public LogType Type { get; set; }
		public string Text { get; set; } = "Log";
		public int Entries { get; set; }
	}

	[Test, Description("Serialize Sample Log Big")]
	public void SerializeSampleLogBig()
	{
		var input = new SampleLog();
		input.Child("test");
		_serializer.Save(Call, input);
		SampleLog output = _serializer.Load<SampleLog>(Call);
		Assert.That(output, Is.Not.Null);
	}

	[Test, Description("Serialize Sample Log")]
	public void SerializeSampleLog()
	{
		var input = new TestLog();
		_serializer.Save(Call, input);
		TestLog output = _serializer.Load<TestLog>(Call);
		Assert.That(output, Is.Not.Null);
	}

	[Test, Description("Serialize Log Timer 2")]
	public void SerializeLogTimer2()
	{
		var input = new Log();
		using (input.Timer("timing"))
		{
			input.Add("child");
		}
		_serializer.Save(Call, input);
		Log output = _serializer.Load<Log>(Call);
		Assert.That(output, Is.Not.Null);
	}

	[Test, Description("Serialize Log Entry")]
	public void SerializeLogEntry()
	{
		var input = new LogEntry();
		_serializer.Save(Call, input);
		LogEntry output = _serializer.Load<LogEntry>(Call);
		Assert.That(output, Is.Not.Null);
	}

	[Test, Description("Serialize Log")]
	public void SerializeLog()
	{
		var input = new Log();
		_serializer.Save(Call, input);
		Log output = _serializer.Load<Log>(Call);
		Assert.That(output, Is.Not.Null);
	}

	[Test, Description("Serialize Log Unknown")]
	public void SerializeLogUnknown()
	{
		var input = new LogUnknown();
		_serializer.Save(Call, input);
		LogUnknown output = _serializer.Load<LogUnknown>(Call);
		Assert.That(output, Is.Not.Null);
	}

	[Test, Description("Serialize Log Child")]
	public void SerializeLogChild()
	{
		var input = new Log();
		input.Call("test");

		_serializer.Save(Call, input);
		Log output = _serializer.Load<Log>(Call);
		Assert.That(output, Is.Not.Null);
	}

	[Test, Description("Serialize Log Timer")]
	public void SerializeLogTimer()
	{
		var input = new LogTimer();

		_serializer.Save(Call, input);
		LogTimer output = _serializer.Load<LogTimer>(Call);
		Assert.That(output, Is.Not.Null);
	}

	[Test, Description("Serialize Log Entry Tags")]
	public void SerializeLogEntryTags()
	{
		var input = new LogEntryTest2
		{
			Tags = [new Tag("abc", 123)]
		};

		_serializer.Save(Call, input);
		LogEntryTest2 output = _serializer.Load<LogEntryTest2>(Call);
		Assert.That(output, Is.Not.Null);
	}

	[Test, Description("Serialize Log Timer Child Unknown")]
	public void SerializeLogTimerChildUnknown()
	{
		var input = new LogTest2();
		input.Add(new Tag("abc", 123));

		_serializer.Save(Call, input);
		LogTest2 output = _serializer.Load<LogTest2>(Call);
		Assert.That(output, Is.Not.Null);
	}

	[Test, Description("Serialize Log Timer Child")]
	public void SerializeLogTimerChild()
	{
		var input = new Log();
		using (input.Timer("test")) { }

		_serializer.Save(Call, input);
		Log output = _serializer.Load<Log>(Call);
		Assert.That(output, Is.Not.Null);
	}

	public class LogEntryUnknown
	{
		public string? Type { get; set; }
	}

	public class LogUnknown : LogEntryUnknown
	{
		public List<LogEntryUnknown> Items = [];
	}

	public class LogEntryTest2
	{
		public Tag[]? Tags;

		public LogEntryTest2() { }

		public LogEntryTest2(Tag[] tags)
		{
			Tags = tags;
		}
	}

	public class LogTest2
	{
		public List<LogEntryTest2> Items = [];

		public void Add(params Tag[] tags)
		{
			var logEntry = new LogEntryTest2(tags);
			Items.Add(logEntry);
		}
	}
}
