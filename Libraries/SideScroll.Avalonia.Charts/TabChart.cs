using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Extensions;
using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Utilities;
using SideScroll.Charts;
using SideScroll.Collections;
using System.Reflection;

namespace SideScroll.Avalonia.Charts;

/// <summary>
/// Pairs a <see cref="SideScroll.Charts.ListSeries"/> with its underlying chart series object and display color.
/// </summary>
public class ChartSeries<TSeries>(ListSeries listSeries, TSeries lineSeries, Color color)
{
	/// <summary>Gets the source data series.</summary>
	public ListSeries ListSeries => listSeries;
	/// <summary>Gets the native chart series used by the charting library.</summary>
	public TSeries LineSeries => lineSeries;
	/// <summary>Gets the display color for this series.</summary>
	public Color Color => color;

	/// <summary>Gets or sets whether this series is selected (visible) in the chart.</summary>
	public bool IsSelected { get; set; } = true; // Visible = Selected

	public override string? ToString() => ListSeries.Name;

	/// <summary>Creates a <see cref="SeriesInfo"/> snapshot of the current name, color, and selection state.</summary>
	public SeriesInfo GetInfo()
	{
		return new SeriesInfo
		{
			Name = ListSeries.Name,
			Color = Color,
			IsSelected = IsSelected,
		};
	}
}

/// <summary>
/// Lightweight snapshot of a chart series' display properties, used to preserve color and selection state across reloads.
/// </summary>
public class SeriesInfo
{
	/// <summary>Gets or sets the series name.</summary>
	public string? Name { get; set; }

	/// <summary>Gets or sets the series color.</summary>
	public Color Color { get; set; }

	/// <summary>Gets or sets whether the series is currently selected (visible).</summary>
	public bool IsSelected { get; set; } = true; // Visible = Selected
}

/// <summary>
/// Event arguments carrying the chart X-axis data coordinate of the current pointer position.
/// </summary>
public class PointerMovedEventArgs(double x) : EventArgs
{
	/// <summary>Gets the X-axis data coordinate corresponding to the pointer position.</summary>
	public double X => x;
}

/// <summary>
/// Defines the contract for chart controls that support annotations.
/// </summary>
public interface ITabChart
{
	/// <summary>Adds a vertical or horizontal annotation line to the chart.</summary>
	public void AddAnnotation(ChartAnnotation chartAnnotation);

	/// <summary>Gets the list of annotations currently displayed on the chart.</summary>
	public List<ChartAnnotation> Annotations { get; }
}

/// <summary>
/// Abstract base class for Avalonia chart controls. Manages series data, annotations, titles, and time tracking,
/// leaving chart-library-specific rendering to derived classes.
/// </summary>
public abstract class TabChart<TSeries> : Border, ITabChart
{
	/// <summary>The annotation label used for the "current time" vertical line.</summary>
	public const string NowTimeName = "Now";

	/// <summary>Gets or sets the color of the pointer time-tracker line.</summary>
	public Color TimeTrackerColor { get; set; } = SideScrollTheme.DataGridRowHighlight.Color;
	/// <summary>Gets or sets the color of chart grid lines.</summary>
	public Color GridLineColor { get; set; } = SideScrollTheme.ChartGridLines.Color;
	/// <summary>Gets or sets the color of the "Now" annotation line.</summary>
	public Color NowColor { get; set; } = SideScrollTheme.ChartNowLine.Color;
	/// <summary>Gets or sets the color of axis labels and chart text.</summary>
	public Color TextColor { get; set; } = SideScrollTheme.ChartLabelForeground.Color;

	public const int DefaultColorCount = 10;
	public static Color GetColor(int index) => SideScrollTheme.ChartSeries(1 + index % DefaultColorCount).Color;

	protected static readonly WeakEventSource<PointerMovedEventArgs> _pointerMovedEventSource = new();

	protected WeakSubscriber<PointerMovedEventArgs>? _pointerMovedSubscriber;

	/// <summary>Raised when the set of selected series changes.</summary>
	public event EventHandler<SeriesSelectedEventArgs>? SelectionChanged;

	protected const double MarginPercent = 0.1; // This needs a min height so this can be lowered
	protected const int MinSelectionWidth = 10;

	/// <summary>Gets or sets the data model driving this chart.</summary>
	public ChartView ChartView { get; set; }
	/// <summary>Gets or sets whether the chart expands to fill available vertical space up to its maximum height.</summary>
	public bool FillHeight { get; set; }

	/// <summary>Gets the list of all chart series with their display state.</summary>
	public List<ChartSeries<TSeries>> ChartSeries { get; } = [];
	protected Dictionary<string, ChartSeries<TSeries>> IdxNameToChartSeries { get; } = [];
	protected Dictionary<string, SeriesInfo> IdxSeriesInfo { get; } = [];

	/// <summary>Gets the list of currently selected (visible) series, or an empty list when all series are selected.</summary>
	public List<ListSeries> SelectedSeries
	{
		get
		{
			List<ListSeries> selected = ChartSeries
				.Where(s => s.IsSelected)
				.Select(s => s.ListSeries)
				.ToList();

			if (selected.Count == ChartSeries.Count && selected.Count > 1)
			{
				selected.Clear(); // If all are selected, none are selected?
			}
			return selected;
		}
	}

	/// <summary>Gets the title text block displayed above the chart, or <c>null</c> if no title was set.</summary>
	public TextBlock? TitleTextBlock { get; protected set; }
	/// <summary>Gets or sets whether the title text block highlights on hover to indicate it is clickable.</summary>
	public bool IsTitleSelectable { get; set; }

	protected PropertyInfo? XAxisPropertyInfo { get; set; }
	/// <summary>Gets whether the X axis should use DateTime formatting, based on the series X property type or a configured time window.</summary>
	public bool UseDateTimeAxis => (XAxisPropertyInfo?.PropertyType == typeof(DateTime)) ||
									(ChartView.TimeWindow != null);

	/// <summary>Gets the list of annotations displayed on the chart.</summary>
	public List<ChartAnnotation> Annotations { get; } = [];

	/// <summary>Gets or sets the annotation representing the current UTC time.</summary>
	public ChartAnnotation? NowTimeAnnotation { get; set; }

	/// <summary>Gets the root grid that contains the title, chart area, and legend.</summary>
	public Grid ContainerGrid { get; }

	public override string? ToString() => ChartView.ToString();

	protected TabChart(ChartView chartView, bool fillHeight = false)
	{
		ChartView = chartView;
		FillHeight = fillHeight;

		HorizontalAlignment = HorizontalAlignment.Stretch;
		if (FillHeight)
		{
			VerticalAlignment = VerticalAlignment.Top;
		}
		else
		{
			VerticalAlignment = VerticalAlignment.Stretch;
		}

		Child = ContainerGrid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitions("*,Auto"),
			RowDefinitions = new RowDefinitions("Auto,*,Auto"),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
		};

		MaxWidth = 1500;
		MaxHeight = 645; // 25 Items

		XAxisPropertyInfo = chartView.Series.FirstOrDefault()?.XPropertyInfo;

		AddTitle();
	}

	private void AddTitle()
	{
		string? title = ChartView.Name;
		if (title == null) return;

		TitleTextBlock = new TabTextBlock
		{
			Text = ChartView.Name,
			FontSize = 16,
			Margin = new Thickness(10, 5, 10, 2),
			//FontWeight = FontWeight.Medium,
			TextWrapping = TextWrapping.Wrap,
			Foreground = SideScrollTheme.ChartLabelForeground,
			[Grid.ColumnSpanProperty] = 2,
		};
		if (ChartView.LegendPosition == ChartLegendPosition.Bottom)
		{
			TitleTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
		}
		else
		{
			TitleTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
			TitleTextBlock.Margin = new Thickness(55, 5, 5, 2);
		}
		TitleTextBlock.PointerEntered += TitleTextBlock_PointerEntered;
		TitleTextBlock.PointerExited += TitleTextBlock_PointerExited;
	}

	/// <summary>Adds an annotation to the chart's annotation list.</summary>
	public virtual void AddAnnotation(ChartAnnotation chartAnnotation)
	{
		Annotations.Add(chartAnnotation);
	}

	/// <summary>Adds or updates the "Now" annotation line to the current UTC time, or removes it if <see cref="SideScroll.Charts.ChartView.ShowNowTime"/> is <c>false</c>.</summary>
	public void UpdateNowTime()
	{
		ChartView.Annotations.RemoveAll(a => a.Text == NowTimeName);

		if (!ChartView.ShowNowTime) return;
		
		var now = DateTime.UtcNow;
		if (NowTimeAnnotation == null)
		{
			if (ChartView.TimeWindow != null && ChartView.TimeWindow.EndTime < now.AddMinutes(1))
				return;

			NowTimeAnnotation = new ChartAnnotation
			{
				Text = NowTimeName,
				Color = NowColor.AsSystemColor(),
				// LineStyle = LineStyle.Dot,
			};
		}

		NowTimeAnnotation.X = now.Ticks;

		// ChartView's can be reused across different Charts, so we can't just remove the current occurence of Now
		ChartView.Annotations.Add(NowTimeAnnotation);
	}

	/// <summary>Requests the charting library to re-render the chart surface.</summary>
	public abstract void InvalidateChart();

	/// <summary>Rebuilds the chart series from the current <see cref="ChartView"/> data.</summary>
	public abstract void ReloadView();

	/// <summary>Updates the chart series from a new <see cref="ChartView"/>, reusing existing colors and state where possible.</summary>
	public abstract void UpdateView(ChartView chartView);

	/// <summary>Hides the chart and releases its series resources.</summary>
	public abstract void Unload();

	/// <summary>Returns the saved <see cref="SeriesInfo"/> for the given series, or <c>null</c> if not found.</summary>
	public SeriesInfo? GetSeriesInfo(ListSeries listSeries)
	{
		if (listSeries.Name != null && IdxSeriesInfo.TryGetValue(listSeries.Name, out SeriesInfo? seriesInfo))
			return seriesInfo;
		return null;
	}

	protected void UpdateSeriesInfo(ChartSeries<TSeries> chartSeries)
	{
		if (chartSeries.ListSeries.Name is string name)
		{
			IdxSeriesInfo[name] = chartSeries.GetInfo();
		}
	}

	protected virtual void OnSelectionChanged(SeriesSelectedEventArgs e)
	{
		// Safely raise the event for all subscribers
		SelectionChanged?.Invoke(this, e);
		ChartView.OnSelectionChanged(e);
	}

	// Anchor the chart to the top and stretch to max height, available size gets set to max :(
	protected override Size MeasureOverride(Size availableSize)
	{
		Size size = base.MeasureOverride(availableSize);
		if (FillHeight)
		{
			size = size.WithHeight(Math.Max(size.Height, Math.Min(MaxHeight, availableSize.Height)));
		}
		return size;
	}

	private void TitleTextBlock_PointerEntered(object? sender, PointerEventArgs e)
	{
		if (IsTitleSelectable)
		{
			TitleTextBlock!.Foreground = SideScrollTheme.ChartLabelForegroundHighlight;
		}
	}

	private void TitleTextBlock_PointerExited(object? sender, PointerEventArgs e)
	{
		TitleTextBlock!.Foreground = SideScrollTheme.LabelForeground;
	}
}
