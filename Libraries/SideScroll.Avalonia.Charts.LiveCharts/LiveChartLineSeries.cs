using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace SideScroll.Avalonia.Charts.LiveCharts;

/// <summary>
/// An <see cref="ObservablePoint"/> that carries a reference to the original source object and supports an optional
/// log-scale Y coordinate distinct from the displayed Y value.
/// </summary>
public class LiveChartPoint : ObservablePoint
{
	/// <summary>Gets the original source data object this point was created from.</summary>
	public object? Object { get; }

	/// <summary>
	/// Initializes a new chart point with an optional log-scale coordinate override.
	/// </summary>
	public LiveChartPoint(object? obj, double? x, double? y, double? yCoordinate) : base(x, y)
	{
		Object = obj;
		if (x == null) return;

		if (yCoordinate != null)
		{
			Coordinate = new(x.Value, yCoordinate.Value);
		}
		else if (y != null)
		{
			Coordinate = new(x.Value, y.Value);
		}
	}

	public override string ToString() => $"{X}: {Y}";
}

/// <summary>
/// A LiveCharts <see cref="LineSeries{TModel}"/> that uses <see cref="LiveChartPoint"/> as its data model and
/// overrides hit-testing to find the nearest point within a configurable pixel distance.
/// </summary>
public class LiveChartLineSeries(LiveChartSeries liveChartSeries) : LineSeries<LiveChartPoint>, ISeries
{
	/// <summary>Gets the <see cref="SideScroll.Avalonia.Charts.LiveCharts.LiveChartSeries"/> that owns this native series.</summary>
	public LiveChartSeries LiveChartSeries => liveChartSeries;

	public override string? ToString() => LiveChartSeries.ToString();

	/// <summary>Exposes the base <c>Fetch</c> implementation publicly so the tooltip and hit-test logic can enumerate rendered points.</summary>
	public new IEnumerable<ChartPoint> Fetch(Chart chart) => base.Fetch(chart);

	IEnumerable<ChartPoint> ISeries.FindHitPoints(Chart chart, LvcPoint pointerPosition, FindingStrategy strategy, FindPointFor findPointFor)
	{
		return FindHitPoints(chart, pointerPosition, LiveChartSeries.Chart.MaxFindDistance);
	}

	IEnumerable<ChartPoint> FindHitPoints(Chart chart, LvcPoint pointerPosition, double maxDistance)
	{
		if (!IsVisible) return [];

		return Fetch(chart)
			.Select(x => new { distance = GetDistanceTo(x, pointerPosition), point = x })
			.Where(x => x.distance < maxDistance)
			.OrderBy(x => x.distance)
			.SelectFirst(x => x.point);
	}

	/// <summary>Calculates the pixel distance between a chart point and a screen location using the chart's current axis scale.</summary>
	public static double GetDistanceTo(ChartPoint target, LvcPoint location)
	{
		if (target.Context.Chart is not CartesianChart cartesianChart)
		{
			throw new NotImplementedException();
		}

		var cartesianSeries = (ICartesianSeries)target.Context.Series;
		var cartesianChartView = (ICartesianChartView)cartesianChart.CoreChart.View;

		var primaryAxis = cartesianChartView.Core.YAxes[cartesianSeries.ScalesYAt];
		var secondaryAxis = cartesianChartView.Core.XAxes[cartesianSeries.ScalesXAt];

		var drawLocation = cartesianChartView.Core.DrawMarginLocation;
		var drawMarginSize = cartesianChartView.Core.DrawMarginSize;

		var secondaryScale = new Scaler(drawLocation, drawMarginSize, secondaryAxis);
		var primaryScale = new Scaler(drawLocation, drawMarginSize, primaryAxis);

		var coordinate = target.Coordinate;

		double x = secondaryScale.ToPixels(coordinate.SecondaryValue);
		double y = primaryScale.ToPixels(coordinate.PrimaryValue);

		// calculate the distance
		var dx = location.X - x;
		var dy = location.Y - y;

		double distance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
		return distance;
	}
}
