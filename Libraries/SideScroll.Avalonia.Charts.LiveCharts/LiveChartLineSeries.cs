using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace SideScroll.Avalonia.Charts.LiveCharts;

public class LiveChartPoint : ObservablePoint
{
	public object? Object { get; }

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

public class LiveChartLineSeries(LiveChartSeries liveChartSeries) : LineSeries<LiveChartPoint>, ISeries
{
	public LiveChartSeries LiveChartSeries => liveChartSeries;

	public override string? ToString() => LiveChartSeries.ToString();

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
