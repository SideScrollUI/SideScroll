namespace Atlas.UI.Avalonia.Charts;

public class DateTimeFormat
{
	public string TextFormat { get; set; }

	public TimeSpan Minimum { get; set; }
	public TimeSpan Maximum { get; set; }

	public override string ToString() => $"{TextFormat}: {Minimum} - {Maximum}";

	public DateTimeFormat(string textFormat, TimeSpan minimum, TimeSpan maximum)
	{
		TextFormat = textFormat;
		Minimum = minimum;
		Maximum = maximum;
	}

	public static List<DateTimeFormat> DateFormats = new()
	{
		new("H:mm:ss", TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(3)),
		new("H:mm", TimeSpan.FromMinutes(1), TimeSpan.FromDays(1)),
		new("M/d H:mm", TimeSpan.FromMinutes(1), TimeSpan.FromDays(3)),
		new("M/d", TimeSpan.FromDays(1), TimeSpan.FromDays(6 * 30)),
		new("yyyy-M-d", TimeSpan.FromDays(1), TimeSpan.FromDays(1000.0 * 12 * 30)),
	};

	public static DateTimeFormat? GetWindowFormat(TimeSpan duration)
	{
		return DateFormats.FirstOrDefault(format => duration < format.Maximum);
	}
}
