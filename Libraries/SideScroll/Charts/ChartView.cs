using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Time;
using System.Collections;
using System.Drawing;
using System.Reflection;

namespace SideScroll.Charts;

/// <summary>
/// Represents a visual annotation on a chart such as a line or marker
/// </summary>
public class ChartAnnotation
{
	/// <summary>
	/// Gets or sets the annotation text label
	/// </summary>
	public string? Text { get; set; }

	//public Color? TextColor { get; set; }
	/// <summary>
	/// Gets or sets the annotation color used for the line and text label
	/// </summary>
	public Color? Color { get; set; }

	/// <summary>
	/// Gets or sets the X-axis position of the annotation
	/// </summary>
	public double? X { get; set; }

	/// <summary>
	/// Gets or sets the Y-axis position of the annotation
	/// </summary>
	public double? Y { get; set; }

	/// <summary>
	/// Gets or sets the line stroke thickness
	/// </summary>
	public double StrokeThickness { get; set; } = 2;
}

/// <summary>
/// Defines the position of the chart legend
/// </summary>
public enum ChartLegendPosition
{
	/// <summary>
	/// Legend is hidden
	/// </summary>
	Hidden,

	/// <summary>
	/// Legend is positioned on the right side
	/// </summary>
	Right,

	/// <summary>
	/// Legend is positioned at the bottom
	/// </summary>
	Bottom,
}

/// <summary>
/// Event arguments for when chart series are selected
/// </summary>
public class SeriesSelectedEventArgs(List<ListSeries> series) : EventArgs
{
	/// <summary>
	/// Gets the list of selected series
	/// </summary>
	public List<ListSeries> Series => series;
}

/// <summary>
/// Represents a chart configuration with multiple data series, annotations, and display settings
/// </summary>
public class ChartView
{
	/// <summary>
	/// Gets or sets the chart name
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the legend position
	/// </summary>
	public ChartLegendPosition LegendPosition { get; set; } = ChartLegendPosition.Right;

	/// <summary>
	/// Gets or sets the legend title
	/// </summary>
	public string? LegendTitle { get; set; }

	/// <summary>
	/// Gets or sets whether to show series order numbers
	/// </summary>
	public bool ShowOrder { get; set; } = true;

	/// <summary>
	/// Gets or sets whether to show the current time marker (for time series only)
	/// </summary>
	public bool ShowNowTime { get; set; } = true; // Time Series only

	/// <summary>
	/// Gets or sets whether to show the time tracker (for time series only)
	/// </summary>
	public bool ShowTimeTracker { get; set; } // Time Series only

	// public bool IsStacked { get; set; }

	/// <summary>
	/// Gets or sets the logarithmic scale base for the Y-axis
	/// </summary>
	public double? LogBase { get; set; }

	/// <summary>
	/// Gets or sets the minimum Y-axis value
	/// </summary>
	public double? MinValue { get; set; }

	/// <summary>
	/// Gets or sets the bin size for X-axis grouping
	/// </summary>
	public double XBinSize { get; set; }

	/// <summary>
	/// Gets or sets the time window for displaying time series data
	/// </summary>
	public TimeWindow? TimeWindow { get; set; }

	/// <summary>
	/// Gets or sets the default period duration for aggregating time series data
	/// </summary>
	public TimeSpan? DefaultPeriodDuration { get; set; }

	/// <summary>
	/// Gets or sets the list of data series to display
	/// </summary>
	public List<ListSeries> Series { get; set; } = [];

	/// <summary>
	/// Gets or sets the maximum number of series to display
	/// </summary>
	public int SeriesLimit { get; set; } = 25;

	/// <summary>
	/// Gets or sets the list of chart annotations
	/// </summary>
	public List<ChartAnnotation> Annotations { get; set; } = [];

	/// <summary>
	/// Gets or sets the source list for dimension-based series
	/// </summary>
	public IList? SourceList { get; set; }

	private Dictionary<string, IList> _dimensions = [];
	private List<PropertyInfo> _dimensionPropertyInfos = [];

	private string? _xPropertyName;
	private string? _yPropertyName;

	/// <summary>
	/// Occurs when the chart series selection changes
	/// This can occur when a user selects a line, chart background, or legend item
	/// </summary>
	public event EventHandler<SeriesSelectedEventArgs>? SelectionChanged;

	public override string? ToString() => Name;

	/// <summary>
	/// Initializes a new instance of the ChartView class
	/// </summary>
	public ChartView(string? name = null, TimeWindow? timeWindow = null)
	{
		Name = name;
		TimeWindow = timeWindow;
	}

	/// <summary>
	/// Initializes a new instance of the ChartView class with a time range
	/// </summary>
	public ChartView(string? name, DateTime startTime, DateTime endTime)
	{
		Name = name;
		TimeWindow = new TimeWindow(startTime, endTime);
	}

	/// <summary>
	/// Adds a new data series to the chart
	/// </summary>
	public ListSeries AddSeries(string name, IList list, string? xPropertyName = null, string? yPropertyName = null, SeriesType seriesType = SeriesType.Sum)
	{
		var series = new ListSeries(name, list, xPropertyName, yPropertyName, seriesType)
		{
			PeriodDuration = DefaultPeriodDuration,
		};
		Series.Add(series);
		return series;
	}

	/// <summary>
	/// Creates multiple series by grouping data by the specified dimension properties
	/// </summary>
	/// <param name="iList">The source data list</param>
	/// <param name="xPropertyName">The property name for X-axis values</param>
	/// <param name="yPropertyName">The property name for Y-axis values</param>
	/// <param name="dimensionPropertyNames">Property names to group by, creating separate series for each unique combination</param>
	public void AddDimensions(IList iList, string xPropertyName, string yPropertyName, params string[] dimensionPropertyNames)
	{
		SourceList = iList;
		_xPropertyName = xPropertyName;
		_yPropertyName = yPropertyName;

		Type elementType = iList.GetType().GetElementTypeForAll()!;

		_dimensionPropertyInfos = dimensionPropertyNames
			.Select(name => elementType.GetProperty(name)!)
			.ToList();

		_dimensions = [];
		foreach (var obj in iList)
		{
			AddDimensionValue(obj);
		}
		SortByTotal();
	}

	/// <summary>
	/// Adds a data value to the appropriate dimension-based series
	/// </summary>
	public void AddDimensionValue(object? obj)
	{
		var values = _dimensionPropertyInfos.Select(propertyInfo => propertyInfo.GetValue(obj)!).ToList();
		if (values.Count == 0) return;

		string name = string.Join(" - ", values);

		if (!_dimensions.TryGetValue(name, out IList? dimensionList))
		{
			dimensionList = (IList)Activator.CreateInstance(SourceList!.GetType())!;
			_dimensions.Add(name, dimensionList);

			var listSeries = new ListSeries(name, dimensionList, _xPropertyName, _yPropertyName)
			{
				XBinSize = XBinSize,
				PeriodDuration = DefaultPeriodDuration,
			};
			Series.Add(listSeries);
		}
		dimensionList.Add(obj);
	}

	/// <summary>
	/// Sorts the series in descending order by their total values
	/// </summary>
	public void SortByTotal()
	{
		var timeWindow = TimeWindow;
		int averageCount = Series.Count(s => s.SeriesType == SeriesType.Average);
		if (averageCount == Series.Count)
		{
			// Use the Min/Max times from the series data points (should this be configurable?)
			timeWindow = GetSeriesTimeWindow();
		}

		var orderedSeries = Series.OrderByDescending(series => series.CalculateTotal(timeWindow));

		Series = [.. orderedSeries];
	}

	/// <summary>
	/// Gets the time window that encompasses all series data points
	/// </summary>
	public TimeWindow GetSeriesTimeWindow()
	{
		DateTime startTime = DateTime.MaxValue;
		DateTime endTime = DateTime.MinValue;
		foreach (ListSeries listSeries in Series)
		{
			TimeWindow seriesWindow = listSeries.GetTimeWindow();
			startTime = startTime.Min(seriesWindow.StartTime);
			endTime = endTime.Max(seriesWindow.EndTime);
		}
		return new TimeWindow(startTime, endTime);
	}

	/// <summary>
	/// Raises the SelectionChanged event
	/// </summary>
	public void OnSelectionChanged(SeriesSelectedEventArgs e)
	{
		SelectionChanged?.Invoke(this, e);
	}
}
