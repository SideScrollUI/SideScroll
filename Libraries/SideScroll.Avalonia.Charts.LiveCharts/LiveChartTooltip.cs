using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Drawing.Layouts;
using SideScroll.Extensions;

namespace SideScroll.Avalonia.Charts.LiveCharts;

// Based on LiveCharts Tooltip
public class LiveChartTooltip(TabControlLiveChart liveChart) : IChartTooltip
{
	public TabControlLiveChart LiveChart => liveChart;

	public double TextSize { get; set; } = 15;
	public float LabelMaxWidth { get; set; } = 310;

	private static readonly int s_zIndex = 10100;

	internal Container<PopUpGeometry>? _container;
	internal StackLayout? _layout;
	private Paint? _backgroundPaint;

	public Paint? FontPaint { get; set; }

	public Paint? BackgroundPaint
	{
		get => _backgroundPaint;
		set
		{
			_backgroundPaint = value;
			if (value is not null)
			{
				// internal, but might not be needed anymore?
				//value.PaintStyle = PaintStyle.Fill;
			}
		}
	}

	public Func<float, float> Easing { get; set; } = EasingFunctions.EaseOut;

	public TimeSpan AnimationsSpeed { get; set; } = TimeSpan.FromMilliseconds(150);

	public void Show(IEnumerable<ChartPoint> foundPoints, Chart chart)
	{
		const int wedge = 10;

		TextSize = chart.View.TooltipTextSize;
		if (chart.View.TooltipBackgroundPaint is not null) BackgroundPaint = chart.View.TooltipBackgroundPaint;
		if (chart.View.TooltipTextPaint is not null) FontPaint = chart.View.TooltipTextPaint;

		bool addAnimation = false;
		if (_container is null || _layout is null)
		{
			_container = new Container<PopUpGeometry>
			{
				Content =_layout = new StackLayout
				{
					Orientation = ContainerOrientation.Vertical,
					HorizontalAlignment = Align.Middle,
					VerticalAlignment = Align.Middle,
				}
			};

			_container.Geometry.Fill = BackgroundPaint;
			_container.Geometry.Wedge = wedge;
			_container.Geometry.WedgeThickness = 3;

			addAnimation = true;

			var drawTask = chart.Canvas.AddGeometry(_container);
			drawTask.ZIndex = 10100;
		}

		bool wasHidden = _container.Opacity < 1;
		_container.Opacity = 1;
		_container.ScaleTransform = new LvcPoint(1, 1);

		if (BackgroundPaint is not null) BackgroundPaint.ZIndex = s_zIndex;
		if (FontPaint is not null) FontPaint.ZIndex = s_zIndex + 1;

		foreach (var child in _layout.Children.ToArray())
		{
			_ = _layout.Children.Remove(child);
		}

		var tableLayout = new TableLayout()
		{
			HorizontalAlignment = Align.Middle,
			VerticalAlignment = Align.Middle
		};

		if (LiveChart.CursorPosition == null || !foundPoints.Any()) return;
		var cursorPosition = LiveChart.CursorPosition.Value;
		var cursorPoint = new LvcPoint(cursorPosition.X, cursorPosition.Y);

		ChartPoint closestPoint = foundPoints
			.Select(x => new { distance = LiveChartLineSeries.GetDistanceTo(x, cursorPoint), point = x })
			.MinBy(x => x.distance)!
			.point;

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
		_container.Geometry.Placement = chart.AutoToolTipsInfo.ToolTipPlacement;

		const int px = 8;
		const int py = 12;

		switch (chart.AutoToolTipsInfo.ToolTipPlacement)
		{
			case LiveChartsCore.Measure.PopUpPlacement.Top:
				_layout.Padding = new Padding(py, px, py, px + wedge); break;
			case LiveChartsCore.Measure.PopUpPlacement.Bottom:
				_layout.Padding = new Padding(py, px + wedge, py, px); break;
			case LiveChartsCore.Measure.PopUpPlacement.Left:
				_layout.Padding = new Padding(py, px, py + wedge, px); break;
			case LiveChartsCore.Measure.PopUpPlacement.Right:
				_layout.Padding = new Padding(py + wedge, px, py, px); break;
			default: break;
		}

		// Update for new padding
		size = _container.Measure();
		var location = pointArray.GetTooltipLocation(size, chart);

		_container.X = location.X;
		_container.Y = location.Y;

		if (wasHidden)
		{
			// Jump to a new location if visibility changes
			_container.CompleteTransition();
		}

		// Wait to add the animation so the ToolTip doesn't start animating from the top left
		if (addAnimation)
		{
			// Follow mouse until hidden
			_container
				.Animate(
					new Animation(Easing, AnimationsSpeed),
					//nameof(IDrawnElement.Opacity),
					//nameof(IDrawnElement.ScaleTransform),
					nameof(IDrawnElement.X),
					nameof(IDrawnElement.Y));
		}
	}

	public void Hide(Chart chart)
	{
		if (chart is null || _container is null) return;

		_container.Opacity = 0f;
		_container.ScaleTransform = new LvcPoint(0.85f, 0.85f);

		chart.Canvas.Invalidate();
	}
}
