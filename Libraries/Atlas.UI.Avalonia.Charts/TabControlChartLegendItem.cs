using Atlas.Core.Charts;
using Atlas.Extensions;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace Atlas.UI.Avalonia.Charts;

public abstract class TabChartLegendItem<TSeries> : Grid
{
	public event EventHandler<EventArgs>? OnSelectionChanged;
	public event EventHandler<EventArgs>? OnVisibleChanged;

	public readonly TabControlChartLegend<TSeries> Legend;
	public readonly ChartSeries<TSeries> ChartSeries;

	public ChartView ChartView => Legend.ChartView;
	public TSeries Series => ChartSeries.LineSeries;

	public TabControlTextBlock? TextBlock;
	public TabControlTextBlock? TextBlockTotal;

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

	private SolidColorBrush _colorBrush;

	public override string? ToString() => ChartSeries.ToString();

	protected TabChartLegendItem(TabControlChartLegend<TSeries> legend, ChartSeries<TSeries> chartSeries)
	{
		Legend = legend;
		ChartSeries = chartSeries;

		ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto");
		RowDefinitions = new RowDefinitions("Auto");

		Background = AtlasTheme.TabBackground;
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
		_polygon!.Fill = filled && Count > 0 ? _colorBrush : Brushes.Transparent;
	}

	public void UpdateTotal()
	{
		Total = ChartSeries.ListSeries.Total;
		Count = ChartSeries.ListSeries.List.Count;
		if (TextBlockTotal != null)
		{
			TextBlockTotal.Text = Total?.FormattedShortDecimal();
		}
	}

	private void AddCheckBox()
	{
		int width = 13;
		int height = 13;

		_polygon = new Polygon
		{
			Width = 16,
			Height = 16,
			Stroke = Brushes.Black,
			StrokeThickness = 1.5,
			Points = GetPolygonPoints(width, height),
		};

		if (Count > 0)
			_polygon.Fill = _colorBrush;
		else
			IsSelected = false;

		_polygon.PointerPressed += Polygon_PointerPressed;
		Children.Add(_polygon);
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

	private void UpdatePolygonPoints(int width, int height)
	{
		_polygon!.Points = GetPolygonPoints(width, height);
	}

	private void AddTextBlock()
	{
		TextBlock = new TabControlTextBlock
		{
			Margin = new Thickness(2, 2, 6, 2),
			//VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			[Grid.ColumnProperty] = 1,
		};
		UpdateTitleText();
		Children.Add(TextBlock);
	}

	private void UpdateTitleText()
	{
		string prefix = "";
		if (Index > 0 && ChartView.Series.Count > 1)
			prefix = $"{Index}. ";

		TextBlock!.Text = prefix + ToString();
	}

	private void AddTotalTextBlock()
	{
		TextBlockTotal = new TabControlTextBlock
		{
			Text = Total?.FormattedShortDecimal(),
			Margin = new Thickness(10, 2, 6, 2),
			HorizontalAlignment = HorizontalAlignment.Right,
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
				UpdatePolygonPoints(15, 15);
				SetFilled(true);
				_highlight = true;
				TextBlock!.Foreground = AtlasTheme.ChartLabelForegroundHighlight;
				if (TextBlockTotal != null)
					TextBlockTotal.Foreground = AtlasTheme.ChartLabelForegroundHighlight;
			}
			else
			{
				UpdatePolygonPoints(13, 13);
				_highlight = false;
				SetFilled(IsSelected);
				TextBlock!.Foreground = AtlasTheme.LabelForeground;
				if (TextBlockTotal != null)
					TextBlockTotal.Foreground = AtlasTheme.LabelForeground;
			}

			UpdateVisible();

			Legend.UpdateHighlight(_highlight);
		}
	}

	public void UpdateHighlight(bool showFaded)
	{
		Color newColor;
		if (Highlight || !showFaded)
			newColor = ChartSeries.Color;
		else
			newColor = Color.FromArgb(32, ChartSeries.Color.R, ChartSeries.Color.G, ChartSeries.Color.B); // Show Faded

		UpdateColor(newColor);
	}

	public abstract void UpdateColor(Color color);

	public abstract void UpdateVisible();

	private void Polygon_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
		{
			IsSelected = !IsSelected;
			OnSelectionChanged?.Invoke(this, EventArgs.Empty);
			e.Handled = true;
		}
	}

	private void TabChartLegendItem_PointerEntered(object? sender, PointerEventArgs e)
	{
		Legend.UnhighlightAll(false);
		Highlight = true;
	}

	private void TabChartLegendItem_PointerExited(object? sender, PointerEventArgs e)
	{
		Highlight = false;
	}
}
