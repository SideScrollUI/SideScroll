using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using NUnit.Framework;
using System;
using System.Globalization;

namespace Atlas.UI.Avalonia.Test;

public class TestAvaloniaFieldValueConverter
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void ConvertDateTime()
	{
		DateTime dateTime = new(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc);
		var converter = new FormatValueConverter();
		string converted = (string)converter.Convert(dateTime, typeof(string), null, CultureInfo.CurrentCulture)!;
		DateTime processedDateTime = (DateTime)converter.Convert(converted, typeof(DateTime), null, CultureInfo.CurrentCulture)!;
		Assert.AreEqual(dateTime, processedDateTime);
	}
}
