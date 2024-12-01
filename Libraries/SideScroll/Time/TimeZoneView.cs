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

	public DateTime ConvertTimeToUtc(DateTime dateTime)
	{
		if (this == Utc)
			return dateTime;

		if (this == Local)
		{
			dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
		}
		else
		{
			dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
		}

		return TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo!);
	}

	public int CompareTo(object? obj)
	{
		return obj?.ToString()?.CompareTo(ToString()) ?? 1;
	}

	public static readonly TimeZoneView Utc = new("Utc", "Utc", TimeZoneInfo.Utc);
	public static readonly TimeZoneView Local = new("Local", "Local", TimeZoneInfo.Local);

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
