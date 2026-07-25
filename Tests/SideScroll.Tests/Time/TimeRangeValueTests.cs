using NUnit.Framework;
using SideScroll.Time;

namespace SideScroll.Tests.Time;

[Category("Core")]
public class TimeRangeValueTests : BaseTest
{
	private static readonly DateTime StartTime = new(2000, 1, 1);

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	private static List<TimeRangeValue> CreateContiguous(int count, double value)
	{
		return Enumerable.Range(0, count)
			.Select(i => new TimeRangeValue(StartTime.AddMinutes(i), StartTime.AddMinutes(i + 1), value))
			.ToList();
	}

	[Test, Description("Merging doesn't modify the values passed in")]
	public void FillAndMergeDoesNotModifyInput()
	{
		List<TimeRangeValue> input = CreateContiguous(3, 5);
		List<DateTime> originalEndTimes = input.Select(i => i.EndTime).ToList();

		TimeRangeValue.FillAndMerge(input, StartTime, StartTime.AddMinutes(3), TimeSpan.FromMinutes(1));

		Assert.That(input.Select(i => i.EndTime), Is.EqualTo(originalEndTimes));
	}

	[Test, Description("Identical values still merge into the first, keeping the last")]
	public void FillAndMergeMergesIdenticalValues()
	{
		List<TimeRangeValue> input = CreateContiguous(3, 5);

		List<TimeRangeValue> output = TimeRangeValue.FillAndMerge(input, StartTime, StartTime.AddMinutes(3), TimeSpan.FromMinutes(1));

		Assert.That(output, Has.Count.EqualTo(2));
		Assert.That(output[0].StartTime, Is.EqualTo(StartTime));
		Assert.That(output[0].EndTime, Is.EqualTo(StartTime.AddMinutes(2)));
		Assert.That(output[1].StartTime, Is.EqualTo(StartTime.AddMinutes(2)));
		Assert.That(output[1].EndTime, Is.EqualTo(StartTime.AddMinutes(3)));
	}

	[Test, Description("Merging the same values twice gives the same result")]
	public void FillAndMergeIsRepeatable()
	{
		List<TimeRangeValue> input = CreateContiguous(3, 5);

		List<TimeRangeValue> first = TimeRangeValue.FillAndMerge(input, StartTime, StartTime.AddMinutes(3), TimeSpan.FromMinutes(1));
		List<TimeRangeValue> second = TimeRangeValue.FillAndMerge(input, StartTime, StartTime.AddMinutes(3), TimeSpan.FromMinutes(1));

		Assert.That(second.Select(v => v.EndTime), Is.EqualTo(first.Select(v => v.EndTime)));
	}

	[Test, Description("Different values aren't merged")]
	public void FillAndMergeKeepsDifferentValues()
	{
		List<TimeRangeValue> input =
		[
			new(StartTime, StartTime.AddMinutes(1), 1),
			new(StartTime.AddMinutes(1), StartTime.AddMinutes(2), 2),
		];

		List<TimeRangeValue> output = TimeRangeValue.FillAndMerge(input, StartTime, StartTime.AddMinutes(2), TimeSpan.FromMinutes(1));

		Assert.That(output, Has.Count.EqualTo(2));
		Assert.That(output[0].Value, Is.EqualTo(1));
		Assert.That(output[1].Value, Is.EqualTo(2));
	}

	[Test, Description("Inserted gaps use the same DateTimeKind as the values around them")]
	public void FillGapsKeepsDateTimeKind()
	{
		DateTime localStart = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Local);
		List<TimeRangeValue> input =
		[
			new(localStart, localStart.AddMinutes(1), 1),
			new(localStart.AddMinutes(10), localStart.AddMinutes(11), 2),
		];

		List<TimeRangeValue> output = TimeRangeValue.FillAndMerge(input, TimeSpan.FromMinutes(1));

		List<TimeRangeValue> gaps = output.Where(v => double.IsNaN(v.Value)).ToList();
		Assert.That(gaps, Has.Count.EqualTo(1));
		Assert.That(gaps[0].StartTime.Kind, Is.EqualTo(DateTimeKind.Local));
		Assert.That(gaps[0].EndTime.Kind, Is.EqualTo(DateTimeKind.Local));
		Assert.That(gaps[0].StartTime, Is.EqualTo(localStart.AddMinutes(2)));
		Assert.That(gaps[0].EndTime, Is.EqualTo(localStart.AddMinutes(10)));
	}

	[Test]
	public void CopyConstructorCopiesValues()
	{
		TimeRangeValue original = new(StartTime, StartTime.AddMinutes(1), 5, new Tag("Name", "Value"));

		TimeRangeValue copy = new(original)
		{
			EndTime = StartTime.AddMinutes(2),
		};

		Assert.That(original.EndTime, Is.EqualTo(StartTime.AddMinutes(1)));
		Assert.That(copy.StartTime, Is.EqualTo(original.StartTime));
		Assert.That(copy.Value, Is.EqualTo(original.Value));
		Assert.That(copy.Tags, Is.EqualTo(original.Tags));
	}
}
