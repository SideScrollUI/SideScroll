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

public abstract class TabChartLegendItem<TSeries> : Grid
{
	public event EventHandler<EventArgs>? OnVisibilityChanged;

	public TabChartLegend<TSeries> Legend { get; }
	public ChartSeries<TSeries> ChartSeries { get; }

	public ChartView ChartView => Legend.ChartView;
	public TSeries Series => ChartSeries.LineSeries;

	public TabTextBlock? TextBlock { get; protected set; }
	public TabTextBlock? TextBlockTotal { get; protected set; }

	protected Polygon? _polygon;

	private int _index;
	public int Index
	{
		get => _index;
		set
		{
			_index = value;
			UpdateTitleText();
		}
	}
	public int Count { get; set; }
	public double? Total { get; set; }

	private bool _isSelected = true;
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

	public abstract void UpdateColor(Color color);

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
