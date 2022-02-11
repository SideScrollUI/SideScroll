using Atlas.Core.Time;
using NUnit.Framework;
using System;

namespace Atlas.Serialize.Test;

[Category("SerializeDateTime")]
public class SerializeDateTime : TestSerializeBase
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

	// DateTime has no set operators and relies on constructor
	[Test, Description("Serialize DateTime Local")]
	public void SerializeDateTimeLocal()
	{
		DateTime input = DateTime.Now;

		_serializer.Save(Call, input);
		DateTime output = _serializer.Load<DateTime>(Call);

		Assert.AreEqual(input, output);
	}

	[Test, Description("Serialize DateTime UTC")]
	public void SerializeDateTimeUtc()
	{
		DateTime input = DateTime.UtcNow;

		_serializer.Save(Call, input);
		DateTime output = _serializer.Load<DateTime>(Call);

		Assert.AreEqual(input, output);
	}

	[Test, Description("Serialize DateTimeOffset Local")]
	public void SerializeDateTimeOffsetLocal()
	{
		DateTime dateTime = DateTime.Now;
		DateTimeOffset input = new(dateTime);

		_serializer.Save(Call, input);
		DateTimeOffset output = _serializer.Load<DateTimeOffset>(Call);

		Assert.AreEqual(input, output);
	}

	[Test, Description("Serialize DateTimeOffset UTC")]
	public void SerializeDateTimeOffsetUtc()
	{
		DateTime dateTime = DateTime.UtcNow;
		DateTimeOffset input = new(dateTime);

		_serializer.Save(Call, input);
		DateTimeOffset output = _serializer.Load<DateTimeOffset>(Call);

		Assert.AreEqual(input, output);
	}

	[Test, Description("Serialize TimeZoneInfo")]
	public void SerializeTimeZoneInfo()
	{
		TimeZoneInfo input = TimeZoneInfo.Local;

		_serializer.Save(Call, input);
		TimeZoneInfo output = _serializer.Load<TimeZoneInfo>(Call);

		Assert.AreEqual(input, output);
	}

	[Test, Description("Serialize TimeZoneInfo Pacific Standard Time")]
	public void SerializeTimeZoneInfoPST()
	{
		TimeZoneInfo input = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

		_serializer.Save(Call, input);
		TimeZoneInfo output = _serializer.Load<TimeZoneInfo>(Call);

		Assert.AreEqual(input, output);
	}

	[Test, Description("Serialize TimeZoneView")]
	public void SerializeTimeZoneView()
	{
		TimeZoneView input = TimeZoneView.Local;

		_serializer.Save(Call, input);
		TimeZoneView output = _serializer.Load<TimeZoneView>(Call);

		Assert.AreEqual(input.Abbreviation, output.Abbreviation);
		Assert.AreEqual(input.Name, output.Name);
		Assert.AreEqual(input.TimeZoneInfo, output.TimeZoneInfo);
	}

	public class NullableDateTime
	{
		public long Long { get; set; }
		public DateTime? TimeStamp { get; set; }
	}

	[Test, Description("Serialize Long and DateTime")]
	public void SerializeLongAndDateTime()
	{
		var input = new NullableDateTime()
		{
			TimeStamp = DateTime.UtcNow,
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<NullableDateTime>(Call);

		Assert.AreEqual(input.TimeStamp, output.TimeStamp);
	}
}
