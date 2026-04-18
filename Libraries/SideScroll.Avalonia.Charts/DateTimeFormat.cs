using SideScroll.Utilities;

namespace SideScroll.Avalonia.Charts;

/// <summary>
/// Selects a date/time format string based on a time window duration, showing enough precision to distinguish individual periods.
/// </summary>
public class DateTimeFormat(string? dateFormat, string? timeFormat, string? timeFormatUtc, TimeSpan minimum, TimeSpan maximum)
{
	/// <summary>Average number of days in a year, used for yearly duration calculations.</summary>
	public const double DaysInAYear = 365.25;

	/// <summary>Gets the date portion format string, or <c>null</c> if no date should be shown.</summary>
	public string? DateFormat => dateFormat;
	/// <summary>Gets the time portion format string for local time, or <c>null</c> if no time should be shown.</summary>
	public string? TimeFormat => timeFormat;
	/// <summary>Gets the time portion format string for UTC times, or <c>null</c> to fall back to <see cref="TimeFormat"/>.</summary>
	public string? TimeFormatUtc => timeFormatUtc;

	/// <summary>Gets the minimum window duration this format applies to.</summary>
	public TimeSpan Minimum => minimum;
	/// <summary>Gets the maximum window duration this format applies to.</summary>
	public TimeSpan Maximum => maximum;

	public override string ToString() => $"{DateFormat} {timeFormat}: {Minimum} - {Maximum}";

	/// <summary>Formats a <see cref="DateTime"/> using this format's date and time strings.</summary>
	public string Format(DateTime dateTime)
	{
		string label = "";

		if (DateFormat != null)
		{
			label += dateTime.ToString(DateFormat);
		}

		if (dateTime.Kind == DateTimeKind.Utc && TimeFormatUtc != null)
		{
			label += ' ' + dateTime.ToString(TimeFormatUtc);
		}
		else if (TimeFormat != null)
		{
			label += ' ' + dateTime.ToString(TimeFormat);
		}

		return label;
	}

	/// <summary>Gets or sets the ordered list of available formats, from sub-second to multi-year precision.</summary>
	public static List<DateTimeFormat> Formats { get; set; } =
	[
		new(null, DateTimeUtils.GetTimeFormatMilliseconds(), "H:mm:ss.FFF", TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(3)),
		new(null, "T", "H:mm:ss", TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(3)),
		new(null, "t", "H:mm", TimeSpan.FromMinutes(1), TimeSpan.FromDays(1)),
		new("M/d", "t", "H:mm", TimeSpan.FromMinutes(1), TimeSpan.FromDays(3)),
		new("M/d", null, null, TimeSpan.FromDays(1), TimeSpan.FromDays(6 * 30)),
		new("yyyy-M-d", null, null, TimeSpan.FromDays(1), TimeSpan.FromDays(6 * DaysInAYear)),
		new("yyyy", null, null, TimeSpan.FromDays(DaysInAYear), TimeSpan.FromDays(1000.0 * DaysInAYear)),
	];

	/// <summary>Returns the most appropriate format for the given window duration, or <c>null</c> if the duration exceeds all defined ranges.</summary>
	public static DateTimeFormat? GetWindowFormat(TimeSpan duration)
	{
		return Formats.FirstOrDefault(format => duration < format.Maximum);
	}
}
