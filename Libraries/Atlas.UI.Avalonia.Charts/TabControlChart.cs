using Atlas.Core;
using Atlas.Core.Charts;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.Themes;
using Atlas.UI.Avalonia.Utilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections;
using System.Reflection;

namespace Atlas.UI.Avalonia.Charts;

public class ChartSeries<TSeries>(ListSeries listSeries, TSeries lineSeries, Color color)
{
	public ListSeries ListSeries => listSeries;
	public TSeries LineSeries => lineSeries;
	public Color Color => color;

	public bool IsSelected { get; set; } = true; // Visible = Selected

	public override string? ToString() => ListSeries.Name;

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

public class SeriesInfo
{
	public string? Name;

	public Color Color;

	public bool IsSelected { get; set; } = true; // Visible = Selected
}

public class SeriesSelectedEventArgs(List<ListSeries> series) : EventArgs
{
	public List<ListSeries> Series { get; set; } = series;
}

public class PointerMovedEventArgs(double x) : EventArgs
{
	public double X { get; set; } = x;
}

public interface ITabControlChart
{
	public void AddAnnotation(ChartAnnotation chartAnnotation);

	public List<ChartAnnotation> Annotations { get; }
}

public abstract class TabControlChart<TSeries> : Grid, ITabControlChart
{
	public static Color TimeTrackerColor = AtlasTheme.DataGridRowHighlight.Color;
	public static Color GridLineColor = Color.Parse("#333333");
	public static Color TextColor = Colors.LightGray;

	private static readonly System.Drawing.Color NowColor = System.Drawing.Color.Green;
	public static Color[] DefaultColors { get; set; } =
	[
		Colors.LawnGreen,
		Colors.Fuchsia,
		Colors.DodgerBlue,
		Colors.Gold,
		Colors.Red,
		Colors.Cyan,
		Colors.BlueViolet,
		Colors.Orange,
		Colors.Salmon,
		Colors.MediumSpringGreen,
	];
	public static Color GetColor(int index) => DefaultColors[index % DefaultColors.Length];

	protected static readonly WeakEventSource<PointerMovedEventArgs> _pointerMovedEventSource = new();

	protected WeakSubscriber<PointerMovedEventArgs>? _pointerMovedSubscriber;

	public event EventHandler<SeriesSelectedEventArgs>? SelectionChanged;

	protected const double MarginPercent = 0.1; // This needs a min height so this can be lowered
	protected const int MinSelectionWidth = 10;

	public TabInstance TabInstance { get; init; }
	public ChartView ChartView { get; set; }
	public bool FillHeight { get; set; }
	public int SeriesLimit { get; set; } = 25;

	public List<ChartSeries<TSeries>> ChartSeries { get; private set; } = [];
	protected Dictionary<string, ChartSeries<TSeries>> IdxNameToChartSeries { get; set; } = [];
	protected Dictionary<IList, ListSeries> IdxListToListSeries { get; set; } = [];
	protected Dictionary<string, SeriesInfo> IdxSeriesInfo = [];

	public List<ListSeries> SelectedSeries
	{
		get
		{
			List<ListSeries> selected = ChartSeries
				.Where(s => s.IsSelected)
				.Select(s => s.ListSeries)
				.ToList();

			if (selected.Count == ChartSeries.Count && selected.Count > 1)
				selected.Clear(); // If all are selected, none are selected?
			return selected;
		}
	}

	public TextBlock? TitleTextBlock { get; protected set; }
	public bool IsTitleSelectable { get; set; }

	protected PropertyInfo? XAxisPropertyInfo;
	public bool UseDateTimeAxis => (XAxisPropertyInfo?.PropertyType == typeof(DateTime)) ||
									(ChartView.TimeWindow != null);

	public List<ChartAnnotation> Annotations { get; set; } = [];

	public override string? ToString() => ChartView.ToString();

	protected TabControlChart(TabInstance tabInstance, ChartView chartView, bool fillHeight = false)
	{
		TabInstance = tabInstance;
		ChartView = chartView;
		FillHeight = fillHeight;

		HorizontalAlignment = HorizontalAlignment.Stretch;
		if (FillHeight)
			VerticalAlignment = VerticalAlignment.Top;
		else
			VerticalAlignment = VerticalAlignment.Stretch;

		ColumnDefinitions = new ColumnDefinitions("*");
		RowDefinitions = new RowDefinitions("*");

		MaxWidth = 1500;
		MaxHeight = 645; // 25 Items

		if (TabInstance.TabViewSettings.ChartDataSettings.Count == 0)
		{
			TabInstance.TabViewSettings.ChartDataSettings.Add(new TabDataSettings());
		}

		XAxisPropertyInfo = chartView.Series.FirstOrDefault()?.XPropertyInfo;

		AddTitle();
	}

	private void AddTitle()
	{
		string? title = ChartView.Name;
		if (title == null) return;
		
		TitleTextBlock = new TabControlTextBlock
		{
			Text = ChartView.Name,
			FontSize = 16,
			Margin = new Thickness(10, 5, 10, 2),
			//FontWeight = FontWeight.Medium,
			TextWrapping = TextWrapping.Wrap,
			[ColumnSpanProperty] = 2,
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

	public virtual void AddAnnotation(ChartAnnotation chartAnnotation)
	{
		Annotations.Add(chartAnnotation);
	}

	public void AddNowTime()
	{
		var now = DateTime.UtcNow;
		if (ChartView.TimeWindow != null && ChartView.TimeWindow.EndTime < now.AddMinutes(1))
			return;

		var annotation = new ChartAnnotation
		{
			Text = "Now",
			Horizontal = false,
			X = now.Ticks,
			Color = NowColor,
			// LineStyle = LineStyle.Dot,
		};

		ChartView.Annotations.Add(annotation);
	}

	public abstract void InvalidateChart();

	public abstract void ReloadView();

	public abstract void UpdateView(ChartView chartView);

	public abstract void Unload();

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
			TitleTextBlock!.Foreground = AtlasTheme.ChartLabelForegroundHighlight;
		}
	}

	private void TitleTextBlock_PointerExited(object? sender, PointerEventArgs e)
	{
		TitleTextBlock!.Foreground = AtlasTheme.LabelForeground;
	}
}
