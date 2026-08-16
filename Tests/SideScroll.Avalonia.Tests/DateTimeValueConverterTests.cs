using NUnit.Framework;
using SideScroll.Avalonia.Controls.Converters;
using System.Globalization;

namespace SideScroll.Avalonia.Tests;

/// <summary>
/// The converter holds the date a time is merged onto, which the clipboard import relies on
/// </summary>
public class DateTimeValueConverterTests
{
	private static readonly DateTime BoundDateTime = new(2020, 6, 15, 8, 30, 0, DateTimeKind.Utc);

	private static DateTimeValueConverter CreateConverterHolding(DateTime dateTime)
	{
		var converter = new DateTimeValueConverter();

		// Reading the bound value is what stores it, the way the date binding does
		converter.Convert(dateTime, typeof(string), null, CultureInfo.InvariantCulture);

		return converter;
	}

	[Test, Description(
		"SetTime() merges onto the stored date, which is what importing a time needs. Converting " +
		"the TimeSpan instead returned a string that cast to a null DateTime and cleared the value")]
	public void SetTimeMergesOntoTheStoredDate()
	{
		DateTimeValueConverter converter = CreateConverterHolding(BoundDateTime);

		DateTime merged = converter.SetTime(new TimeSpan(14, 45, 0));

		Assert.That(merged.Date, Is.EqualTo(BoundDateTime.Date), "keeps the date it was bound to");
		Assert.That(merged.TimeOfDay, Is.EqualTo(new TimeSpan(14, 45, 0)), "takes the imported time");
	}

	[Test, Description("The merged time is what Convert() reports back for the text box")]
	public void SetTimeUpdatesTheTextItReportsBack()
	{
		DateTimeValueConverter converter = CreateConverterHolding(BoundDateTime);

		DateTime merged = converter.SetTime(new TimeSpan(14, 45, 0));

		Assert.That(converter.Convert(merged, typeof(string), null, CultureInfo.InvariantCulture),
			Is.EqualTo(new TimeSpan(14, 45, 0).ToString()));
	}

	[Test, Description(
		"Converting a TimeSpan is what the import used to do. It reads the stored date back out of " +
		"the value it's given, so a TimeSpan resets it, and it returns a string either way")]
	public void ConvertingATimeSpanReturnsAStringAndClearsTheStoredDate()
	{
		DateTimeValueConverter converter = CreateConverterHolding(BoundDateTime);

		object? result = converter.Convert(TimeSpan.FromHours(14), typeof(string), null, CultureInfo.InvariantCulture);

		Assert.That(result, Is.Not.InstanceOf<DateTime>(), "never a DateTime, so a DateTime? cast is null");

		// The stored date is gone, so merging a time afterwards no longer lands on the bound date
		Assert.That(converter.SetTime(new TimeSpan(14, 45, 0)).Date, Is.Not.EqualTo(BoundDateTime.Date),
			"the stored date was reset");
	}

	[Test, Description("With no date stored yet, a time still produces a usable value rather than nothing")]
	public void SetTimeWithoutAStoredDateUsesToday()
	{
		var converter = new DateTimeValueConverter();

		DateTime result = converter.SetTime(new TimeSpan(9, 15, 0));

		Assert.That(result.TimeOfDay, Is.EqualTo(new TimeSpan(9, 15, 0)));
	}

	[Test, Description(
		"Editing the time text still reports a validation error for an unparseable one, which is " +
		"the reason the string overload returns something a binding understands")]
	public void EditingWithAnInvalidTimeReportsAValidationError()
	{
		DateTimeValueConverter converter = CreateConverterHolding(BoundDateTime);

		object? result = converter.ConvertBack("not a time", typeof(DateTime), null, CultureInfo.InvariantCulture);

		Assert.That(result, Is.Not.InstanceOf<DateTime>());
	}

	[Test, Description("Editing the time text still merges onto the stored date")]
	public void EditingTheTimeTextMergesOntoTheStoredDate()
	{
		DateTimeValueConverter converter = CreateConverterHolding(BoundDateTime);

		object? result = converter.ConvertBack("14:45:00", typeof(DateTime), null, CultureInfo.InvariantCulture);

		Assert.That(result, Is.InstanceOf<DateTime>());
		Assert.That(((DateTime)result!).Date, Is.EqualTo(BoundDateTime.Date));
		Assert.That(((DateTime)result!).TimeOfDay, Is.EqualTo(new TimeSpan(14, 45, 0)));
	}

	[Test, Description("The date branch of the import still round trips its text")]
	public void ConvertingADateTimeReturnsItsTimeText()
	{
		DateTimeValueConverter converter = CreateConverterHolding(BoundDateTime);

		object? text = converter.Convert(BoundDateTime, typeof(string), null, CultureInfo.InvariantCulture);

		Assert.That(text, Is.EqualTo("8:30:00"));
	}
}
