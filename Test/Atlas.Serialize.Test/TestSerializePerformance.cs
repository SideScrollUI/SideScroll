using Atlas.Core;
using NUnit.Framework;

namespace Atlas.Serialize.Test;

[Category("Performance")]
public class TestSerializePerformance : TestSerializeBase
{
	private SerializerMemory _serializer = new SerializerMemoryAtlas();

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TestSerializePerformance");
	}

	[SetUp]
	public void SetUp()
	{
		_serializer = new SerializerMemoryAtlas();
	}

	[Test, Description("DictionaryTest")]
	public void DictionaryTest()
	{
		var items = new Dictionary<int, int>();

		for (int i = 0; i < 100000; i++)
		{
			items[i] = i;
		}

		using CallTimer callTimer = Call.Timer("Get Count");

		int count = items.Values.Count;
	}

	[Test]
	public void TimeRangeValue()
	{
		TimeRangeValue input = TimeRangeSample;

		_serializer.Save(Call, input);
		var output = _serializer.Load<TimeRangeValue>(Call);
	}

	[Test]
	public void TimeRangeValues()
	{
		List<TimeRangeValue> input = TimeRangeSamples(100000); // doesn't work for 1,000,000

		using CallTimer callTimer = Call.Timer("Cloning");

		_serializer.Save(callTimer, input);
		var output = _serializer.Load<List<TimeRangeValue>>(callTimer);
	}

	private static List<TimeRangeValue> TimeRangeSamples(int count)
	{
		var input = new List<TimeRangeValue>();

		for (int i = 0; i < count; i++)
		{
			input.Add(TimeRangeSample);
		}

		return input;
	}

	private static TimeRangeValue TimeRangeSample => new()
	{
		StartTime = new DateTime(1980, 10, 23),
		EndTime = new DateTime(2020, 10, 24),
	};
}
