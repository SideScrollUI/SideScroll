using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;

namespace SideScroll.Avalonia.Charts.LiveCharts;

public class LiveChartPoint : ObservablePoint
{
	public object? Object { get; set; }

	public LiveChartPoint(object? obj, double? x, double? y, double? yCoordinate) : base(x, y)
	{
		Object = obj;
		if (x != null && yCoordinate != null)
		{
			Coordinate = y is null ? Coordinate.Empty : new(x.Value, yCoordinate.Value);
		}
	}
}

public class LiveChartLineSeries(LiveChartSeries liveChartSeries) : LineSeries<LiveChartPoint>, ISeries
{
	public LiveChartSeries LiveChartSeries => liveChartSeries;

	public override string? ToString() => LiveChartSeries.ToString();

	public new IEnumerable<ChartPoint> Fetch(IChart chart) => base.Fetch(chart);

	IEnumerable<ChartPoint> ISeries.FindHitPoints(IChart chart, LvcPoint pointerPosition, TooltipFindingStrategy strategy)
	{
		return FindHitPoints(chart, pointerPosition, LiveChartSeries.Chart.MaxFindDistance);
	}

	IEnumerable<ChartPoint> FindHitPoints(IChart chart, LvcPoint pointerPosition, double maxDistance)
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
		if (target.Context.Chart is not ICartesianChartView<SkiaSharpDrawingContext> cartesianChart)
		{
			throw new NotImplementedException();
		}

		var cartesianSeries = (ICartesianSeries<SkiaSharpDrawingContext>)target.Context.Series;

		// Can be empty when loading
		var yAxes = cartesianChart.Core.YAxes;
		if (yAxes.Length <= cartesianSeries.ScalesYAt)
			return double.NaN;

		var xAxes = cartesianChart.Core.XAxes;
		if (xAxes.Length <= cartesianSeries.ScalesXAt)
			return double.NaN;

		var primaryAxis = yAxes[cartesianSeries.ScalesYAt];
		var secondaryAxis = xAxes[cartesianSeries.ScalesXAt];

		var drawLocation = cartesianChart.Core.DrawMarginLocation;
		var drawMarginSize = cartesianChart.Core.DrawMarginSize;

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
