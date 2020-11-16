using Atlas.Extensions;
using System;
using System.Collections.Generic;

namespace Atlas.Core.Time
{
	[PublicData]
	public class TimeZoneView : IComparable
	{
		public string Abbreviation { get; set; }
		public string Name { get; set; }
		public TimeZoneInfo TimeZoneInfo { get; set; }

		public TimeZoneView()
		{
		}

		public TimeZoneView(string abbreviation, string name, TimeZoneInfo timeZoneInfo)
		{
			Abbreviation = abbreviation;
			Name = name;
			TimeZoneInfo = timeZoneInfo;
		}

		public TimeZoneView(string abbreviation, string name, TimeSpan timeSpan)
		{
			Abbreviation = abbreviation;
			Name = name;
			try
			{
				TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(name);
			}
			catch (Exception)
			{
				TimeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(name, timeSpan, name, name);
			}
		}

		public override string ToString()
		{
			if (Abbreviation == Name)
				return Abbreviation;
			return Abbreviation + " - " + Name + ": " + TimeZoneInfo.BaseUtcOffset.Formatted();
		}

		public DateTime ConvertTimeToUtc(DateTime dateTime)
		{
			if (this == Utc)
				return dateTime;

			if (this == Local)
				dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
			else
				dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);

			return TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo);
		}

		public int CompareTo(object obj)
		{
			return obj.ToString().CompareTo(ToString());
		}

		public static TimeZoneView Utc = new TimeZoneView("Utc", "Utc", TimeZoneInfo.Utc);
		public static TimeZoneView Local = new TimeZoneView("Local", "Local", TimeZoneInfo.Local);

		public static List<TimeZoneView> All = new List<TimeZoneView>()
		{
			Utc,
			Local,
			new TimeZoneView("PDT", "Pacific Daylight Time", TimeSpan.FromHours(-7)),
			new TimeZoneView("PST", "Pacific Standard Time", TimeSpan.FromHours(-8)),
			new TimeZoneView("EDT", "Eastern Daylight Time", TimeSpan.FromHours(-4)),
			new TimeZoneView("EST", "Eastern Standard Time", TimeSpan.FromHours(-5)),
			new TimeZoneView("SGT", "Singapore Standard Time", TimeSpan.FromHours(8)),
		};
	}

	// https://en.wikipedia.org/wiki/List_of_time_zone_abbreviations
}
