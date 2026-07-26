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

	// ─── Overlapping ranges ──────────────────────────────────────────────

	// A long range, a short one nested inside it, then a later one also inside it
	private static List<TimeRangeValue> CreateNested() =>
	[
		new(StartTime, StartTime.AddMinutes(60), 1),
		new(StartTime.AddMinutes(10), StartTime.AddMinutes(20), 2),
		new(StartTime.AddMinutes(50), StartTime.AddMinutes(55), 3),
	];

	[Test, Description(
		"A shorter range nested inside a longer one used to move the running end time backwards, so " +
		"a later point inserted a gap into time the longer range already covered")]
	public void FillGapsNestedRangesHaveNoGaps()
	{
		List<TimeRangeValue> output = TimeRangeValue.FillAndMerge(CreateNested(), TimeSpan.FromMinutes(5));

		Assert.That(output.Where(v => double.IsNaN(v.Value)), Is.Empty,
			"Every point falls inside the first range, so there's nothing to break the line for.");
		Assert.That(output, Has.Count.EqualTo(3));
	}

	[Test, Description("The same nested ranges filled across a window, including the trailing gap")]
	public void FillAndMergeNestedRangesHaveNoGaps()
	{
		List<TimeRangeValue> output = TimeRangeValue.FillAndMerge(
			CreateNested(), StartTime, StartTime.AddMinutes(60), TimeSpan.FromMinutes(5));

		Assert.That(output.Where(v => double.IsNaN(v.Value)), Is.Empty);
		Assert.That(output, Has.Count.EqualTo(3));
	}

	[Test, Description("A real gap after an overlapping run is still detected")]
	public void FillGapsAfterNestedRangesStillBreaks()
	{
		List<TimeRangeValue> input =
		[
			.. CreateNested(),
			new(StartTime.AddMinutes(90), StartTime.AddMinutes(95), 4),
		];

		List<TimeRangeValue> output = TimeRangeValue.FillAndMerge(input, TimeSpan.FromMinutes(5));

		List<TimeRangeValue> gaps = [.. output.Where(v => double.IsNaN(v.Value))];
		Assert.That(gaps, Has.Count.EqualTo(1));
		Assert.That(gaps[0].StartTime, Is.EqualTo(StartTime.AddMinutes(65)), "Starts after the longest range.");
		Assert.That(gaps[0].EndTime, Is.EqualTo(StartTime.AddMinutes(90)));
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
