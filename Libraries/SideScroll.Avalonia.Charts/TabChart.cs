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
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using System.Collections;
using System.Reflection;

namespace SideScroll.Avalonia.Charts;

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
	public string? Name { get; set; }

	public Color Color { get; set; }

	public bool IsSelected { get; set; } = true; // Visible = Selected
}

public class PointerMovedEventArgs(double x) : EventArgs
{
	public double X => x;
}

public interface ITabChart
{
	public void AddAnnotation(ChartAnnotation chartAnnotation);

	public List<ChartAnnotation> Annotations { get; }
}

public abstract class TabChart<TSeries> : Border, ITabChart
{
	public const string NowTimeName = "Now";

	public Color TimeTrackerColor { get; set; } = SideScrollTheme.DataGridRowHighlight.Color;
	public Color GridLineColor { get; set; } = SideScrollTheme.ChartGridLines.Color;
	public Color NowColor { get; set; } = SideScrollTheme.ChartNowLine.Color;
	public Color TextColor { get; set; } = SideScrollTheme.ChartLabelForeground.Color;

	public const int DefaultColorCount = 10;
	public static Color GetColor(int index) => SideScrollTheme.ChartSeries(1 + index % DefaultColorCount).Color;

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
	protected Dictionary<string, SeriesInfo> IdxSeriesInfo { get; set; } = [];

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

	public TextBlock? TitleTextBlock { get; protected set; }
	public bool IsTitleSelectable { get; set; }

	protected PropertyInfo? XAxisPropertyInfo { get; set; }
	public bool UseDateTimeAxis => (XAxisPropertyInfo?.PropertyType == typeof(DateTime)) ||
									(ChartView.TimeWindow != null);

	public List<ChartAnnotation> Annotations { get; set; } = [];

	public ChartAnnotation? NowTimeAnnotation { get; set; }

	public Grid ContainerGrid { get; protected set; }

	public override string? ToString() => ChartView.ToString();

	protected TabChart(TabInstance tabInstance, ChartView chartView, bool fillHeight = false)
	{
		TabInstance = tabInstance;
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

	public virtual void AddAnnotation(ChartAnnotation chartAnnotation)
	{
		Annotations.Add(chartAnnotation);
	}

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
				Horizontal = false,
				Color = NowColor.AsSystemColor(),
				// LineStyle = LineStyle.Dot,
			};
		}

		NowTimeAnnotation.X = now.Ticks;

		// ChartView's can be reused across different Charts, so we can't just remove the current occurence of Now
		ChartView.Annotations.Add(NowTimeAnnotation);
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
