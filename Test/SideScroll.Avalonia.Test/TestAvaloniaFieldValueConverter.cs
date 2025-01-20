using NUnit.Framework;
using SideScroll.Avalonia.Controls.Converters;
using SideScroll.Time;
using System.Globalization;

namespace SideScroll.Avalonia.Test;

public class TestAvaloniaFieldValueConverter
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void ConvertDateTime()
	{
		TimeZoneView.Current = TimeZoneView.Utc;
		DateTime dateTime = new(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc);
		var converter = new FormatValueConverter();
		string converted = (string)converter.Convert(dateTime, typeof(string), null, CultureInfo.CurrentCulture)!;
		DateTime processedDateTime = (DateTime)converter.Convert(converted, typeof(DateTime), null, CultureInfo.CurrentCulture)!;
		Assert.That(processedDateTime, Is.EqualTo(dateTime));
	}
}
