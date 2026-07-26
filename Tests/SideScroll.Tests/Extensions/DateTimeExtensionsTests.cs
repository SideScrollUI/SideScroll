using NUnit.Framework;
using SideScroll.Extensions;
using System.Globalization;

namespace SideScroll.Tests.Extensions;

[Category("Core")]
public class DateTimeExtensionsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	[Test]
	public void Ceil_DefaultSeconds()
	{
		DateTime dateTime = new(2023, 10, 18, 14, 30, 45, 500, DateTimeKind.Utc);

		DateTime result = dateTime.Ceil();

		Assert.That(result, Is.EqualTo(new DateTime(2023, 10, 18, 14, 30, 46, DateTimeKind.Utc)));
		Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
	}

	[Test]
	public void Ceil_Minutes()
	{
		DateTime dateTime = new(2023, 10, 18, 14, 30, 30, DateTimeKind.Utc);

		DateTime result = dateTime.Ceil(TimeSpan.TicksPerMinute);

		Assert.That(result, Is.EqualTo(new DateTime(2023, 10, 18, 14, 31, 0, DateTimeKind.Utc)));
	}

	[Test]
	public void Ceil_AlreadyAligned()
	{
		DateTime dateTime = new(2023, 10, 18, 14, 30, 0, DateTimeKind.Utc);

		Assert.That(dateTime.Ceil(TimeSpan.TicksPerMinute), Is.EqualTo(dateTime));
		Assert.That(dateTime.Ceil(), Is.EqualTo(dateTime));
	}

	[Test, Description(
		"There's nothing above the final interval to round up to, so it saturates. Rounding past " +
		"MaxValue used to throw an ArgumentOutOfRangeException")]
	public void Ceil_MaxValue_Saturates()
	{
		Assert.That(DateTime.MaxValue.Ceil(), Is.EqualTo(DateTime.MaxValue));
		Assert.That(DateTime.MaxValue.Ceil(TimeSpan.TicksPerMinute), Is.EqualTo(DateTime.MaxValue));
		Assert.That(DateTime.MaxValue.Ceil(TimeSpan.TicksPerDay), Is.EqualTo(DateTime.MaxValue));
	}

	[Test, Description("Every value inside the final interval saturates, not just MaxValue itself")]
	public void Ceil_WithinTheFinalInterval_Saturates()
	{
		DateTime lastTick = new(DateTime.MaxValue.Ticks - 1, DateTimeKind.Utc);

		Assert.That(lastTick.Ceil(), Is.EqualTo(new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc)));
	}

	[Test]
	public void Ceil_Saturated_KeepsTheKind()
	{
		DateTime maxUtc = new(DateTime.MaxValue.Ticks, DateTimeKind.Utc);

		Assert.That(maxUtc.Ceil().Kind, Is.EqualTo(DateTimeKind.Utc));
	}

	[Test, Description("A value a full interval below MaxValue still rounds up normally")]
	public void Ceil_BelowTheFinalInterval_RoundsUp()
	{
		DateTime dateTime = new(DateTime.MaxValue.Ticks - TimeSpan.TicksPerSecond, DateTimeKind.Utc);

		DateTime result = dateTime.Ceil();

		Assert.That(result, Is.LessThan(DateTime.MaxValue), "It shouldn't have saturated.");
		Assert.That(result.Ticks % TimeSpan.TicksPerSecond, Is.Zero, "It should land on a second.");
	}

	[Test]
	public void Ceil_MinValue_IsUnchanged()
	{
		Assert.That(DateTime.MinValue.Ceil(), Is.EqualTo(DateTime.MinValue));
	}

	// ─── Max / Min ───────────────────────────────────────────────────────

	// Ticks are wall clock readings, so a Local and a Utc value for the same instant have
	// different Ticks. Comparing them directly picks the larger reading, not the later instant
	private static readonly DateTime _localNoon = new(2026, 7, 25, 12, 0, 0, DateTimeKind.Local);
	private static DateTime UtcSameInstant => _localNoon.ToUniversalTime();

	[Test, Description("Max compares instants rather than raw Ticks")]
	public void Max_MixedKinds_SameInstant_ReturnsThatInstant()
	{
		DateTime max = _localNoon.Max(UtcSameInstant);

		Assert.That(max.ToUniversalTime(), Is.EqualTo(_localNoon.ToUniversalTime()));
	}

	[Test, Description("The winning value keeps its own Kind instead of being relabelled")]
	public void Max_MixedKinds_KeepsTheWinnersKind()
	{
		DateTime later = _localNoon.AddHours(1);

		DateTime max = UtcSameInstant.Max(later);

		Assert.That(max.Kind, Is.EqualTo(DateTimeKind.Local), "The later value was the Local one.");
		Assert.That(max.ToUniversalTime(), Is.EqualTo(later.ToUniversalTime()));
	}

	[Test]
	public void Min_MixedKinds_ReturnsTheEarlierInstant()
	{
		DateTime earlier = _localNoon.AddHours(-1);

		DateTime min = UtcSameInstant.Min(earlier);

		Assert.That(min.Kind, Is.EqualTo(DateTimeKind.Local));
		Assert.That(min.ToUniversalTime(), Is.EqualTo(earlier.ToUniversalTime()));
	}

	[TestCase(DateTimeKind.Utc)]
	[TestCase(DateTimeKind.Local)]
	[TestCase(DateTimeKind.Unspecified)]
	[Description("Values sharing a Kind are unaffected, in both ordering and the resulting Kind")]
	public void MaxMin_SameKind_PicksTheExtreme(DateTimeKind kind)
	{
		DateTime earlier = new(2026, 7, 25, 12, 0, 0, kind);
		DateTime later = new(2026, 7, 25, 18, 0, 0, kind);

		Assert.That(earlier.Max(later), Is.EqualTo(later));
		Assert.That(later.Max(earlier), Is.EqualTo(later));
		Assert.That(earlier.Min(later), Is.EqualTo(earlier));
		Assert.That(later.Min(earlier), Is.EqualTo(earlier));
		Assert.That(earlier.Max(later).Kind, Is.EqualTo(kind));
	}

	// ─── Trim(DateTimeOffset) ────────────────────────────────────────────

	[Test, Description(
		"Trim keeps the offset the way DateTime.Trim keeps its Kind. Normalizing to +00:00 kept the " +
		"instant but changed the wall clock time everything displays.")]
	public void Trim_DateTimeOffset_KeepsTheOffset()
	{
		DateTimeOffset dateTimeOffset = new(2026, 7, 25, 10, 0, 0, 500, TimeSpan.FromHours(-7));

		DateTimeOffset trimmed = dateTimeOffset.Trim(TimeSpan.TicksPerSecond);

		Assert.That(trimmed.Offset, Is.EqualTo(TimeSpan.FromHours(-7)));
		Assert.That(trimmed.DateTime, Is.EqualTo(new DateTime(2026, 7, 25, 10, 0, 0)));
	}

	[Test, Description("Trimming still drops the sub second part of the instant")]
	public void Trim_DateTimeOffset_RemovesFractionalTicks()
	{
		DateTimeOffset dateTimeOffset = new(2026, 7, 25, 10, 0, 0, 500, TimeSpan.FromHours(-7));

		DateTimeOffset trimmed = dateTimeOffset.Trim(TimeSpan.TicksPerSecond);

		Assert.That(trimmed.Millisecond, Is.Zero);
		Assert.That(trimmed.UtcTicks, Is.EqualTo(
			dateTimeOffset.UtcTicks - (dateTimeOffset.UtcTicks % TimeSpan.TicksPerSecond)));
	}

	[Test]
	public void Trim_DateTimeOffset_AlreadyUtcAndAligned_IsUnchanged()
	{
		DateTimeOffset dateTimeOffset = new(2026, 7, 25, 10, 0, 0, TimeSpan.Zero);

		Assert.That(dateTimeOffset.Trim(TimeSpan.TicksPerSecond), Is.EqualTo(dateTimeOffset));
	}

	// ─── FormatId ────────────────────────────────────────────────────────

	[Test]
	public void FormatId_UsesTheGregorianCalendar()
	{
		DateTime dateTime = new(2026, 7, 25, 13, 45, 0, DateTimeKind.Utc);

		Assert.That(dateTime.FormatId(), Does.StartWith("2026-07-25"));
	}

	[TestCase("th-TH")] // Buddhist calendar, would render the year as 2569
	[TestCase("ar-SA")] // Hijri calendar, would render 1448-02-11
	[Description(
		"FormatId identifies a value, so the current culture's default calendar can't be allowed to " +
		"render the same instant differently on different machines")]
	public void FormatId_NonGregorianCulture_IsUnchanged(string culture)
	{
		DateTime dateTime = new(2026, 7, 25, 13, 45, 0, DateTimeKind.Utc);

		using var scope = new CultureScope(culture);

		Assert.That(dateTime.FormatId(), Does.StartWith("2026-07-25"));
	}

	private sealed class CultureScope : IDisposable
	{
		private readonly CultureInfo _original = CultureInfo.CurrentCulture;

		public CultureScope(string name)
		{
			CultureInfo.CurrentCulture = new CultureInfo(name);
		}

		public void Dispose() => CultureInfo.CurrentCulture = _original;
	}
}
