using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using SideScroll.Extensions;

namespace SideScroll.Avalonia.Charts.LiveCharts;

// Based on LiveCharts Tooltip
public class LiveChartTooltip(TabLiveChart liveChart) : IChartTooltip<SkiaSharpDrawingContext>
{
	public TabLiveChart LiveChart => liveChart;

	public double TextSize { get; set; } = 15;
	public float LabelMaxWidth { get; set; } = 310;

	private static readonly int s_zIndex = 10100;

	internal StackPanel<PopUpGeometry, SkiaSharpDrawingContext>? _panel;
	private IPaint<SkiaSharpDrawingContext>? _backgroundPaint;

	public IPaint<SkiaSharpDrawingContext>? FontPaint { get; set; }

	public IPaint<SkiaSharpDrawingContext>? BackgroundPaint
	{
		get => _backgroundPaint;
		set
		{
			_backgroundPaint = value;
			if (value is not null)
			{
				value.IsFill = true;
			}
		}
	}

	public void Show(IEnumerable<ChartPoint> foundPoints, Chart<SkiaSharpDrawingContext> chart)
	{
		const int wedge = 10;

		if (chart.View.TooltipTextSize is not null) TextSize = chart.View.TooltipTextSize.Value;
		if (chart.View.TooltipBackgroundPaint is not null) BackgroundPaint = chart.View.TooltipBackgroundPaint;
		if (chart.View.TooltipTextPaint is not null) FontPaint = chart.View.TooltipTextPaint;

		bool addAnimation = false;
		if (_panel is null)
		{
			_panel = new StackPanel<PopUpGeometry, SkiaSharpDrawingContext>
			{
				Orientation = ContainerOrientation.Vertical,
				HorizontalAlignment = Align.Middle,
				VerticalAlignment = Align.Middle,
				BackgroundPaint = BackgroundPaint
			};

			_panel.BackgroundGeometry.Wedge = wedge;
			_panel.BackgroundGeometry.WedgeThickness = 3;

			addAnimation = true;
		}

		if (BackgroundPaint is not null) BackgroundPaint.ZIndex = s_zIndex;
		if (FontPaint is not null) FontPaint.ZIndex = s_zIndex + 1;

		foreach (var child in _panel.Children.ToArray())
		{
			_ = _panel.Children.Remove(child);
			chart.RemoveVisual(child);
		}

		var tableLayout = new TableLayout<RoundedRectangleGeometry, SkiaSharpDrawingContext>
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
					new LabelVisual
					{
						Text = title,
						Paint = FontPaint,
						TextSize = TextSize,
						Padding = new Padding(0, 0, 0, 4),
						MaxWidth = LabelMaxWidth,
						VerticalAlignment = Align.Start,
						HorizontalAlignment = Align.Start,
						ClippingMode = LiveChartsCore.Measure.ClipMode.XY
					}, row++, 0, horizontalAlign: Align.Start);
			}

			var lines = lineSeries.LiveChartSeries.GetTooltipLines(closestPoint);

			foreach (string line in lines)
			{
				if (!line.IsNullOrEmpty())
				{
					tableLayout.AddChild(
						new LabelVisual
						{
							Text = line,
							Paint = FontPaint,
							TextSize = TextSize,
							Padding = new Padding(0, 3, 0, 0),
							MaxWidth = LabelMaxWidth,
							VerticalAlignment = Align.Start,
							HorizontalAlignment = Align.Start,
							ClippingMode = LiveChartsCore.Measure.ClipMode.None
						}, row++, 0, horizontalAlign: Align.Start);
				}
				else
				{
					tableLayout.AddChild(
						new StackPanel<RectangleGeometry, SkiaSharpDrawingContext> { Padding = new(0, 8) }, row++, 0);
				}
			}

			// todo: After Tooltip clipping issue fixed:
			// Switch to showing all series, or all with a data point present for that X value
			/*var series = (IChartSeries<SkiaSharpDrawingContext>)point.Context.Series;

			tableLayout.AddChild(series.GetMiniaturesSketch().AsDrawnControl(s_zIndex), i, 0);

			tableLayout.AddChild(
				new LabelVisual
				{
					Text = point.Context.Series.Name ?? string.Empty,
					Paint = FontPaint,
					TextSize = TextSize,
					Padding = new Padding(10, 0, 0, 0),
					MaxWidth = lw,
					VerticalAlignment = Align.Start,
					HorizontalAlignment = Align.Start,
					ClippingMode = LiveChartsCore.Measure.ClipMode.None
				}, i, 1, horizontalAlign: Align.Start);

			tableLayout.AddChild(
				new LabelVisual
				{
					Text = content,
					Paint = FontPaint,
					TextSize = TextSize,
					Padding = new Padding(10, 0, 0, 0),
					MaxWidth = lw,
					VerticalAlignment = Align.Start,
					HorizontalAlignment = Align.Start,
					ClippingMode = LiveChartsCore.Measure.ClipMode.None
				}, i, 2, horizontalAlign: Align.End);
			*/
		}

		_panel.Children.Add(tableLayout);

		var size = _panel.Measure(chart);
		_ = foundPoints.GetTooltipLocation(size, chart);
		_panel.BackgroundGeometry.Placement = chart.AutoToolTipsInfo.ToolTipPlacement;

		switch (chart.AutoToolTipsInfo.ToolTipPlacement)
		{
			case LiveChartsCore.Measure.PopUpPlacement.Top:
				_panel.Padding = new Padding(12, 8, 12, 8 + wedge); break;
			case LiveChartsCore.Measure.PopUpPlacement.Bottom:
				_panel.Padding = new Padding(12, 8 + wedge, 12, 8); break;
			case LiveChartsCore.Measure.PopUpPlacement.Left:
				_panel.Padding = new Padding(12, 8, 12 + wedge, 8); break;
			case LiveChartsCore.Measure.PopUpPlacement.Right:
				_panel.Padding = new Padding(12 + wedge, 8, 12, 8); break;
			default: break;
		}

		// Update for new padding
		size = _panel.Measure(chart);
		var location = foundPoints.GetTooltipLocation(size, chart);

		_panel.X = location.X;
		_panel.Y = location.Y;

		chart.AddVisual(_panel);

		// Wait to add the animation so the ToolTip doesn't start animating from the top left
		if (addAnimation)
		{
			_panel
				.Animate(
					new Animation(EasingFunctions.EaseOut, TimeSpan.FromMilliseconds(150)),
					nameof(RoundedRectangleGeometry.X),
					nameof(RoundedRectangleGeometry.Y));
		}
	}

	public void Hide(Chart<SkiaSharpDrawingContext> chart)
	{
		if (chart is null || _panel is null) return;

		chart.RemoveVisual(_panel);
	}
}
