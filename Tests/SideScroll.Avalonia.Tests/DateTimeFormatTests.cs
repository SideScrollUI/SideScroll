using NUnit.Framework;
using SideScroll.Avalonia.Charts;

namespace SideScroll.Avalonia.Tests;

[Category("Charts")]
[SetCulture("en-US")] // The "t" and "T" format strings are culture dependent
public class DateTimeFormatTests : BaseTest
{
	private static readonly DateTime LocalTime = new(2000, 1, 2, 14, 30, 0, DateTimeKind.Local);
	private static readonly DateTime UtcTime = new(2000, 1, 2, 14, 30, 0, DateTimeKind.Utc);

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Charts");
	}

	[Test, Description("A format with no date part shouldn't prefix its time with the separator")]
	public void FormatWithoutDateHasNoLeadingSpace()
	{
		DateTimeFormat format = new(null, "H:mm", "H:mm", TimeSpan.FromMinutes(1), TimeSpan.FromDays(1));

		Assert.That(format.Format(LocalTime), Is.EqualTo("14:30"));
	}

	[Test, Description("The separator is still written between a date and a time")]
	public void FormatWithDateAndTimeIsSeparated()
	{
		DateTimeFormat format = new("M/d", "H:mm", "H:mm", TimeSpan.FromMinutes(1), TimeSpan.FromDays(3));

		Assert.That(format.Format(LocalTime), Is.EqualTo("1/2 14:30"));
	}

	[Test, Description("A date only format is unchanged")]
	public void FormatWithDateOnlyHasNoTrailingSpace()
	{
		DateTimeFormat format = new("yyyy-M-d", null, null, TimeSpan.FromDays(1), TimeSpan.FromDays(180));

		Assert.That(format.Format(LocalTime), Is.EqualTo("2000-1-2"));
	}

	[Test, Description("A Utc value uses TimeFormatUtc, and falls back to TimeFormat when it isn't set")]
	public void FormatUsesTheUtcTimeFormatForUtcValues()
	{
		DateTimeFormat withUtc = new(null, "t", "H:mm", TimeSpan.FromMinutes(1), TimeSpan.FromDays(1));
		Assert.That(withUtc.Format(UtcTime), Is.EqualTo("14:30"));
		Assert.That(withUtc.Format(LocalTime), Is.EqualTo("2:30 PM"));

		DateTimeFormat withoutUtc = new(null, "t", null, TimeSpan.FromMinutes(1), TimeSpan.FromDays(1));
		Assert.That(withoutUtc.Format(UtcTime), Is.EqualTo("2:30 PM"), "Falls back to TimeFormat.");
	}

	[Test, Description("None of the built in sub day formats start with a space, they're used as axis labels")]
	public void BuiltInFormatsHaveNoLeadingSpace()
	{
		foreach (DateTimeFormat format in DateTimeFormat.Formats)
		{
			Assert.That(format.Format(LocalTime), Does.Not.StartWith(" "), format.ToString());
			Assert.That(format.Format(UtcTime), Does.Not.StartWith(" "), format.ToString());
		}
	}
}
