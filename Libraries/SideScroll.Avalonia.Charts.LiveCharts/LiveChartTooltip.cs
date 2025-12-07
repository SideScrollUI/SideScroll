using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;
using LiveChartsCore.Kernel;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Drawing.Layouts;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SideScroll.Extensions;
using SkiaSharp;

namespace SideScroll.Avalonia.Charts.LiveCharts;

// Based on LiveCharts Tooltip
public class LiveChartTooltip(TabLiveChart liveChart) : SKDefaultTooltip
{
	public TabLiveChart LiveChart => liveChart;

	public double TextSize { get; set; } = 15;
	public float LabelMaxWidth { get; set; } = 310;

	internal StackLayout? _layout;

	public Paint? FontPaint { get; set; }

	private bool _animationAdded;

	private ChartPoint? _closestPoint;

	// Override so we can delay adding the Animation so it doesn't start from the top left
	protected override void Initialize(Chart chart)
	{
		var theme = chart.GetTheme();

		var backgroundPaint =
			chart.View.TooltipBackgroundPaint ??
			theme.TooltipBackgroundPaint ??
			new SolidColorPaint(new SKColor(235, 235, 235, 230))
			{
				ImageFilter = new DropShadow(2, 2, 6, 6, new SKColor(50, 0, 0, 100))
			};

		TextSize = chart.View.TooltipTextSize;
		if (chart.View.TooltipTextPaint is Paint toolTipTextPaint) FontPaint = toolTipTextPaint;

		Geometry.Wedge = Wedge;
		Geometry.WedgeThickness = 3;

		Geometry.Fill = backgroundPaint;
	}

	// The GetLayout method is used to define the content of the tooltip,
	// It is called every time the tooltip changes.
	protected override Layout<SkiaSharpDrawingContext> GetLayout(IEnumerable<ChartPoint> foundPoints, Chart chart)
	{
		_layout ??= new StackLayout
		{
			Orientation = ContainerOrientation.Vertical,
			HorizontalAlignment = Align.Middle,
			VerticalAlignment = Align.Middle,
		};

		foreach (var child in _layout.Children.ToArray())
		{
			_ = _layout.Children.Remove(child);
		}

		var tableLayout = new TableLayout
		{
			HorizontalAlignment = Align.Middle,
			VerticalAlignment = Align.Middle
		};

		if (LiveChart.CursorPosition == null || !foundPoints.Any()) return _layout;
		var cursorPosition = LiveChart.CursorPosition.Value;
		var cursorPoint = new LvcPoint(cursorPosition.X, cursorPosition.Y);

		ChartPoint closestPoint = foundPoints
			.Select(x => new { distance = LiveChartLineSeries.GetDistanceTo(x, cursorPoint), point = x })
			.MinBy(x => x.distance)!
			.point;
		_closestPoint = closestPoint;

		// Points are in chart series order, not closest
		// Use pointer moved value in chart to find closest?
		if (closestPoint.Context.Series is LiveChartLineSeries lineSeries)
		{
			int row = 0;
			string? title = lineSeries.LiveChartSeries.GetTooltipTitle();
			if (title != null)
			{
				tableLayout.AddChild(
					new LabelGeometry
					{
						Text = title,
						Paint = FontPaint,
						TextSize = (float)TextSize,
						Padding = new Padding(0, 0, 0, 4),
						MaxWidth = LabelMaxWidth,
						VerticalAlign = Align.Start,
						HorizontalAlign = Align.Start,
					}, row++, 0, horizontalAlign: Align.Start);
			}

			var lines = lineSeries.LiveChartSeries.GetTooltipLines(closestPoint);

			foreach (string line in lines)
			{
				if (!line.IsNullOrEmpty())
				{
					tableLayout.AddChild(
						new LabelGeometry
						{
							Text = line,
							Paint = FontPaint,
							TextSize = (float)TextSize,
							Padding = new Padding(0, 3, 0, 0),
							MaxWidth = LabelMaxWidth,
							VerticalAlign = Align.Start,
							HorizontalAlign = Align.Start,
						}, row++, 0, horizontalAlign: Align.Start);
				}
				else
				{
					tableLayout.AddChild(
						new StackLayout { Padding = new(0, 8) }, row++, 0);
				}
			}

			// todo: After Tooltip clipping issue fixed:
			// Switch to showing all series, or all with a data point present for that X value
			/*var series = (IChartSeries)point.Context.Series;

			tableLayout.AddChild(series.GetMiniaturesSketch().AsDrawnControl(s_zIndex), i, 0);

			tableLayout.AddChild(
				new LabelGeometry
				{
					Text = point.Context.Series.Name ?? string.Empty,
					Paint = FontPaint,
					TextSize = TextSize,
					Padding = new Padding(10, 0, 0, 0),
					MaxWidth = lw,
					VerticalAlign = Align.Start,
					HorizontalAlign = Align.Start,
				}, i, 1, horizontalAlign: Align.Start);

			tableLayout.AddChild(
				new LabelGeometry
				{
					Text = content,
					Paint = FontPaint,
					TextSize = TextSize,
					Padding = new Padding(10, 0, 0, 0),
					MaxWidth = lw,
					VerticalAlign = Align.Start,
					HorizontalAlign = Align.Start,
				}, i, 2, horizontalAlign: Align.End);
			*/
		}

		_layout.Children.Add(tableLayout);

		var pointArray = new ChartPoint[] { closestPoint };
		var size = _layout.Measure();
		_ = pointArray.GetTooltipLocation(size, chart);
		Geometry.Placement = chart.AutoToolTipsInfo.ToolTipPlacement;

		const int px = 8;
		const int py = 12;

		switch (chart.AutoToolTipsInfo.ToolTipPlacement)
		{
			case LiveChartsCore.Measure.PopUpPlacement.Top:
				_layout.Padding = new Padding(py, px, py, px + Wedge); break;
			case LiveChartsCore.Measure.PopUpPlacement.Bottom:
				_layout.Padding = new Padding(py, px + Wedge, py, px); break;
			case LiveChartsCore.Measure.PopUpPlacement.Left:
				_layout.Padding = new Padding(py, px, py + Wedge, px); break;
			case LiveChartsCore.Measure.PopUpPlacement.Right:
				_layout.Padding = new Padding(py + Wedge, px, py, px); break;
			default: break;
		}
		return _layout;
	}

    public override void Show(IEnumerable<ChartPoint> foundPoints, Chart chart)
	{
		bool wasHidden = Opacity < 1;

		base.Show(foundPoints, chart);

		// Write code here to add custom behavior when the tooltip is shown.

		// Update for new padding
		var size = Measure();
		var pointArray = new ChartPoint[] { _closestPoint! };

		var location = pointArray.GetTooltipLocation(size, chart);

		X = location.X;
		Y = location.Y;

		if (wasHidden)
		{
			// Jump to a new location if visibility changes
			CompleteTransition();
		}

		// Wait to add the animation so the ToolTip doesn't start animating from the top left
		if (!_animationAdded)
		{
			this.Animate(
				new Animation(Easing, AnimationsSpeed),
					// OpacityProperty, // Too distracting
					ScaleTransformProperty,
					XProperty,
					YProperty);
			_animationAdded = true;
		}
	}

	public override void Hide(Chart chart)
	{
		base.Hide(chart);

		// Write code here to add custom behavior when the tooltip is hidden.
	}
}
