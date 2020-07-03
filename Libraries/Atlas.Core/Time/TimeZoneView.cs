using System;
using System.Collections.Generic;

namespace Atlas.Core.Time
{
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

		public override string ToString()
		{
			if (Abbreviation == Name)
				return Abbreviation;
			return Abbreviation + " - " + Name;
		}

		public DateTime ConvertTimeToUtc(DateTime dateTime)
		{
			return TimeZoneInfo.ConvertTimeToUtc(dateTime);
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
			new TimeZoneView("PDT", "Pacific Standard Time", TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")),
			new TimeZoneView("EST", "Eastern Standard Time", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
			new TimeZoneView("SGT", "Singapore Time", TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time")),
		};
	}

	// https://en.wikipedia.org/wiki/List_of_time_zone_abbreviations
}
