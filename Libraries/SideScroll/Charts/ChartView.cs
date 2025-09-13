using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Time;
using System.Collections;
using System.Drawing;
using System.Reflection;

namespace SideScroll.Charts;

public class ChartAnnotation
{
	public string? Text { get; set; }
	//public Color? TextColor { get; set; }
	public Color? Color { get; set; }

	public bool Horizontal { get; set; } = true;

	public double? X { get; set; }
	public double? Y { get; set; }

	public double StrokeThickness { get; set; } = 2;
}

public enum ChartLegendPosition
{
	Hidden,
	Right,
	Bottom,
}

public class SeriesSelectedEventArgs(List<ListSeries> series) : EventArgs
{
	public List<ListSeries> Series => series;
}

public class ChartView
{
	public string? Name { get; set; }

	public ChartLegendPosition LegendPosition { get; set; } = ChartLegendPosition.Right;
	public string? LegendTitle { get; set; }

	public bool ShowOrder { get; set; } = true;
	public bool ShowNowTime { get; set; } = true; // Time Series only
	public bool ShowTimeTracker { get; set; } // Time Series only
	public bool IsStacked { get; set; }

	public double? LogBase { get; set; }
	public double? MinValue { get; set; }
	public double XBinSize { get; set; }

	public TimeWindow? TimeWindow { get; set; }
	public TimeSpan? DefaultPeriodDuration { get; set; }

	public List<ListSeries> Series { get; set; } = [];
	public int SeriesLimit { get; set; } = 25;

	public List<ChartAnnotation> Annotations { get; set; } = [];

	public IList? SourceList { get; set; }

	private Dictionary<string, IList> _dimensions = [];
	private List<PropertyInfo> _dimensionPropertyInfos = [];

	private string? _xPropertyName;
	private string? _yPropertyName;

	public event EventHandler<SeriesSelectedEventArgs>? SelectionChanged;

	public override string? ToString() => Name;

	public ChartView(string? name = null, TimeWindow? timeWindow = null)
	{
		Name = name;
		TimeWindow = timeWindow;
	}

	public ChartView(string? name, DateTime startTime, DateTime endTime)
	{
		Name = name;
		TimeWindow = new TimeWindow(startTime, endTime);
	}

	public ListSeries AddSeries(string name, IList list, string? xPropertyName = null, string? yPropertyName = null, SeriesType seriesType = SeriesType.Sum)
	{
		var series = new ListSeries(name, list, xPropertyName, yPropertyName, seriesType)
		{
			PeriodDuration = DefaultPeriodDuration,
		};
		Series.Add(series);
		return series;
	}

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

		Series = new List<ListSeries>(orderedSeries);
	}

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

	public void OnSelectionChanged(SeriesSelectedEventArgs e)
	{
		SelectionChanged?.Invoke(this, e);
	}
}
