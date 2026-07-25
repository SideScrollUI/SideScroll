using NUnit.Framework;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class SerializeTypeRepoTests : SerializeBaseTest
{
	private Serializer _serializer = new();

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeTypeRepo");
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new Serializer();
	}

	public enum ByteEnum : byte
	{
		First = 1,
		Last = 250,
	}

	public enum LongEnum : long
	{
		Big = 5_000_000_000,
	}

	public enum ShortEnum : short
	{
		Negative = -300,
	}

	public class EnumClass
	{
		public ByteEnum ByteValue { get; set; }
		public LongEnum LongValue { get; set; }
		public ShortEnum ShortValue { get; set; }
	}

	[Test, Description("Enums with a non int underlying type round trip")]
	public void SerializeNonIntEnums()
	{
		var input = new EnumClass
		{
			ByteValue = ByteEnum.Last,
			LongValue = LongEnum.Big,
			ShortValue = ShortEnum.Negative,
		};

		var output = input.DeepClone(Call)!;

		Assert.That(output.ByteValue, Is.EqualTo(input.ByteValue));
		Assert.That(output.LongValue, Is.EqualTo(input.LongValue));
		Assert.That(output.ShortValue, Is.EqualTo(input.ShortValue));
	}

	public class OffsetClass
	{
		public DateTimeOffset Value { get; set; }
	}

	[Test, Description("DateTimeOffset keeps its offset, not just the instant")]
	public void SerializeDateTimeOffsetKeepsOffset()
	{
		var input = new OffsetClass
		{
			Value = new DateTimeOffset(2024, 1, 2, 12, 0, 0, TimeSpan.FromHours(5)),
		};

		var output = input.DeepClone(Call)!;

		// EqualsExact() also compares the Offset, Equals() only compares the instant
		Assert.That(output.Value.EqualsExact(input.Value), Is.True,
			$"Expected {input.Value:o} but was {output.Value:o}");
		Assert.That(output.Value.Offset, Is.EqualTo(TimeSpan.FromHours(5)));
	}

	[Test]
	public void SerializeDateTimeOffsetNegativeOffset()
	{
		var input = new OffsetClass
		{
			Value = new DateTimeOffset(2024, 6, 1, 8, 30, 0, TimeSpan.FromHours(-7.5)),
		};

		var output = input.DeepClone(Call)!;

		Assert.That(output.Value.EqualsExact(input.Value), Is.True);
	}

	public class VersionClass
	{
		public Version? Value { get; set; } = new(3, 1, 2);
	}

	public class UriClass
	{
		public Uri? Value { get; set; } = new("https://example.com/path");
	}

	public class TimeZoneClass
	{
		public TimeZoneInfo? Value { get; set; } = TimeZoneInfo.Utc;
	}

	[Test, Description("Immutable reference types are shared instead of constructed when cloning")]
	public void CloneVersion()
	{
		var input = new VersionClass();

		var output = _serializer.Clone(Call.Log, input)!;

		Assert.That(output.Value, Is.EqualTo(new Version(3, 1, 2)));
	}

	[Test]
	public void CloneUri()
	{
		var input = new UriClass();

		var output = _serializer.Clone(Call.Log, input)!;

		Assert.That(output.Value, Is.EqualTo(input.Value));
	}

	[Test]
	public void CloneTimeZoneInfo()
	{
		var input = new TimeZoneClass();

		var output = _serializer.Clone(Call.Log, input)!;

		Assert.That(output.Value, Is.EqualTo(TimeZoneInfo.Utc));
	}

	public class MultiArrayClass
	{
		public int[,]? Grid { get; set; }
		public string[,,]? Cube { get; set; }
	}

	[Test, Description("Multi dimensional arrays keep their shape and values")]
	public void SerializeMultiDimensionalArray()
	{
		var input = new MultiArrayClass
		{
			Grid = new int[2, 3]
			{
				{ 1, 2, 3 },
				{ 4, 5, 6 },
			},
			Cube = new string[1, 2, 2]
			{
				{
					{ "a", "b" },
					{ "c", "d" },
				},
			},
		};

		var output = input.DeepClone(Call)!;

		Assert.That(output.Grid, Is.Not.Null);
		Assert.That(output.Grid!.Rank, Is.EqualTo(2));
		Assert.That(output.Grid.GetLength(0), Is.EqualTo(2));
		Assert.That(output.Grid.GetLength(1), Is.EqualTo(3));
		Assert.That(output.Grid, Is.EqualTo(input.Grid));

		Assert.That(output.Cube, Is.Not.Null);
		Assert.That(output.Cube!.Rank, Is.EqualTo(3));
		Assert.That(output.Cube, Is.EqualTo(input.Cube));
	}

	[Test]
	public void CloneMultiDimensionalArray()
	{
		var input = new MultiArrayClass
		{
			Grid = new int[2, 2]
			{
				{ 1, 2 },
				{ 3, 4 },
			},
		};

		var output = _serializer.Clone(Call.Log, input)!;

		Assert.That(output.Grid, Is.EqualTo(input.Grid));
	}

	public class ByteArrayClass
	{
		public byte[]? Value { get; set; }
	}

	[Test]
	public void SerializeByteArray()
	{
		var input = new ByteArrayClass
		{
			Value = Enumerable.Range(0, 10_000).Select(i => (byte)i).ToArray(),
		};

		var output = input.DeepClone(Call)!;

		Assert.That(output.Value, Is.EqualTo(input.Value));
	}

	[Test]
	public void SerializeJaggedArray()
	{
		int[][] input =
		[
			[1, 2],
			[3, 4, 5],
		];

		int[][] output = input.DeepClone(Call);

		Assert.That(output, Is.EqualTo(input));
	}
}
