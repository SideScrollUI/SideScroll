using SideScroll.Extensions;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;

namespace SideScroll.Core;

public enum SeriesType
{
	Average,
	Sum,
	Count,
	Minimum,
	Maximum,
	Other,
}

public class ListSeries
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public Dictionary<string, string> Tags { get; set; } = []; // todo: next schema change, replace with TagCollection

	public IList List; // List to start with, any elements added will also trigger an event to add new points

	public PropertyInfo? XPropertyInfo; // optional
	public PropertyInfo? YPropertyInfo; // optional

	public string? XLabel;
	public string? YLabel;

	public double XBinSize;

	public TimeSpan? PeriodDuration { get; set; }
	public SeriesType SeriesType { get; set; } = SeriesType.Sum;
	public double? Total { get; set; }

	// Visual
	public Color? Color { get; set; }
	public double? MarkerSize { get; set; } // LiveCharts includes diameter, OxyPlot treats as radius?
	public double StrokeThickness { get; set; } = 2;

	public override string ToString() => $"{Name}[{List?.Count}]";

	public ListSeries(IList list)
	{
		LoadList(list);
	}

	public ListSeries(string? name, IList list)
	{
		Name = name;
		LoadList(list);
	}

	public ListSeries(IList list, PropertyInfo xPropertyInfo, PropertyInfo yPropertyInfo)
	{
		List = list;
		XPropertyInfo = xPropertyInfo;
		YPropertyInfo = yPropertyInfo;

		Name = yPropertyInfo.Name.WordSpaced();
		NameAttribute? attribute = yPropertyInfo.GetCustomAttribute<NameAttribute>();
		if (attribute != null)
			Name = attribute.Name;
	}

	public ListSeries(string? name, IList list, string? xPropertyName, string? yPropertyName = null, SeriesType seriesType = SeriesType.Sum)
	{
		Name = name;
		List = list;
		SeriesType = seriesType;

		Type elementType = list.GetType().GetElementTypeForAll()!;
		if (xPropertyName != null)
		{
			XPropertyInfo = elementType.GetProperty(xPropertyName);
		}
		else
		{
			XPropertyInfo = elementType.GetPropertyWithAttribute<XAxisAttribute>();
		}

		if (yPropertyName != null)
		{
			YPropertyInfo = elementType.GetProperty(yPropertyName);
		}
		else
		{
			YPropertyInfo = elementType.GetPropertyWithAttribute<YAxisAttribute>();
		}
	}

	[MemberNotNull(nameof(List))]
	private void LoadList(IList list)
	{
		List = list;

		if (list == null)
			return;

		Type elementType = list.GetType().GetElementTypeForAll()!;
		XPropertyInfo = elementType.GetPropertyWithAttribute<XAxisAttribute>();
		YPropertyInfo = elementType.GetPropertyWithAttribute<YAxisAttribute>();
	}

	private static double GetObjectValue(object obj)
	{
		double value = Convert.ToDouble(obj);
		if (double.IsNaN(value))
			return 0;

		return value;
	}

	public double? CalculateTotal(TimeWindow? timeWindow = null)
	{
		timeWindow = timeWindow?.Selection ?? timeWindow;
		Total = GetTotal(timeWindow);
		if (Total.HasValue && Total > 50)
		{
			Total = Math.Floor(Total!.Value);
		}
		return Total;
	}

	public double? GetTotal(TimeWindow? timeWindow)
	{
		var timeRangeValues = TimeRangeValues;
		if (timeWindow == null || PeriodDuration == null || timeRangeValues == null)
			return GetTotal();

		return SeriesType switch
		{
			SeriesType.Count => TimeRangePeriod.TotalCounts(timeRangeValues, timeWindow, PeriodDuration.Value),
			SeriesType.Sum => TimeRangePeriod.TotalSum(timeRangeValues, timeWindow, PeriodDuration.Value),
			SeriesType.Minimum => TimeRangePeriod.TotalMinimum(timeRangeValues, timeWindow),
			SeriesType.Maximum => TimeRangePeriod.TotalMaximum(timeRangeValues, timeWindow),
			_ => TimeRangePeriod.TotalAverage(timeRangeValues, timeWindow, PeriodDuration.Value),
		};
	}

	public double? GetTotal()
	{
		return SeriesType switch
		{
			SeriesType.Count => List.Count,
			SeriesType.Average => Values().Average(),
			SeriesType.Minimum => Values().Min(),
			SeriesType.Maximum => Values().Max(),
			SeriesType.Sum => Values().Sum(),
			_ => null,
		};
	}

	public IEnumerable<double> Values()
	{
		if (YPropertyInfo != null)
		{
			foreach (object obj in List)
			{
				object? value = YPropertyInfo.GetValue(obj);
				if (value != null)
					yield return GetObjectValue(value);
			}
		}
		else
		{
			foreach (object obj in List)
			{
				double value = GetObjectValue(obj);
				yield return value;
			}
		}
	}

	public List<TimeRangeValue>? GroupByPeriod(TimeWindow timeWindow)
	{
		var timeRangeValues = TimeRangeValues;
		if (timeWindow == null || PeriodDuration == null || timeRangeValues == null)
			return timeRangeValues;

		return SeriesType switch
		{
			SeriesType.Sum => TimeRangePeriod.PeriodSums(timeRangeValues, timeWindow, PeriodDuration.Value),
			SeriesType.Minimum => TimeRangePeriod.PeriodMins(timeRangeValues, timeWindow, PeriodDuration.Value),
			SeriesType.Maximum => TimeRangePeriod.PeriodMaxes(timeRangeValues, timeWindow, PeriodDuration.Value),
			_ => TimeRangePeriod.PeriodAverages(timeRangeValues, timeWindow, PeriodDuration.Value),
		};
	}

	public List<TimeRangeValue>? TimeRangeValues
	{
		get
		{
			if (XPropertyInfo?.PropertyType != typeof(DateTime))
				return null;

			var timeRangeValues = new List<TimeRangeValue>();
			foreach (object obj in List)
			{
				if (obj is TimeRangeValue timeRangeValue)
				{
					timeRangeValues.Add(timeRangeValue);
					continue;
				}

				DateTime timeStamp = (DateTime)XPropertyInfo.GetValue(obj)!;
				double value = 1;
				if (YPropertyInfo != null)
				{
					object yObj = YPropertyInfo.GetValue(obj)!;
					value = Convert.ToDouble(yObj);
				}
				DateTime startTime = timeStamp;
				DateTime endTime = startTime.Add(PeriodDuration ?? TimeSpan.Zero);
				timeRangeValue = new TimeRangeValue(startTime, endTime, value);
				timeRangeValues.Add(timeRangeValue);
			}
			var ordered = timeRangeValues.OrderBy(t => t.StartTime).ToList();
			return ordered;
		}
	}

	public TimeWindow GetTimeWindow()
	{
		DateTime startTime = DateTime.MaxValue;
		DateTime endTime = DateTime.MinValue;
		foreach (TimeRangeValue timeRangeValue in TimeRangeValues!)
		{
			startTime = startTime.Min(timeRangeValue.StartTime);
			endTime = endTime.Max(timeRangeValue.EndTime);
		}
		return new TimeWindow(startTime, endTime);
	}
}
