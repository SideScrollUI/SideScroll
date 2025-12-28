using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Time;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;

namespace SideScroll.Collections;

/// <summary>
/// Defines the aggregation method for series data
/// </summary>
public enum SeriesType
{
	/// <summary>
	/// Calculate the average value
	/// </summary>
	Average,

	/// <summary>
	/// Calculate the sum of values
	/// </summary>
	Sum,

	/// <summary>
	/// Count the number of items
	/// </summary>
	Count,

	/// <summary>
	/// Find the minimum value
	/// </summary>
	Minimum,

	/// <summary>
	/// Find the maximum value
	/// </summary>
	Maximum,

	/// <summary>
	/// Custom aggregation type
	/// </summary>
	Other,
}

/// <summary>
/// Represents a data series for charting with support for time-based aggregation and visual properties
/// </summary>
public class ListSeries
{
	/// <summary>
	/// Gets or sets the series name
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the series description
	/// </summary>
	public string? Description { get; set; }
	
	/// <summary>
	/// Gets or sets the series metadata tags
	/// </summary>
	public Dictionary<string, string> Tags { get; set; } = []; // todo: next schema change, replace with TagCollection

	/// <summary>
	/// Gets or sets the underlying data list
	/// </summary>
	public IList List { get; set; } // List to start with, any elements added will also trigger an event to add new points

	/// <summary>
	/// Gets or sets the property info for the X-axis values
	/// </summary>
	public PropertyInfo? XPropertyInfo { get; set; } // optional

	/// <summary>
	/// Gets or sets the property info for the Y-axis values
	/// </summary>
	public PropertyInfo? YPropertyInfo { get; set; } // optional

	// public string? XLabel { get; set; }
	/// <summary>
	/// Gets or sets the Y-axis label
	/// </summary>
	public string? YLabel { get; set; }

	/// <summary>
	/// Gets or sets the bin size for X-axis grouping
	/// </summary>
	public double XBinSize { get; set; }

	/// <summary>
	/// Gets or sets the time duration for each period when aggregating time-series data
	/// </summary>
	public TimeSpan? PeriodDuration { get; set; }

	/// <summary>
	/// Gets or sets the aggregation method for the series
	/// </summary>
	public SeriesType SeriesType { get; set; } = SeriesType.Sum;

	/// <summary>
	/// Gets or sets the calculated total value
	/// </summary>
	public double? Total { get; set; }

	// Visual
	/// <summary>
	/// Gets or sets the series chart color
	/// </summary>
	public Color? Color { get; set; }

	/// <summary>
	/// Gets or sets the marker size for data points
	/// </summary>
	public double? MarkerSize { get; set; } // LiveCharts includes diameter, OxyPlot treats as radius?

	/// <summary>
	/// Gets or sets the line stroke thickness
	/// </summary>
	public double StrokeThickness { get; set; } = 2;

	public override string ToString() => $"{Name}[{List?.Count}]";

	/// <summary>
	/// Initializes a new instance of the ListSeries class with the specified list
	/// </summary>
	public ListSeries(IList list)
	{
		LoadList(list);
	}

	/// <summary>
	/// Initializes a new instance of the ListSeries class with a name and list
	/// </summary>
	public ListSeries(string? name, IList list)
	{
		Name = name;
		LoadList(list);
	}

	/// <summary>
	/// Initializes a new instance of the ListSeries class with X and Y Axis PropertyInfos
	/// </summary>
	public ListSeries(IList list, PropertyInfo xPropertyInfo, PropertyInfo yPropertyInfo)
	{
		List = list;
		XPropertyInfo = xPropertyInfo;
		YPropertyInfo = yPropertyInfo;

		Name = yPropertyInfo.Name.WordSpaced();
		NameAttribute? attribute = yPropertyInfo.GetCustomAttribute<NameAttribute>();
		if (attribute != null)
		{
			Name = attribute.Name;
		}
	}

	/// <summary>
	/// Initializes a new instance of the ListSeries class with property names for axis mapping
	/// </summary>
	/// <param name="name">The series name</param>
	/// <param name="list">The data list</param>
	/// <param name="xPropertyName">The property name for X-axis values, or null to use XAxisAttribute</param>
	/// <param name="yPropertyName">The property name for Y-axis values, or null to use YAxisAttribute</param>
	/// <param name="seriesType">The aggregation method to use</param>
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

	/// <summary>
	/// Calculates and stores the total value for the series within the specified time window
	/// </summary>
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

	/// <summary>
	/// Gets the aggregated total value for the series within the specified time window
	/// </summary>
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

	/// <summary>
	/// Gets the aggregated total value for all items in the series
	/// </summary>
	public double? GetTotal()
	{
		if (List.Count == 0) return null;

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

	/// <summary>
	/// Enumerates the Y-axis values from the list
	/// </summary>
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

	/// <summary>
	/// Groups time-series data into periods based on the SeriesType aggregation method
	/// </summary>
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

	/// <summary>
	/// Gets the list data converted to time range values, ordered by start time
	/// </summary>
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

				DateTime timestamp = (DateTime)XPropertyInfo.GetValue(obj)!;
				double value = 1;
				if (YPropertyInfo != null)
				{
					object yObj = YPropertyInfo.GetValue(obj)!;
					value = Convert.ToDouble(yObj);
				}
				DateTime startTime = timestamp;
				DateTime endTime = startTime.Add(PeriodDuration ?? TimeSpan.Zero);
				timeRangeValue = new TimeRangeValue(startTime, endTime, value);
				timeRangeValues.Add(timeRangeValue);
			}
			var ordered = timeRangeValues.OrderBy(t => t.StartTime).ToList();
			return ordered;
		}
	}

	/// <summary>
	/// Gets the time window spanning all data points in the series
	/// </summary>
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
