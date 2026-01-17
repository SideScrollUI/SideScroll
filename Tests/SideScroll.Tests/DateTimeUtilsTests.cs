using NUnit.Framework;
using SideScroll.Time;
using SideScroll.Utilities;

namespace SideScroll.Tests;

[Category("Core")]
public class DateTimeUtilsTests : BaseTest
{
	private TimeZoneView _originalTimeZone = null!;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
		// Save the original timezone
		_originalTimeZone = TimeZoneView.Current;
	}

	[OneTimeTearDown]
	public void BaseTearDown()
	{
		// Restore original timezone
		TimeZoneView.Current = _originalTimeZone;
	}

	[SetUp]
	public void Setup()
	{
		// Reset to UTC for consistent test results
		TimeZoneView.Current = TimeZoneView.Utc;
	}

	#region FormatTimeRange Tests

	[Test]
	public void FormatTimeRange_SameDay_NoSeconds_UtcTime()
	{
		// Same day, no seconds (minutes only), UTC time
		DateTime start = new(2023, 10, 18, 14, 30, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 18, 15, 45, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-18 14:30 - 15:45 - 1.3 Hours"));
	}

	[Test]
	public void FormatTimeRange_SameDay_WithSeconds_UtcTime()
	{
		// Same day with seconds, UTC time
		DateTime start = new(2023, 10, 18, 14, 30, 45, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 18, 15, 45, 30, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-18 14:30:45 - 15:45:30 - 1.2 Hours"));
	}

	[Test]
	public void FormatTimeRange_SameDay_WithMilliseconds_UtcTime()
	{
		// Same day with milliseconds, UTC time - uses local time format when milliseconds present
		DateTime start = new(2023, 10, 18, 14, 30, 45, 123, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 18, 15, 45, 30, 456, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		// Should use millisecond format and contain duration in Hours
		Assert.That(result, Does.Contain(".123"));
		Assert.That(result, Does.Contain(".456"));
		Assert.That(result, Does.Contain("Hours"));
	}

	[Test]
	public void FormatTimeRange_DifferentDays_WithSeconds()
	{
		// Different days
		DateTime start = new(2023, 10, 18, 14, 30, 45, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 19, 15, 45, 30, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-18 14:30:45 - 2023-10-19 15:45:30 - 1.1 Days"));
	}

	[Test]
	public void FormatTimeRange_DifferentDays_NoSeconds()
	{
		// Different days, no seconds
		DateTime start = new(2023, 10, 18, 14, 30, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 19, 15, 45, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-18 14:30 - 2023-10-19 15:45 - 1.1 Days"));
	}

	[Test]
	public void FormatTimeRange_WithoutDuration()
	{
		// Test without duration parameter
		DateTime start = new(2023, 10, 18, 14, 30, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 18, 15, 45, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end, withDuration: false);

		Assert.That(result, Is.EqualTo("2023-10-18 14:30 - 15:45"));
		Assert.That(result, Does.Not.Contain("Hours"));
	}

	[Test]
	public void FormatTimeRange_ZeroDuration()
	{
		// Start and end are the same
		DateTime start = new(2023, 10, 18, 14, 30, 0, DateTimeKind.Utc);
		DateTime end = start;

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-18 14:30 - 14:30 - 0 Seconds"));
	}

	[Test]
	public void FormatTimeRange_OneSecondDuration()
	{
		// Very short duration
		DateTime start = new(2023, 10, 18, 14, 30, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 18, 14, 30, 1, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-18 14:30:00 - 14:30:01 - 1 Second"));
	}

	[Test]
	public void FormatTimeRange_ExactlyOneHour()
	{
		DateTime start = new(2023, 10, 18, 14, 0, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 18, 15, 0, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-18 14:00 - 15:00 - 1 Hour"));
	}

	[Test]
	public void FormatTimeRange_ExactlyOneDay()
	{
		DateTime start = new(2023, 10, 18, 0, 0, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 19, 0, 0, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-18 0:00 - 2023-10-19 0:00 - 1 Day"));
	}

	[Test]
	public void FormatTimeRange_MultipleDays()
	{
		DateTime start = new(2023, 10, 18, 10, 30, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 25, 15, 45, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-18 10:30 - 2023-10-25 15:45 - 1 Weeks"));
	}

	[Test]
	public void FormatTimeRange_AcrossYearBoundary()
	{
		DateTime start = new(2023, 12, 31, 23, 30, 0, DateTimeKind.Utc);
		DateTime end = new(2024, 1, 1, 0, 30, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-12-31 23:30 - 2024-1-1 0:30 - 1 Hour"));
	}

	[Test]
	public void FormatTimeRange_LocalTimeZone_NoSeconds()
	{
		// Set timezone to Local
		TimeZoneView.Current = TimeZoneView.Local;

		DateTime start = new(2023, 10, 18, 14, 30, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 18, 15, 45, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		// The exact format will depend on system locale, but should contain date and duration
		Assert.That(result, Does.Contain("2023-10-18"));
		Assert.That(result, Does.Contain("Hours"));
	}

	[Test]
	public void FormatTimeRange_UnspecifiedDateTimeKind()
	{
		// Test with unspecified DateTime kind (treated as local time by TimeZoneInfo.ConvertTimeToUtc)
		// When TimeZoneView.Current is UTC, unspecified times get converted from local timezone
		DateTime start = new(2023, 10, 18, 14, 30, 0, DateTimeKind.Unspecified);
		DateTime end = new(2023, 10, 18, 15, 45, 0, DateTimeKind.Unspecified);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		// Result will vary based on local timezone, so just verify structure
		Assert.That(result, Does.Contain("2023-10-18"));
		Assert.That(result, Does.Contain("1.3 Hours"));
	}

	[Test]
	public void FormatTimeRange_MidnightTimes()
	{
		// Times at midnight
		DateTime start = new(2023, 10, 18, 0, 0, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 18, 1, 0, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-18 0:00 - 1:00 - 1 Hour"));
	}

	[Test]
	public void FormatTimeRange_NoonTimes()
	{
		// Times at noon
		DateTime start = new(2023, 10, 18, 12, 0, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 18, 13, 0, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-18 12:00 - 13:00 - 1 Hour"));
	}

	[Test]
	public void FormatTimeRange_WithMillisecondsOnly_StartTime()
	{
		// Only start time has milliseconds - uses local time format when milliseconds present
		DateTime start = new(2023, 10, 18, 14, 30, 45, 123, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 18, 15, 45, 30, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		// Should use millisecond format and include the .123
		Assert.That(result, Does.Contain(".123"));
		Assert.That(result, Does.Contain("Hours"));
	}

	[Test]
	public void FormatTimeRange_WithMillisecondsOnly_EndTime()
	{
		// Only end time has milliseconds - uses local time format when milliseconds present
		DateTime start = new(2023, 10, 18, 14, 30, 45, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 18, 15, 45, 30, 456, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		// Should use millisecond format and include the .456
		Assert.That(result, Does.Contain(".456"));
		Assert.That(result, Does.Contain("Hours"));
	}

	[Test]
	public void FormatTimeRange_SingleDigitDayAndMonth()
	{
		// Test with single-digit month and day (no leading zeros expected)
		DateTime start = new(2023, 1, 5, 14, 30, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 1, 5, 15, 45, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-1-5 14:30 - 15:45 - 1.3 Hours"));
	}

	[Test]
	public void FormatTimeRange_LeapYear_February29()
	{
		// Test with leap year date
		DateTime start = new(2024, 2, 29, 14, 30, 0, DateTimeKind.Utc);
		DateTime end = new(2024, 2, 29, 15, 45, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2024-2-29 14:30 - 15:45 - 1.3 Hours"));
	}

	[Test]
	public void FormatTimeRange_VeryLongDuration_Weeks()
	{
		// Test with duration of several weeks
		DateTime start = new(2023, 10, 1, 10, 0, 0, DateTimeKind.Utc);
		DateTime end = new(2023, 10, 31, 15, 30, 0, DateTimeKind.Utc);

		string result = DateTimeUtils.FormatTimeRange(start, end);

		Assert.That(result, Is.EqualTo("2023-10-1 10:00 - 2023-10-31 15:30 - 4.3 Weeks"));
	}

	#endregion

	#region TryParseTimeSpan Tests

	[Test]
	public void TryParseTimeSpan_HoursMinutesSeconds()
	{
		bool success = DateTimeUtils.TryParseTimeSpan("1:30:45", out TimeSpan result);

		Assert.That(success, Is.True);
		Assert.That(result.Hours, Is.EqualTo(1));
		Assert.That(result.Minutes, Is.EqualTo(30));
		Assert.That(result.Seconds, Is.EqualTo(45));
	}

	[Test]
	public void TryParseTimeSpan_HoursMinutes()
	{
		bool success = DateTimeUtils.TryParseTimeSpan("1:30", out TimeSpan result);

		Assert.That(success, Is.True);
		Assert.That(result.Hours, Is.EqualTo(1));
		Assert.That(result.Minutes, Is.EqualTo(30));
		Assert.That(result.Seconds, Is.EqualTo(0));
	}

	[Test]
	public void TryParseTimeSpan_WithFractionalSeconds()
	{
		bool success = DateTimeUtils.TryParseTimeSpan("1:30:45.1234567", out TimeSpan result);

		Assert.That(success, Is.True);
		Assert.That(result.Hours, Is.EqualTo(1));
		Assert.That(result.Minutes, Is.EqualTo(30));
		Assert.That(result.Seconds, Is.EqualTo(45));
	}

	[Test]
	public void TryParseTimeSpan_InvalidFormat()
	{
		bool success = DateTimeUtils.TryParseTimeSpan("invalid", out TimeSpan result);

		Assert.That(success, Is.False);
		Assert.That(result, Is.EqualTo(default(TimeSpan)));
	}

	[Test]
	public void TryParseTimeSpan_Null()
	{
		bool success = DateTimeUtils.TryParseTimeSpan(null, out TimeSpan result);

		Assert.That(success, Is.False);
	}

	[Test]
	public void TryParseTimeSpan_WithWhitespace()
	{
		bool success = DateTimeUtils.TryParseTimeSpan("  1:30:45  ", out TimeSpan result);

		Assert.That(success, Is.True);
		Assert.That(result.Hours, Is.EqualTo(1));
	}

	#endregion

	#region TryParseDateTime Tests

	[Test]
	public void TryParseDateTime_UnixEpoch10Digits()
	{
		bool success = DateTimeUtils.TryParseDateTime("1569998557", out DateTime result);

		Assert.That(success, Is.True);
		Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
	}

	[Test]
	public void TryParseDateTime_UnixEpoch13Digits()
	{
		bool success = DateTimeUtils.TryParseDateTime("1569998557298", out DateTime result);

		Assert.That(success, Is.True);
		Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
	}

	[Test]
	public void TryParseDateTime_StandardFormat()
	{
		bool success = DateTimeUtils.TryParseDateTime("2023-10-18 14:30:45", out DateTime result);

		Assert.That(success, Is.True);
		Assert.That(result.Year, Is.EqualTo(2023));
		Assert.That(result.Month, Is.EqualTo(10));
		Assert.That(result.Day, Is.EqualTo(18));
	}

	[Test]
	public void TryParseDateTime_ApacheLogFormat()
	{
		bool success = DateTimeUtils.TryParseDateTime("18/Jul/2019:11:47:45 +0000", out DateTime result);

		Assert.That(success, Is.True);
		Assert.That(result.Year, Is.EqualTo(2019));
		Assert.That(result.Month, Is.EqualTo(7));
		Assert.That(result.Day, Is.EqualTo(18));
	}

	[Test]
	public void TryParseDateTime_ApacheLogFormatNoTimezone()
	{
		bool success = DateTimeUtils.TryParseDateTime("18/Jul/2019:11:47:45", out DateTime result);

		Assert.That(success, Is.True);
		Assert.That(result.Year, Is.EqualTo(2019));
	}

	[Test]
	public void TryParseDateTime_InvalidFormat()
	{
		bool success = DateTimeUtils.TryParseDateTime("invalid", out DateTime result);

		Assert.That(success, Is.False);
	}

	[Test]
	public void TryParseDateTime_Null()
	{
		bool success = DateTimeUtils.TryParseDateTime(null, out DateTime result);

		Assert.That(success, Is.False);
	}

	[Test]
	public void TryParseDateTime_WithCommas()
	{
		bool success = DateTimeUtils.TryParseDateTime("1,569,998,557", out DateTime result);

		Assert.That(success, Is.True);
		Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
	}

	[Test]
	public void TryParseDateTime_ConvertedToUtc()
	{
		bool success = DateTimeUtils.TryParseDateTime("2023-10-18 14:30:45", out DateTime result);

		Assert.That(success, Is.True);
		Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
	}

	#endregion

	#region GetTimeFormatMilliseconds Tests

	[Test]
	public void GetTimeFormatMilliseconds_InvariantCulture()
	{
		string format = DateTimeUtils.GetTimeFormatMilliseconds(System.Globalization.CultureInfo.InvariantCulture);

		Assert.That(format, Is.Not.Null);
		Assert.That(format, Does.Contain("mm:ss.FFF"));
	}

	[Test]
	public void GetTimeFormatMilliseconds_DefaultCulture()
	{
		string format = DateTimeUtils.GetTimeFormatMilliseconds();

		Assert.That(format, Is.Not.Null);
		Assert.That(format, Does.Contain("mm:ss.FFF"));
	}

	#endregion

	#region EpochTime Tests

	[Test]
	public void EpochTime_IsCorrect()
	{
		Assert.That(DateTimeUtils.EpochTime.Year, Is.EqualTo(1970));
		Assert.That(DateTimeUtils.EpochTime.Month, Is.EqualTo(1));
		Assert.That(DateTimeUtils.EpochTime.Day, Is.EqualTo(1));
		Assert.That(DateTimeUtils.EpochTime.Hour, Is.EqualTo(0));
		Assert.That(DateTimeUtils.EpochTime.Minute, Is.EqualTo(0));
		Assert.That(DateTimeUtils.EpochTime.Second, Is.EqualTo(0));
		Assert.That(DateTimeUtils.EpochTime.Kind, Is.EqualTo(DateTimeKind.Utc));
	}

	#endregion
}
