using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using NUnit.Framework;
using System;

namespace Atlas.GUI.Avalonia.Test
{
	public class TestAvaloniaFieldValueConverter
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void ConvertDateTime()
		{
			DateTime dateTime = new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc);
			FormatValueConverter converter = new FormatValueConverter();
			string converted = (string)converter.Convert(dateTime, typeof(string), null, null);
			DateTime processedDateTime = (DateTime)converter.Convert(converted, typeof(DateTime), null, null);
			Assert.AreEqual(dateTime, processedDateTime);
		}
	}
}