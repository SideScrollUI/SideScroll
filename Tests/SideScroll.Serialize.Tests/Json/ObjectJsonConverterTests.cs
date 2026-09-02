using NUnit.Framework;
using SideScroll.Serialize.Json;
using System.Text.Json;

namespace SideScroll.Serialize.Tests.Json;

/// <summary>
/// A value held in an object member has to come back as the type it went in as. Only what JSON
/// represents natively can go out bare, everything else needs its type written alongside it
/// </summary>
[Category("Serialize")]
public class ObjectJsonConverterTests
{
	public class Holder
	{
		public object? Value { get; set; }
	}

	private static object? RoundTrip(object? value, JsonSerializerOptions? options = null)
	{
		options ??= JsonConverters.PrivateSerializerOptions;

		string json = JsonSerializer.Serialize(new Holder { Value = value }, options);
		return JsonSerializer.Deserialize<Holder>(json, options)?.Value;
	}

	[Test, Description("These went out as a bare string or number and came back as one")]
	public void TypedScalarsRoundTripAsThemselves()
	{
		var dateTime = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
		var dateTimeOffset = new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(2));
		var timeSpan = TimeSpan.FromMinutes(90);
		Guid guid = Guid.NewGuid();

		Assert.That(RoundTrip(dateTime), Is.EqualTo(dateTime));
		Assert.That(RoundTrip(dateTimeOffset), Is.EqualTo(dateTimeOffset));
		Assert.That(RoundTrip(timeSpan), Is.EqualTo(timeSpan));
		Assert.That(RoundTrip(guid), Is.EqualTo(guid));
	}

	[Test, Description("A decimal came back as a double, losing precision without reporting anything")]
	public void DecimalRoundTripsWithoutLosingPrecision()
	{
		const decimal Value = 1.0000000000000000000000000001m;

		object? result = RoundTrip(Value);

		Assert.That(result, Is.TypeOf<decimal>());
		Assert.That(result, Is.EqualTo(Value));
	}

	[Test, Description("These are what Read() reconstructs as themselves, so they still go out bare")]
	public void PrimitivesAndStringsAreUnchanged()
	{
		foreach (object value in new object[] { 42, 1.5, true, false, "text", (long)9_000_000_000 })
		{
			object? result = RoundTrip(value);

			// Is.EqualTo converts between numeric types, so the type is what has to be asserted first
			Assert.That(result, Is.TypeOf(value.GetType()), value.GetType().Name);
			Assert.That(result, Is.EqualTo(value), value.GetType().Name);
		}
	}

	[Test, Description(
		"A primitive without a JSON representation of its own came back as whichever type the " +
		"reader's fallback produced: a byte or short as an int, a float as a double, a char as a string")]
	public void NarrowerPrimitivesRoundTripAsThemselves()
	{
		foreach (object value in new object[] { (byte)5, (sbyte)-5, (short)5, (ushort)5, (uint)5u, 5UL, 'c', 1.5f })
		{
			object? result = RoundTrip(value);

			Assert.That(result, Is.TypeOf(value.GetType()), value.GetType().Name);
			Assert.That(result, Is.EqualTo(value), value.GetType().Name);
		}
	}

	[Test, Description(
		"A bare number is read back by magnitude, not by the type that wrote it, so int is the only " +
		"one of the bare number types whose whole range survives")]
	public void NumbersInsideIntRangeComeBackAsInt()
	{
		// A long small enough to be an int
		Assert.That(RoundTrip(42L), Is.TypeOf<int>());

		// A double is written without a decimal point when it has nothing to put after one, so 2.0
		// is written as 2 and is a whole number by the time the reader sees it
		Assert.That(RoundTrip(2.0d), Is.TypeOf<int>());

		// The value is what's preserved, so the two compare equal to what went in
		Assert.That(RoundTrip(42L), Is.EqualTo(42L));
		Assert.That(RoundTrip(2.0d), Is.EqualTo(2.0d));
	}

	[Test, Description("A bare value written before this still loads, the reader dispatches on shape")]
	public void PreviouslyWrittenBareValuesStillLoad()
	{
		// What the old writer produced for a DateTime in an object member
		const string OldJson = """{"Value":"2020-01-02T03:04:05Z"}""";

		Holder? holder = JsonSerializer.Deserialize<Holder>(OldJson, JsonConverters.PrivateSerializerOptions);

		Assert.That(holder!.Value, Is.EqualTo("2020-01-02T03:04:05Z"), "still a string, as it was before");
	}

	[Test, Description("Null and blocked types are unaffected by which types carry their own")]
	public void NullRoundTripsAsNull()
	{
		Assert.That(RoundTrip(null), Is.Null);
	}

	[Test, Description("The public options are what a shared link uses, and behave the same way")]
	public void PublicOptionsRoundTripTypedScalars()
	{
		var dateTime = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);

		Assert.That(RoundTrip(dateTime, JsonConverters.PublicSerializerOptions), Is.EqualTo(dateTime));
		Assert.That(RoundTrip(12.34m, JsonConverters.PublicSerializerOptions), Is.EqualTo(12.34m));
	}

	[Test, Description("A collection of them round trips too, since the elements go through the same converter")]
	public void TypedScalarsInsideACollectionRoundTrip()
	{
		var dateTime = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
		List<object?> values = [dateTime, 12.34m, Guid.Empty, "text", 42];

		object? result = RoundTrip(values);

		Assert.That(result, Is.InstanceOf<List<object?>>());
		Assert.That((List<object?>)result!, Is.EqualTo(values));
	}
}
