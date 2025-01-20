using SideScroll.Attributes;
using SideScroll.Extensions;

namespace SideScroll.Time;

[PublicData]
public class TimeZoneView : IComparable
{
	public string? Abbreviation { get; set; }
	public string? Name { get; set; }
	public TimeZoneInfo? TimeZoneInfo { get; set; }

	public TimeZoneView() { }

	public TimeZoneView(string abbreviation, string name, TimeZoneInfo timeZoneInfo)
	{
		Abbreviation = abbreviation;
		Name = name;
		TimeZoneInfo = timeZoneInfo;
	}

	public TimeZoneView(string abbreviation, string name, string id, TimeSpan timeSpan)
	{
		Abbreviation = abbreviation;
		Name = name;

		try
		{
			TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(id);
		}
		catch (Exception)
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

	public int CompareTo(object? obj)
	{
		return obj?.ToString()?.CompareTo(ToString()) ?? 1;
	}

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

	public static TimeZoneView Utc { get; set; } = new("Utc", "Utc", TimeZoneInfo.Utc);
	public static TimeZoneView Local { get; set; } = new("Local", "Local", TimeZoneInfo.Local);

	public static TimeZoneView Current { get; set; } = Local;

	public static DateTime Now => Current.Convert(DateTime.Now);

	// Time Zones can have different names across Operating Systems, and this provides a compatible view
	// Some abbreviations are reused across different countries and are ambiguous to use
	// https://en.wikipedia.org/wiki/List_of_time_zone_abbreviations
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
