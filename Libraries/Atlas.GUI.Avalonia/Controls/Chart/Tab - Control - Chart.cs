using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Linq;
using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Avalonia;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlChart : UserControl //, IDisposable
	{
		private TabInstance tabInstance;
		//public ChartSettings ChartSettings { get; set; }
		public ListGroup ListGroup { get; set; }

		//private List<ListSeries> ListSeries { get; set; }
		private Dictionary<IList, ListSeries> ListToTabSeries { get; set; } = new Dictionary<IList, ListSeries>();
		private Dictionary<IList, int> ListToTabIndex { get; set; } = new Dictionary<IList, int>(); // not used

		//public SeriesCollection SeriesCollection { get; set; }
		public string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }

		public PlotModel plotModel;
		public PlotView plotView;
		private PropertyInfo xAxisPropertyInfo;
		private TabControlChartLegend legend;
		public OxyPlot.Axes.LinearAxis valueAxis; // left/right?
		private OxyPlot.Axes.CategoryAxis categoryAxis;

		public OxyPlot.Axes.LinearAxis linearAxis;
		public OxyPlot.Axes.DateTimeAxis dateTimeAxis;

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

		public TabControlChart(TabInstance tabInstance, ListGroup listGroup)
		{
			this.tabInstance = tabInstance;
			this.ListGroup = listGroup;

			InitializeControls();
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

		private void InitializeControls()
		{
			this.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch; // OxyPlot import collision
			this.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
			//this.Orientation = Orientation.Vertical;
			MaxWidth = 1500;
			MaxHeight = 1000;

			// autogenerate columns
			if (tabInstance.tabViewSettings.ChartDataSettings.Count == 0)
				tabInstance.tabViewSettings.ChartDataSettings.Add(new TabDataSettings());
			
			plotView = new PlotView()
			{
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,

				Background = Brushes.Transparent,
				BorderBrush = Brushes.LightGray,
				IsMouseWheelEnabled = false,
				DisconnectCanvasWhileUpdating = false, // Tracker will show behind grid lines if the PlotView is resized and this is set
				MinHeight = 125,
				MinWidth = 150,
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

			containerGrid.Children.Add(plotView);

			legend = new TabControlChartLegend(this, ListGroup.Horizontal);
			if (ListGroup.Horizontal)
				Grid.SetRow(legend, 1);
			else
				Grid.SetColumn(legend, 1);
			containerGrid.Children.Add(legend);
			legend.OnSelectionChanged += Legend_OnSelectionChanged;

			this.Content = containerGrid;

			this.Focusable = true;
		}

		private void Legend_OnSelectionChanged(object sender, EventArgs e)
		{
			UpdateValueAxis();
		}

		public string GetDateTimeFormat(double duration)
		{
			if (duration <= 60)
				return "H:mm:ss";
			if (duration < 60 * 60)
				return "H:mm";
			if (duration < 24 * 60 * 60)
				return "H:mm";
			if (duration < 3 * 24 * 60 * 60)
				return "M/d H:mm";
			if (duration < 6 * 30 * 24 * 60 * 60)
				return "M/d";

			return "yyyy-M-d";
		}
		public void LoadListGroup(ListGroup listGroup)
		{
			ListGroup = listGroup;
			LoadPlotModel();
		}

		public void LoadPlotModel()
		{
			UnloadModel();
			RecreatePlotModel();

			//double duration = 0;
			foreach (ListSeries listSeries in ListGroup.ListSeries)
			{
				//duration = listSeries.iList[0]
				AddSeries(listSeries);
			}

			// would need to be able to disable to use
			//foreach (ListSeries listSeries in ChartSettings.ListSeries)
			//	AddSeries(listSeries);

			AddAxis();
			UpdateValueAxis();
			UpdateLinearAxis();

			plotView.Model = plotModel;
			this.IsVisible = true;
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
			UpdateLinearAxis();
			plotView.InvalidatePlot(true);
			plotView.Model.InvalidatePlot(true);
			legend.RefreshModel();
		}

		public void Unload()
		{
			this.IsVisible = false;
			UnloadModel();
		}

		private bool UseDateTimeAxis => (xAxisPropertyInfo?.PropertyType == typeof(DateTime)) ||
				(ListGroup.StartTime != null && ListGroup.EndTime != null);

		private void AddAxis()
		{
			if (UseDateTimeAxis)
			{
				AddDateTimeAxis(ListGroup.StartTime, ListGroup.EndTime);
			}
			else
			{
				AddLinearAxis();
			}
			if (ListGroup.ListSeries.Count > 0 && ListGroup.ListSeries[0].IsStacked)
				AddCategoryAxis();
			else
				AddValueAxis();
		}

		public OxyPlot.Axes.DateTimeAxis AddDateTimeAxis(DateTime? startTime = null, DateTime? endTime = null)
		{
			dateTimeAxis = new OxyPlot.Axes.DateTimeAxis
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
				double widthPerLabel = 6 * dateTimeAxis.StringFormat.Length + 25;
				dateTimeAxis.IntervalLength = Math.Max(50, widthPerLabel);
			}
			plotModel.Axes.Add(dateTimeAxis);
			return dateTimeAxis;
		}

		private void AddLinearAxis()
		{
			linearAxis = new OxyPlot.Axes.LinearAxis
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
				MajorGridlineStyle = LineStyle.Solid,
				MajorGridlineColor = GridLineColor,
				MinorGridlineStyle = LineStyle.None,
				IntervalLength = 25,
				//MinorStep = 20,
				//MajorStep = 10,
				//MinimumMinorStep = 20,
				MinorTickSize = 0,
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

		public OxyPlot.Axes.CategoryAxis AddCategoryAxis(AxisPosition axisPosition = AxisPosition.Left, string key = null)
		{
			categoryAxis = new OxyPlot.Axes.CategoryAxis
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
				categoryAxis.Key = key;

			foreach (ListSeries listSeries in ListGroup.ListSeries)
			{
				categoryAxis.Labels.Add(listSeries.Name);
			}

			plotModel.Axes.Add(categoryAxis);
			return categoryAxis;
		}

		private void UpdateLinearAxis()
		{
			if (linearAxis == null)
				return;

			double minimum = double.MaxValue;
			double maximum = double.MinValue;

			foreach (OxyPlot.Series.Series series in plotModel.Series)
			{
				if (series is OxyPlot.Series.LineSeries lineSeries)
				{
					if (lineSeries.LineStyle == LineStyle.None)
						continue;

					foreach (var dataPoint in lineSeries.Points)
					{
						double x = dataPoint.X;
						if (double.IsNaN(x))
							continue;

						minimum = Math.Min(minimum, x);
						maximum = Math.Max(maximum, x);
					}
				}
			}

			if (minimum == double.MaxValue)
			{
				// didn't find any values
				minimum = 0;
				maximum = 1;
			}

			linearAxis.Minimum = minimum;
			linearAxis.Maximum = maximum;
		}

		private void UpdateValueAxis() // OxyPlot.Axes.LinearAxis valueAxis, string axisKey = null
		{
			if (valueAxis == null)
				return;

			double minimum = double.MaxValue;
			double maximum = double.MinValue;
			bool hasFraction = false;

			foreach (OxyPlot.Series.Series series in plotModel.Series)
			{
				if (series is OxyPlot.Series.LineSeries lineSeries)
				{
					if (lineSeries.LineStyle == LineStyle.None)
						continue;

					foreach (var dataPoint in lineSeries.Points)
					{
						double y = dataPoint.Y;
						if (double.IsNaN(y))
							continue;

						hasFraction |= (y % 1 != 0.0);

						minimum = Math.Min(minimum, y);
						maximum = Math.Max(maximum, y);
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

			valueAxis.MinimumMajorStep = hasFraction ? 0 : 1;

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
				return string.Format("{0} T", d / 1E12);
			}
			else if (ad >= 1E9)
			{
				return string.Format("{0} G", d / 1E9);
			}
			else if (ad >= 1E6)
			{
				return string.Format("{0} M", d / 1E6);
			}
			else if (ad >= 1E3)
			{
				return string.Format("{0} K", d / 1E3);
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

		private void UnloadModel()
		{
			linearAxis = null;
			dateTimeAxis = null;
			//if (plotModel != null)
			//	plotModel.Series.Clear();
			/*foreach (ListSeries listSeries in ChartSettings.ListSeries)
			{
				INotifyCollectionChanged iNotifyCollectionChanged = listSeries.iList as INotifyCollectionChanged;
				//if (iNotifyCollectionChanged != null)
				//	iNotifyCollectionChanged.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;
			}*/
		}

		public void AddSeries(ListSeries listSeries)
		{
			if (listSeries.IsStacked)
				AddBarSeries(listSeries);
			else
				AddListSeries(listSeries);
		}

		private void AddBarSeries(ListSeries listSeries)
		{
			var barSeries = new OxyPlot.Series.BarSeries
			{
				Title = listSeries.Name,
				StrokeThickness = 2,
				FillColor = GetColor(plotModel.Series.Count),
				TextColor = OxyColors.Black,
				IsStacked = listSeries.IsStacked,
				TrackerFormatString = "{0}\nTime: {2:yyyy-M-d H:mm:ss.FFF}\nValue: {4:#,0.###}",
			};
			var dataPoints = GetDataPoints(listSeries, listSeries.iList);
			foreach (DataPoint dataPoint in dataPoints)
			{
				barSeries.Items.Add(new BarItem(dataPoint.X, (int)dataPoint.Y));
			}

			plotModel.Series.Add(barSeries);

			/*ListToTabSeries[listSeries.iList] = listSeries;
			ListToTabIndex[listSeries.iList] = ListToTabIndex.Count;*/
		}

		public OxyPlot.Series.LineSeries AddListSeries(ListSeries listSeries)
		{
			string xTrackerFormat = listSeries.xPropertyName ?? "Index" + ": {2:#,0.###}";
			if (UseDateTimeAxis || listSeries.xPropertyInfo?.PropertyType == typeof(DateTime))
				xTrackerFormat = "Time: {2:yyyy-M-d H:mm:ss.FFF}";
			var lineSeries = new OxyPlot.Series.LineSeries
			{
				Title = listSeries.Name,
				LineStyle = LineStyle.Solid,
				StrokeThickness = 2,
				Color = GetColor(plotModel.Series.Count),
				TextColor = OxyColors.Black,
				CanTrackerInterpolatePoints = false,
				MinimumSegmentLength = 2,
				MarkerSize = 3,
				//MarkerType = MarkerType.Circle,
				MarkerType = listSeries.iList.Count < 20 ? MarkerType.Circle : MarkerType.None,
				TrackerFormatString = "{0}\n" + xTrackerFormat + "\nValue: {4:#,0.###}",
			};
			// can't add gaps with ItemSource so convert to DataPoint ourselves
			var dataPoints = GetDataPoints(listSeries, listSeries.iList);
			lineSeries.Points.AddRange(dataPoints);

			// use circle markers if there's a single point all alone, otherwise it won't show
			if (HasSinglePoint(lineSeries))
				lineSeries.MarkerType = MarkerType.Circle;

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
			return lineSeries;
		}

		private bool HasSinglePoint(OxyPlot.Series.LineSeries lineSeries)
		{
			bool prevNan1 = false;
			bool prevNan2 = false;
			foreach (DataPoint dataPoint in lineSeries.Points)
			{
				bool nan = double.IsNaN(dataPoint.Y);
				if (prevNan2 && !prevNan1 && nan)
					return true;
				
				prevNan2 = prevNan1;
				prevNan1 = nan;
			}
			return false;
		}

		private void UpdateXAxisProperty(ListSeries listSeries)
		{
			if (listSeries.yPropertyInfo != null)
			{
				if (listSeries.xPropertyInfo != null)
					xAxisPropertyInfo = listSeries.xPropertyInfo;
				if (xAxisPropertyInfo == null)
				{
					Type elementType = listSeries.iList.GetType().GetElementTypeForAll();
					foreach (PropertyInfo propertyInfo in elementType.GetProperties())
					{
						if (propertyInfo.GetCustomAttribute<XAxisAttribute>() != null)
							xAxisPropertyInfo = propertyInfo;
					}
				}
			}
		}

		private List<DataPoint> GetDataPoints(ListSeries listSeries, IList iList)
		{
			UpdateXAxisProperty(listSeries);
			var dataPoints = new List<DataPoint>();
			if (listSeries.yPropertyInfo != null)
			{
				// faster than using ItemSource?
				foreach (object obj in iList)
				{
					object value = listSeries.yPropertyInfo.GetValue(obj);
					double x = dataPoints.Count;
					if (xAxisPropertyInfo != null)
					{
						object xObj = xAxisPropertyInfo.GetValue(obj);
						if (xObj is DateTime dateTime)
						{
							x = OxyPlot.Axes.DateTimeAxis.ToDouble(dateTime);
						}
						else if (xObj == null)
						{
							continue;
						}
						else
						{
							x = Convert.ToDouble(xObj);
						}
					}
					double d = double.NaN;
					if (value != null)
						d = Convert.ToDouble(value);
					dataPoints.Add(new DataPoint(x, d));
				}
				dataPoints = dataPoints.OrderBy(d => d.X).ToList();

				if (dataPoints.Count > 0 && listSeries.xBinSize > 0)
				{
					double firstBin = dataPoints[0].X;
					double lastBin = dataPoints[dataPoints.Count-1].X;
					int numBins = (int)Math.Ceiling((lastBin - firstBin) / listSeries.xBinSize) + 1;
					double[] bins = new double[numBins];
					foreach (DataPoint dataPoint in dataPoints)
					{
						int bin = (int)((dataPoint.X - firstBin) / listSeries.xBinSize);
						bins[bin] += dataPoint.Y;
					}
					dataPoints = new List<DataPoint>();
					for (int i = 0; i < numBins; i++)
					{
						dataPoints.Add(new DataPoint(firstBin + i * listSeries.xBinSize, bins[i]));
					}
				}
			}
			else
			{
				foreach (object obj in iList)
				{
					double value = Convert.ToDouble(obj);
					dataPoints.Add(new DataPoint(dataPoints.Count, value));
				}
			}
			return dataPoints;
		}

		private void TabData_OnSelectionChanged(object sender, EventArgs e)
		{
			UnloadModel();
			LoadPlotModel();
		}

		private void SeriesChanged(ListSeries listSeries, IList iList, OxyPlot.Series.LineSeries lineSeries)
		{
			lock (plotModel.SyncRoot)
			{
				//this.Update();
				var dataPoints = GetDataPoints(listSeries, iList);
				lineSeries.Points.AddRange(dataPoints);
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
