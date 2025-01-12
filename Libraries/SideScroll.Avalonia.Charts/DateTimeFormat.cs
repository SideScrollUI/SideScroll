namespace SideScroll.Avalonia.Charts;

public class DateTimeFormat(string? dateFormat, string? timeFormat, string? timeFormatUtc, TimeSpan minimum, TimeSpan maximum)
{
	public string? DateFormat => dateFormat;
	public string? TimeFormat => timeFormat;
	public string? TimeFormatUtc => timeFormatUtc;

	public TimeSpan Minimum => minimum;
	public TimeSpan Maximum => maximum;

	public override string ToString() => $"{DateFormat} {timeFormat}: {Minimum} - {Maximum}";

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

	public static List<DateTimeFormat> Formats { get; set; } =
	[
		new(null, "T", "H:mm:ss", TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(3)),
		new(null, "t", "H:mm", TimeSpan.FromMinutes(1), TimeSpan.FromDays(1)),
		new("M/d", "t", "H:mm", TimeSpan.FromMinutes(1), TimeSpan.FromDays(3)),
		new("M/d", null, null, TimeSpan.FromDays(1), TimeSpan.FromDays(6 * 30)),
		new("yyyy-M-d", null, null, TimeSpan.FromDays(1), TimeSpan.FromDays(1000.0 * 12 * 30)),
	];

	public static DateTimeFormat? GetWindowFormat(TimeSpan duration)
	{
		return Formats.FirstOrDefault(format => duration < format.Maximum);
	}
}
