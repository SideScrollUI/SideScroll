using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Avalonia;
using Avalonia.Input;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlChart : UserControl //, IDisposable
	{
		//private string name;
		private TabInstance tabInstance;
		//public ChartSettings ChartSettings { get; set; }
		public ListGroup ListGroup { get; set; }

		//private List<ListSeries> ListSeries { get; set; }
		private Dictionary<IList, ListSeries> ListToTabSeries { get; set; } = new Dictionary<IList, ListSeries>();
		private Dictionary<IList, int> ListToTabIndex { get; set; } = new Dictionary<IList, int>(); // not used

		//public SeriesCollection SeriesCollection { get; set; }
		public string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }

		// try to change might be lower or higher than the rendering interval
		private const int UpdateInterval = 20;

		//private bool disposed;
		//private readonly Timer timer;
		//private int numberOfSeries;

		//private TabControlDataGrid tabControlDataGrid;
		public PlotModel plotModel;
		public PlotView plotView;
		private PropertyInfo xAxisPropertyInfo;
		private TabControlChartLegend legend;
		private OxyPlot.Axes.LinearAxis valueAxis;

		private static OxyColor GridLineColor = OxyColor.Parse("#333333");
		public static OxyColor[] Colors { get; set; } = new OxyColor[] {
				OxyColors.LawnGreen,
				OxyColors.Fuchsia,
				OxyColors.Cyan,
				//OxyColors.Aquamarine, // too close to Cyan (but more matte)
				OxyColors.Gold,
				OxyColors.DodgerBlue,
				OxyColors.Red,
				OxyColors.BlueViolet,
				//OxyColors.SlateBlue,
				OxyColors.Orange,
				//OxyColors.Pink,
				//OxyColors.Coral,
				//OxyColors.YellowGreen,
				OxyColors.Salmon,
				OxyColors.MediumSpringGreen,
			};

		public static OxyColor GetColor(int index)
		{
			return Colors[index % Colors.Length];
		}

		//public event EventHandler<EventArgs> OnSelectionChanged;
		//private bool autoSelectNew = true;

		public TabControlChart(TabInstance tabInstance, ListGroup listGroup)
		{
			this.tabInstance = tabInstance;
			this.ListGroup = listGroup;

			InitializeControls();
			//DataContext = new TestNode().Children;
		}

		public override string ToString()
		{
			return ListGroup.ToString(); // todo: fix for multiple
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			return base.MeasureOverride(availableSize);
		}

		private void Initialize()
		{
			InitializeControls();
		}

		protected override void OnMeasureInvalidated()
		{
			base.OnMeasureInvalidated();
		}

		///public class LabelControl
		//{
			/*TextBlock labelTitle = new TextBlock()
			{
				Text = ToString(),
				Background = new SolidColorBrush(Theme.TitleBackgroundColor),
				Foreground = new SolidColorBrush(Theme.TitleForegroundColor),
				FontSize = 14,
				//Margin = new Thickness(2), // Shows as black, Need Padding so Border not needed
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				//VerticalAlignment = VerticalAlignment.Auto, // doesn't exist
				//Height = 24,
			};
			//this.Children.Add(labelTitle);

			Border borderTitle = new Border()
			{
				//BorderThickness = new Thickness(10),
				BorderThickness = new Thickness(5, 2, 2, 2),
				//Background = new SolidColorBrush(Theme.GridColumnHeaderBackgroundColor),
				//Background = new SolidColorBrush(Colors.Crimson),
				BorderBrush = new SolidColorBrush(Theme.TitleBackgroundColor),
				[Grid.RowProperty] = 0,
				[Grid.ColumnSpanProperty] = 2,
			};
			borderTitle.Child = labelTitle;*/
		//}

		// don't want to reload this because 
		private void InitializeControls()
		{
			//this.Background = new SolidColorBrush(Theme.BackgroundColor);
			this.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch; // OxyPlot import collision
			this.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
			//this.Width = 1000;
			//this.Height = 1000;
			//this.Children.Add(border);
			//this.Orientation = Orientation.Vertical;
			MinHeight = 200;
			MaxWidth = 1000;
			MaxHeight = 1000;
			MinWidth = 150;

			// autogenerate columns
			if (tabInstance.tabViewSettings.ChartDataSettings.Count == 0)
				tabInstance.tabViewSettings.ChartDataSettings.Add(new TabDataSettings());
			//tabDataGrid = new TabDataGrid(tabInstance, ChartSettings.ListSeries, true, tabInstance.tabViewSettings.ChartDataSettings);
			//tabControlDataGrid = new TabControlDataGrid(tabInstance, ListGroup.ListSeries, true, tabInstance.tabViewSettings.ChartDataSettings[0]);
			//Grid.SetRow(tabDataGrid, 1);

			//tabDataGrid.AddButtonColumn("<>", nameof(TaskInstance.Cancel));

			//tabDataGrid.AutoLoad = tabModel.AutoLoad;
			//tabControlDataGrid.OnSelectionChanged += TabData_OnSelectionChanged;
			//tabDataGrid.Width = 1000;
			//tabDataGrid.Height = 1000;
			//tabDataGrid.Initialize();
			//bool addSplitter = false;
			//tabParentControls.AddControl(tabDataGrid, true, false);
			
			plotView = new PlotView()
			{
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,

				Background = Brushes.Transparent,
				//Foreground = Brushes.LightGray,
				BorderBrush = Brushes.LightGray,
				IsMouseWheelEnabled = false,
				DisconnectCanvasWhileUpdating = false, // Tracker will show behind grid lines if the PlotView is resized and this is set
			};

			// Show Hover text on mouse over instead of requiring holding the mouse down (why isn't this the default?)
			plotView.ActualController.UnbindMouseDown(OxyMouseButton.Left); // remove default
			plotView.ActualController.BindMouseEnter(PlotCommands.HoverSnapTrack); // show when hovering

			LoadPlotModel();
			/*plotView.Template = new ControlTemplate() // todo: fix
			{
				Content = new object(),
				TargetType = typeof(object),
			};*/

			// Doesn't work for Children that Stretch?
			/*StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = Orientation.Vertical;
			stackPanel.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
			stackPanel.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
			stackPanel.Children.Add(borderTitle);
			stackPanel.Children.Add(plotView);*/


			Grid containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*,Auto"),
				RowDefinitions = new RowDefinitions("*,Auto"), // Header, Body
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,
				Background = new SolidColorBrush(Theme.BackgroundColor), // grid lines look bad when hovering
			};
			//containerGrid.Children.Add(borderTitle);

			//containerGrid.Children.Add(tabControlDataGrid);
			containerGrid.Children.Add(plotView);

			legend = new TabControlChartLegend(plotView, ListGroup.Horizontal);
			if (ListGroup.Horizontal)
				Grid.SetRow(legend, 1);
			else
				Grid.SetColumn(legend, 1);
			containerGrid.Children.Add(legend);
			legend.OnSelectionChanged += Legend_OnSelectionChanged;

			//this.watch.Start();
			this.Content = containerGrid;

			this.Focusable = true;
		}

		private void Legend_OnSelectionChanged(object sender, EventArgs e)
		{
			UpdateValueAxis();
		}

		public string GetDateTimeFormat(double duration)
		{
			if (duration < 60)
				return "H:mm:ss";
			if (duration < 60 * 60)
				return "H:mm";
			if (duration < 24 * 60 * 60)
				return "H:mm";
			if (duration < 3 * 24 * 60 * 60)
				return "M/d H:mm";
			if (duration < 30 * 24 * 60 * 60)
				return "M/d";

			return "yyyy-M-d";
		}

		private void LoadPlotModel()
		{
			UnloadModel();
			RecreatePlotModel();

			//double duration = 0;
			foreach (ListSeries listSeries in ListGroup.ListSeries)
			{
				//duration = listSeries.iList[0]
				AddSeries(listSeries);
			}
			AddAxis();

			// would need to be able to disable to use
			//foreach (ListSeries listSeries in ChartSettings.ListSeries)
			//	AddSeries(listSeries);

			UpdateValueAxis();

			plotView.Model = plotModel;
		}

		public void RecreatePlotModel()
		{
			plotModel = new PlotModel()
			{
				Title = ListGroup?.Name,
				TitleFontWeight = 400,
				TitleFontSize = 16,
				//TitleFont = "Arial",
				IsLegendVisible = false,
				LegendPlacement = LegendPlacement.Outside,
				//LegendTitleColor = OxyColors.Yellow, // doesn't work

				TitleColor = OxyColors.LightGray,
				//PlotAreaBorderColor = OxyColors.LightGray,
				PlotAreaBorderColor = OxyColor.Parse("#888888"),
				TextColor = OxyColors.Black,
				LegendTextColor = OxyColors.LightGray,
				SelectionColor = OxyColors.Blue,
			};
			plotView.Model = plotModel;
		}

		public void Refresh()
		{
			UpdateValueAxis();
			plotView.InvalidatePlot(true);
			plotView.Model.InvalidatePlot(true);
			legend.RefreshModel();
		}

		public void Unload()
		{
			this.IsVisible = false;
			UnloadModel();
		}

		public OxyPlot.Axes.DateTimeAxis AddDateTimeAxis(DateTime? startTime = null, DateTime? endTime = null)
		{
			var dateTimeAxis = new OxyPlot.Axes.DateTimeAxis
			{
				Position = AxisPosition.Bottom,
				//MinorIntervalType = DateTimeIntervalType.Days,
				//IntervalType = DateTimeIntervalType.Days,
				IntervalType = DateTimeIntervalType.Hours,
				MajorGridlineStyle = LineStyle.Solid,
				MajorGridlineColor = GridLineColor,
				//MinorGridlineStyle = LineStyle.None,
				IntervalLength = 75,
				IsAxisVisible = true,
				AxislineColor = OxyColors.Black,
				//AxislineColor = GridLineColor,
				AxislineStyle = LineStyle.Solid,
				AxislineThickness = 2,
				TickStyle = TickStyle.Outside,
				TicklineColor = GridLineColor,
				//MajorTickSize = 5,
				MinorGridlineColor = OxyColors.Gray,
				//MinorTicklineColor = GridLineColor,
				//MinorTickSize = 5,
				AxisTickToLabelDistance = 2,
				TitleColor = OxyColors.LightGray,
				TextColor = OxyColors.LightGray,
			};
			if (startTime != null && endTime != null)
			{
				double duration = endTime.Value.Subtract(startTime.Value).TotalSeconds;
				dateTimeAxis.Minimum = OxyPlot.Axes.DateTimeAxis.ToDouble(startTime.Value);
				dateTimeAxis.Maximum = OxyPlot.Axes.DateTimeAxis.ToDouble(endTime.Value);
				dateTimeAxis.StringFormat = GetDateTimeFormat(duration);
			}
			plotModel.Axes.Add(dateTimeAxis);
			return dateTimeAxis;
		}

		private void AddLinearAccess()
		{
			var linearAxis = new OxyPlot.Axes.LinearAxis
			{
				Position = AxisPosition.Bottom,
				MajorGridlineStyle = LineStyle.Solid,
				MajorGridlineColor = GridLineColor,
				TitleColor = OxyColors.LightGray,
				TextColor = OxyColors.LightGray,
				TicklineColor = GridLineColor,
				MinorGridlineColor = OxyColors.Gray,
			};
			plotModel.Axes.Add(linearAxis);
		}

		public OxyPlot.Axes.LinearAxis AddValueAxis(AxisPosition axisPosition = AxisPosition.Left, string key = null)
		{
			valueAxis = new OxyPlot.Axes.LinearAxis
			{
				Position = axisPosition,
				IntervalLength = 20,
				MajorGridlineStyle = LineStyle.Solid,
				MajorGridlineColor = GridLineColor,
				MinorGridlineStyle = LineStyle.None,
				MinorTickSize = 0,
				MinorStep = 20,
				MinimumMinorStep = 10,
				IsAxisVisible = true,
				AxislineColor = GridLineColor,
				AxislineStyle = LineStyle.Solid,
				AxislineThickness = 2,
				TickStyle = TickStyle.Outside,
				TicklineColor = GridLineColor,
				//MajorTickSize = 2,
				MinorGridlineColor = OxyColors.Gray,
				TitleColor = OxyColors.LightGray,
				TextColor = OxyColors.LightGray,
				LabelFormatter = ValueFormatter,
			};
			if (key != null)
				valueAxis.Key = key;
			plotModel.Axes.Add(valueAxis);
			return valueAxis;
		}

		private void UpdateValueAxis()
		{
			double minimum = double.MaxValue;
			double maximum = double.MinValue;

			foreach (OxyPlot.Series.Series series in plotModel.Series)
			{
				if (series is OxyPlot.Series.LineSeries lineSeries)
				{
					if (lineSeries.LineStyle == LineStyle.None)
						continue;
					if (lineSeries.ItemsSource != null)
					{

						//if (axisKey == "right" && lineSeries.YAxisKey != "right")
						//	continue;

						PropertyInfo propertyInfo = null;
						foreach (var item in lineSeries.ItemsSource)
						{
							if (propertyInfo == null)
								propertyInfo = item.GetType().GetProperty(lineSeries.DataFieldY);

							var value = propertyInfo.GetValue(item);
							double d = Convert.ToDouble(value);
							if (double.IsNaN(d))
								continue;

							minimum = Math.Min(minimum, d);
							maximum = Math.Max(maximum, d);
						}
					}
					else if (lineSeries.Points.Count > 0)
					{
						foreach (var point in lineSeries.Points)
						{
							double d = point.Y;
							if (double.IsNaN(d))
								continue;

							minimum = Math.Min(minimum, d);
							maximum = Math.Max(maximum, d);
						}
					}
				}
				if (series is OxyPlot.Series.ScatterSeries scatterSeries)
				{
					if (scatterSeries.ItemsSource == null)
						continue;

					//if (axisKey == "right" && lineSeries.YAxisKey != "right")
					//	continue;

					PropertyInfo propertyInfo = null;
					foreach (var item in scatterSeries.ItemsSource)
					{
						if (propertyInfo == null)
							propertyInfo = item.GetType().GetProperty(scatterSeries.DataFieldY);

						var value = propertyInfo.GetValue(item);
						double d = Convert.ToDouble(value);
						if (double.IsNaN(d))
							continue;

						minimum = Math.Min(minimum, d);
						maximum = Math.Max(maximum, d);
					}
				}
			}

			if (minimum == double.MaxValue)
			{
				// didn't find any values
				minimum = 0;
				maximum = 1;
			}

			var margin = (maximum - minimum) * 0.10;
			if (minimum == maximum)
				margin = Math.Abs(minimum);

			valueAxis.Minimum = minimum - margin;
			valueAxis.Maximum = maximum + margin;
		}

		private static string ValueFormatter(double d)
		{
			double ad = Math.Abs(d);
			if (ad >= 1E12)
			{
				return string.Format("{0}T", d / 1E12);
			}
			else if (ad >= 1E9)
			{
				return string.Format("{0}G", d / 1E9);
			}
			else if (ad >= 1E6)
			{
				return string.Format("{0}M", d / 1E6);
			}
			else if (ad >= 1E3)
			{
				return string.Format("{0}K", d / 1E3);
			}
			else if (ad < 1E3)
			{
				return string.Format("{0}", d);
			}
			else
			{
				return string.Format("{0}", d);
			}
		}

		private void AddAxis()
		{
			if (xAxisPropertyInfo != null && xAxisPropertyInfo.PropertyType == typeof(DateTime))
			{
				AddDateTimeAxis();
				//plotModel.Axes.Add(new OxyPlot.Axes.DateTimeAxis { Position = AxisPosition.Bottom });
			}
			else
			{
				AddLinearAccess();
			}
			AddValueAxis();
			{
				//plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left });
			}
		}

		private void UnloadModel()
		{
			//if (plotModel != null)
			//	plotModel.Series.Clear();
			/*foreach (ListSeries listSeries in ChartSettings.ListSeries)
			{
				INotifyCollectionChanged iNotifyCollectionChanged = listSeries.iList as INotifyCollectionChanged;
				//if (iNotifyCollectionChanged != null)
				//	iNotifyCollectionChanged.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;
			}*/
		}

		private void AddSeries(ListSeries listSeries)
		{
			var lineSeries = new OxyPlot.Series.LineSeries
			{
				Title = listSeries.Name,
				LineStyle = LineStyle.Solid,
				StrokeThickness = 2,
				Color = GetColor(plotModel.Series.Count),
				TextColor = OxyColors.Black,
				CanTrackerInterpolatePoints = false,
				MarkerSize = 3,
				MarkerType = MarkerType.Circle,
				DataFieldX = listSeries.xPropertyName,
				DataFieldY = listSeries.yPropertyName,
				//ItemsSource = listSeries.iList,
				//DataFieldY = 
			};
			if (listSeries.xPropertyName != null)
			{
				lineSeries.ItemsSource = listSeries.iList;
				xAxisPropertyInfo = listSeries.xPropertyInfo;
			}
			else
			{
				AddPoints(listSeries, listSeries.iList, lineSeries);
			}

			plotModel.Series.Add(lineSeries);

			INotifyCollectionChanged iNotifyCollectionChanged = listSeries.iList as INotifyCollectionChanged;
			if (iNotifyCollectionChanged != null)
				//iNotifyCollectionChanged.CollectionChanged += INotifyCollectionChanged_CollectionChanged;
				iNotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(delegate (object sender, NotifyCollectionChangedEventArgs e)
				{
					// can we remove this later when disposing?
					SeriesChanged(listSeries, e.NewItems, lineSeries);
				});

			ListToTabSeries[listSeries.iList] = listSeries;
			ListToTabIndex[listSeries.iList] = ListToTabIndex.Count;
		}

		private void AddPoints(ListSeries listSeries, IList iList, OxyPlot.Series.LineSeries lineSeries)
		{
			if (iList.Count == 0)
				return;

			if (listSeries.xPropertyInfo != null)
			{
				// faster than using ItemSource?
				foreach (PropertyInfo propertyInfo in iList[0].GetType().GetProperties())
				{
					if (propertyInfo.GetCustomAttribute<XAxisAttribute>() != null)
						xAxisPropertyInfo = propertyInfo;
				}
				foreach (object obj in iList)
				{
					object value = listSeries.xPropertyInfo.GetValue(obj);
					double x = lineSeries.Points.Count;
					if (xAxisPropertyInfo != null)
					{
						object xObj = xAxisPropertyInfo.GetValue(obj);
						if (xObj is DateTime dateTime)
						{
							x = OxyPlot.Axes.DateTimeAxis.ToDouble(dateTime);
						}
						else
						{
							x = (dynamic)xObj;
						}
					}
					lineSeries.Points.Add(new DataPoint(x, (dynamic)value));
				}
			}
			else
			{
				foreach (object obj in iList)
				{
					lineSeries.Points.Add(new DataPoint(lineSeries.Points.Count, (dynamic)obj));
				}
			}
		}

		private void TabData_OnSelectionChanged(object sender, EventArgs e)
		{
			UnloadModel();
			LoadPlotModel();
		}

		private void SeriesChanged(ListSeries listSeries, IList iList, OxyPlot.Series.LineSeries lineSeries)
		{
			lock (this.plotModel.SyncRoot)
			{
				//this.Update();
				AddPoints(listSeries, iList, lineSeries);
			}

			Dispatcher.UIThread.InvokeAsync(() => this.plotModel.InvalidatePlot(true), DispatcherPriority.Background);
		}
		/*private void INotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			lock (this.plotModel.SyncRoot)
			{
				//this.Update();
				int index = ListToTabIndex[(IList)sender];
				ListSeries listSeries = ListToTabSeries[(IList)sender];
				AddPoints((OxyPlot.Series.LineSeries)plotModel.Series[index], listSeries, e.NewItems);
			}

			Dispatcher.UIThread.InvokeAsync(() => this.plotModel.InvalidatePlot(true), DispatcherPriority.Background);
		}*/

		public void Dispose()
		{
			UnloadModel();
		}
	}
}
