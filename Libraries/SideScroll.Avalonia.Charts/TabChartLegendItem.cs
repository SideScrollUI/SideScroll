using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Themes;
using SideScroll.Charts;
using SideScroll.Extensions;

namespace SideScroll.Avalonia.Charts;

/// <summary>
/// Abstract base class for a single legend item row. Displays a color swatch, series name, and optional total value,
/// and handles click-to-select and hover-to-highlight interactions.
/// </summary>
public abstract class TabChartLegendItem<TSeries> : Grid
{
	/// <summary>Raised when the item's visibility (selection) state is toggled by the user.</summary>
	public event EventHandler<EventArgs>? OnVisibilityChanged;

	/// <summary>Gets the parent legend panel this item belongs to.</summary>
	public TabChartLegend<TSeries> Legend { get; }
	/// <summary>Gets the chart series this legend item represents.</summary>
	public ChartSeries<TSeries> ChartSeries { get; }

	/// <summary>Gets the chart view data model from the parent legend.</summary>
	public ChartView ChartView => Legend.ChartView;
	/// <summary>Gets the native chart series object.</summary>
	public TSeries Series => ChartSeries.LineSeries;

	/// <summary>Gets the text block displaying the series name (and optional rank prefix).</summary>
	public TabTextBlock? TextBlock { get; protected set; }
	/// <summary>Gets the text block displaying the series total value, or <c>null</c> if totals are not shown.</summary>
	public TabTextBlock? TextBlockTotal { get; protected set; }

	protected Polygon? _polygon;

	private int _index;
	/// <summary>Gets or sets the 1-based display rank shown as a prefix in the legend label when ordering is enabled.</summary>
	public int Index
	{
		get => _index;
		set
		{
			_index = value;
			UpdateTitleText();
		}
	}
	/// <summary>Gets or sets the number of data points in this series.</summary>
	public int Count { get; set; }
	/// <summary>Gets or sets the aggregate total value for this series.</summary>
	public double? Total { get; set; }

	private bool _isSelected = true;
	/// <summary>Gets or sets whether this series is selected (visible). Updates the color swatch fill accordingly.</summary>
	public bool IsSelected
	{
		get => _isSelected;
		set
		{
			ChartSeries.IsSelected = value;
			_isSelected = value;
			SetFilled(value);
		}
	}

	private readonly SolidColorBrush _colorBrush;

	private static readonly List<Point> PolygonPointsSmall = GetPolygonPoints(13, 13);
	private static readonly List<Point> PolygonPointsLarge = GetPolygonPoints(15, 15);

	public override string? ToString() => ChartSeries.ToString();

	protected TabChartLegendItem(TabChartLegend<TSeries> legend, ChartSeries<TSeries> chartSeries)
	{
		Legend = legend;
		ChartSeries = chartSeries;

		ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto");
		RowDefinitions = new RowDefinitions("Auto");

		_colorBrush = new SolidColorBrush(ChartSeries.Color);

		UpdateTotal();

		AddCheckBox();
		AddTextBlock();

		if (ChartView.ShowOrder && ChartView.LegendPosition == ChartLegendPosition.Right)
		{
			AddTotalTextBlock();
		}

		PointerEntered += TabChartLegendItem_PointerEntered;
		PointerExited += TabChartLegendItem_PointerExited;
	}

	private void SetFilled(bool filled)
	{
		if (_polygon != null)
		{
			_polygon.Fill = filled && Count > 0 ? _colorBrush : Brushes.Transparent;
		}
	}

	/// <summary>Refreshes the point count, total value, and swatch fill from the underlying series data.</summary>
	public void UpdateTotal()
	{
		Total = ChartSeries.ListSeries.Total;
		if (Count == 0 && ChartSeries.ListSeries.List.Count > 0)
		{
			IsSelected = true; // Now has points
		}
		Count = ChartSeries.ListSeries.List.Count;
		if (TextBlockTotal != null)
		{
			TextBlockTotal.Text = Total?.FormattedShortDecimal();
		}
		UpdateCheckBox();
	}

	private void AddCheckBox()
	{
		_polygon = new Polygon
		{
			Width = 16,
			Height = 13,
			Stroke = SideScrollTheme.ChartLegendIconBorder,
			StrokeThickness = 1.5,
			Points = PolygonPointsSmall,
			VerticalAlignment = VerticalAlignment.Center,
		};

		UpdateCheckBox();

		_polygon.PointerPressed += Polygon_PointerPressed;
		Children.Add(_polygon);
	}

	private void UpdateCheckBox()
	{
		if (_polygon == null) return;

		if (Count > 0)
		{
			if (_polygon.Fill == null)
			{
				IsSelected = true;
			}
			_polygon.Fill = _colorBrush;
		}
		else
		{
			IsSelected = false;
		}
	}

	private static List<Point> GetPolygonPoints(int width, int height, int cornerSize = 3)
	{
		return new List<Point>
		{
			new(0, height),
			new(width - cornerSize, height),
			new(width, height - cornerSize),
			new(width, 0),
			new(cornerSize, 0),
			new(0, cornerSize),
		};
	}

	private void AddTextBlock()
	{
		TextBlock = new TabTextBlock
		{
			Margin = new Thickness(2, 2, 6, 2),
			//VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			Foreground = SideScrollTheme.ChartLabelForeground,
			[Grid.ColumnProperty] = 1,
		};
		UpdateTitleText();
		Children.Add(TextBlock);
	}

	private void UpdateTitleText()
	{
		string prefix = "";
		if (Index > 0 && ChartView.Series.Count > 1)
		{
			prefix = $"{Index}. ";
		}

		TextBlock!.Text = prefix + ToString();
	}

	private void AddTotalTextBlock()
	{
		TextBlockTotal = new TabTextBlock
		{
			Text = Total?.FormattedShortDecimal(),
			Margin = new Thickness(10, 2, 6, 2),
			HorizontalAlignment = HorizontalAlignment.Right,
			Foreground = SideScrollTheme.ChartLabelForeground,
			[Grid.ColumnProperty] = 2,
		};
		Children.Add(TextBlockTotal);
	}

	protected bool _highlight;
	public bool Highlight
	{
		get => _highlight;
		set
		{
			if (value == _highlight)
				return;

			_highlight = value;
			if (_highlight)
			{
				_polygon!.Points = PolygonPointsLarge;
				SetFilled(true);
				_highlight = true;
				TextBlock!.Foreground = SideScrollTheme.ChartLabelForegroundHighlight;
				if (TextBlockTotal != null)
				{
					TextBlockTotal.Foreground = SideScrollTheme.ChartLabelForegroundHighlight;
				}
			}
			else
			{
				_polygon!.Points = PolygonPointsSmall;
				_highlight = false;
				SetFilled(IsSelected);
				TextBlock!.Foreground = SideScrollTheme.ChartLabelForeground;
				if (TextBlockTotal != null)
				{
					TextBlockTotal.Foreground = SideScrollTheme.ChartLabelForeground;
				}
			}

			UpdateVisible();

			Legend.UpdateHighlight(_highlight);
		}
	}

	/// <summary>Updates the series color to full or faded based on whether this item is highlighted or <paramref name="showFaded"/> is requested.</summary>
	public void UpdateHighlight(bool showFaded)
	{
		Color newColor;
		if (Highlight || !showFaded)
		{
			newColor = ChartSeries.Color;
		}
		else
		{
			newColor = Color.FromArgb(32, ChartSeries.Color.R, ChartSeries.Color.G, ChartSeries.Color.B); // Show Faded
		}

		UpdateColor(newColor);
	}

	/// <summary>Applies the given color to the underlying native series paint.</summary>
	public abstract void UpdateColor(Color color);

	/// <summary>Synchronizes the native series visibility with the current <see cref="IsSelected"/> and <see cref="Highlight"/> state.</summary>
	public abstract void UpdateVisible();

	private void Polygon_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
		{
			IsSelected = !IsSelected;
			OnVisibilityChanged?.Invoke(this, EventArgs.Empty);
			e.Handled = true;
		}
	}

	private void TabChartLegendItem_PointerEntered(object? sender, PointerEventArgs e)
	{
		Legend.UnhighlightAll();
		Highlight = true;
	}

	private void TabChartLegendItem_PointerExited(object? sender, PointerEventArgs e)
	{
		Highlight = false;
	}
}
