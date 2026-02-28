using SideScroll.Attributes;
using SideScroll.Extensions;

namespace SideScroll.Time;

/// <summary>
/// Provides a view of a time zone with abbreviation, name, and conversion functionality
/// </summary>
[PublicData]
public class TimeZoneView : IComparable
{
	/// <summary>
	/// The time zone abbreviation (e.g., PST, EST, UTC)
	/// </summary>
	public string? Abbreviation { get; set; }

	/// <summary>
	/// The full time zone name
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// The .NET TimeZoneInfo for this time zone
	/// </summary>
	public TimeZoneInfo? TimeZoneInfo { get; set; }

	/// <summary>
	/// Initializes a new instance of the TimeZoneView class
	/// </summary>
	public TimeZoneView() { }

	/// <summary>
	/// Initializes a new instance of the TimeZoneView class with the specified properties
	/// </summary>
	public TimeZoneView(string abbreviation, string name, TimeZoneInfo timeZoneInfo)
	{
		Abbreviation = abbreviation;
		Name = name;
		TimeZoneInfo = timeZoneInfo;
	}

	/// <summary>
	/// Initializes a new instance of the TimeZoneView class, looking up or creating a custom time zone
	/// </summary>
	/// <param name="abbreviation">The time zone abbreviation</param>
	/// <param name="name">The full time zone name</param>
	/// <param name="id">The time zone identifier to look up</param>
	/// <param name="timeSpan">The UTC offset to use if creating a custom time zone</param>
	public TimeZoneView(string abbreviation, string name, string id, TimeSpan timeSpan)
	{
		Abbreviation = abbreviation;
		Name = name;

		try
		{
			TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(id);
		}
		catch (TimeZoneNotFoundException)
		{
			TimeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(id, timeSpan, name, name);
		}
	}

	public override string? ToString()
	{
		if (Abbreviation == Name)
			return Abbreviation;
		return Abbreviation + " - " + Name + ": " + TimeZoneInfo?.BaseUtcOffset.FormattedDecimal();
	}

	/// <summary>
	/// Converts a DateTime to this time zone
	/// </summary>
	public DateTime Convert(DateTime dateTime)
	{
		if (Equals(Utc)) return ConvertTimeToUtc(dateTime);

		if (dateTime.Kind == DateTimeKind.Utc)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo!);
		}

		if (Equals(Local))
		{
			return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
		}
		else if (dateTime.Kind != DateTimeKind.Unspecified)
		{
			TimeSpan utcOffset = TimeZoneInfo!.GetUtcOffset(dateTime);
			DateTime utcDateTime = ConvertTimeToUtc(dateTime).Add(utcOffset);
			return DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified);
		}

		return dateTime;
	}

	/// <summary>
	/// Converts a DateTime to UTC using this time zone as the source
	/// </summary>
	public DateTime ConvertTimeToUtc(DateTime dateTime)
	{
		if (Equals(Utc))
		{
			if (dateTime.Kind == DateTimeKind.Utc)
			{
				return dateTime;
			}
		}
		else if (Equals(Local))
		{
			dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
		}
		else
		{
			dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
		}

		return TimeZoneInfo.ConvertTimeToUtc(dateTime);
	}

	/// <summary>
	/// Compares this time zone to another object by string representation
	/// </summary>
	public int CompareTo(object? obj)
	{
		return obj?.ToString()?.CompareTo(ToString()) ?? 1;
	}

	/// <summary>
	/// Determines whether the specified object is a TimeZoneView with the same name
	/// </summary>
	public override bool Equals(object? obj)
	{
		if (obj is TimeZoneView timeZoneView)
		{
			return timeZoneView.Name == Name;
		}

		return false;
	}

	// Override to make compiler happy
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>
	/// UTC time zone
	/// </summary>
	public static TimeZoneView Utc { get; } = new("Utc", "Utc", TimeZoneInfo.Utc);

	/// <summary>
	/// Local system time zone
	/// </summary>
	public static TimeZoneView Local { get; } = new("Local", "Local", TimeZoneInfo.Local);

	/// <summary>
	/// The current time zone being used (defaults to Local)
	/// </summary>
	public static TimeZoneView Current { get; set; } = Local;

	/// <summary>
	/// Gets the current DateTime in the Current time zone
	/// </summary>
	public static DateTime Now => Current.Convert(DateTime.Now);

	/// <summary>
	/// List of common time zones with cross-platform compatible names.
	/// Time zones can have different names across operating systems, and this provides a compatible view.
	/// Some abbreviations are reused across different countries and are ambiguous to use.
	/// See: https://en.wikipedia.org/wiki/List_of_time_zone_abbreviations
	/// </summary>
	public static List<TimeZoneView> All { get; set; } =
	[
		Utc,
		Local,
		new("PST / PDT", "Pacific Time", "Pacific Standard Time", TimeSpan.FromHours(-8)),
		new("MT", "Mountain Time (North America)", "Mountain Standard Time", TimeSpan.FromHours(-7)), // MDT: UTC−06
		new("CST", "Central Standard Time", "Central Standard Time", TimeSpan.FromHours(-6)),
		new("EST / EDT", "Eastern Time", "Eastern Standard Time", TimeSpan.FromHours(-5)),
		new("CET", "Central European Time", "Central European Standard Time", TimeSpan.FromHours(1)),
		new("SGT", "Singapore Time", "Singapore Standard Time", TimeSpan.FromHours(8)),
		new("JST", "Japan Time", "Japan Standard Time", TimeSpan.FromHours(9)), // Windows 10: uses old id/name: Tokyo Standard Time
	];
}
