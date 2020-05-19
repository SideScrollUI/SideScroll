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
using OxyPlot.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Linq;

namespace Atlas.UI.Avalonia.Controls
{
	public class OxyListSeries
	{
		public ListSeries ListSeries { get; set; }
		public OxyPlot.Series.Series OxySeries { get; set; }
		public bool IsVisible { get; set; }

		public OxyListSeries(ListSeries listSeries, OxyPlot.Series.Series oxySeries)
		{
			ListSeries = listSeries;
			OxySeries = oxySeries;
			IsVisible = true;
		}
	}

	public class SeriesSelectedEventArgs : EventArgs
	{
		public List<ListSeries> Series { get; set; }

		public SeriesSelectedEventArgs(List<ListSeries> series)
		{
			Series = series;
		}
	}

	public class TabControlChart : UserControl //, IDisposable
	{
		private const double MarginPercent = 0.1;
		private static OxyColor nowColor = OxyColors.Green;

		private TabInstance tabInstance;
		//public ChartSettings ChartSettings { get; set; }
		public ListGroup ListGroup { get; set; }

		//private List<ListSeries> ListSeries { get; set; }
		public List<OxyListSeries> oxyListSeriesList = new List<OxyListSeries>();
		private Dictionary<IList, ListSeries> ListToTabSeries { get; set; } = new Dictionary<IList, ListSeries>();
		private Dictionary<IList, int> ListToTabIndex { get; set; } = new Dictionary<IList, int>(); // not used
		public List<ListSeries> SelectedSeries
		{
			get
			{
				var selected = new List<ListSeries>();
				foreach (var oxyListSeries in oxyListSeriesList)
				{
					if (oxyListSeries.IsVisible)
						selected.Add(oxyListSeries.ListSeries);
				}
				if (selected.Count == oxyListSeriesList.Count)
					selected.Clear(); // If all are selected, none are selected?
				return selected;
			}
		}

		//public SeriesCollection SeriesCollection { get; set; }

		public PlotModel plotModel;
		public PlotView plotView;
		private PropertyInfo xAxisPropertyInfo;
		public TabControlChartLegend legend;
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

		public event EventHandler<SeriesSelectedEventArgs> OnSelectionChanged;

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

		private void InitializeControls()
		{
			HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch; // OxyPlot import collision
			VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
			MaxWidth = 1500;
			MaxHeight = 600;

			if (tabInstance.tabViewSettings.ChartDataSettings.Count == 0)
				tabInstance.tabViewSettings.ChartDataSettings.Add(new TabDataSettings());
			
			plotView = new PlotView()
			{
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,

				Background = Brushes.Transparent,
				BorderBrush = Brushes.LightGray,
				IsMouseWheelEnabled = false,
				//DisconnectCanvasWhileUpdating = false, // Tracker will show behind grid lines if the PlotView is resized and this is set
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

			var containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*,Auto"),
				RowDefinitions = new RowDefinitions("*,Auto"),
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,
				Background = new SolidColorBrush(Theme.BackgroundColor), // grid lines look bad when hovering without this
			};

			containerGrid.Children.Add(plotView);

			legend = new TabControlChartLegend(this, ListGroup.Horizontal);
			if (ListGroup.Horizontal)
			{
				Grid.SetRow(legend, 1);
				legend.MaxHeight = 100;
			}
			else
			{
				Grid.SetColumn(legend, 1);
				legend.MaxWidth = 300;
			}
			containerGrid.Children.Add(legend);
			legend.OnSelectionChanged += Legend_OnSelectionChanged;

			Content = containerGrid;

			Focusable = true;
		}

		private void Legend_OnSelectionChanged(object sender, EventArgs e)
		{
			UpdateValueAxis();
			OnSelectionChanged?.Invoke(sender, new SeriesSelectedEventArgs(SelectedSeries));
		}

		public void LoadListGroup(ListGroup listGroup)
		{
			ListGroup = listGroup;
			LoadPlotModel();
			Refresh();
		}

		public void LoadPlotModel()
		{
			UnloadModel();
			RecreatePlotModel();

			foreach (ListSeries listSeries in ListGroup.ListSeries)
			{
				AddSeries(listSeries);
			}

			// would need to be able to disable to use
			//foreach (ListSeries listSeries in ChartSettings.ListSeries)
			//	AddSeries(listSeries);

			AddAxis();
			UpdateValueAxis();
			UpdateLinearAxis();

			plotView.Model = plotModel;
			IsVisible = true;
		}

		public void RecreatePlotModel()
		{
			UnloadModel();
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
			legend.RefreshModel();
			plotView.InvalidatePlot(true);
			plotView.Model.InvalidatePlot(true);
		}

		public void Unload()
		{
			IsVisible = false;
			UnloadModel();
		}

		private bool UseDateTimeAxis => (xAxisPropertyInfo?.PropertyType == typeof(DateTime)) ||
				(ListGroup.StartTime != null && ListGroup.EndTime != null);

		private void AddAxis()
		{
			if (UseDateTimeAxis)
			{
				AddDateTimeAxis(ListGroup.StartTime, ListGroup.EndTime);
				AddNowTime();
				//AddTrackerLine();
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
				IsPanEnabled = false,
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
				//MinimumMajorStep = TimeSpan.FromSeconds(1).TotalDays,
				TitleColor = OxyColors.LightGray,
				TextColor = OxyColors.LightGray,
			};
			if (startTime != null && endTime != null)
			{
				double duration = endTime.Value.Subtract(startTime.Value).TotalSeconds;
				dateTimeAxis.Minimum = OxyPlot.Axes.DateTimeAxis.ToDouble(startTime.Value);
				dateTimeAxis.Maximum = OxyPlot.Axes.DateTimeAxis.ToDouble(endTime.Value);
				UpdateDateTimeInterval(duration);
			}
			plotModel.Axes.Add(dateTimeAxis);
			return dateTimeAxis;
		}

		private void UpdateDateTimeInterval(double duration)
		{
			var dateFormat = GetDateTimeFormat(duration);
			dateTimeAxis.StringFormat = dateFormat.TextFormat;
			dateTimeAxis.MinimumMajorStep = dateFormat.StepSize.TotalDays;
			double widthPerLabel = 6 * dateTimeAxis.StringFormat.Length + 25;
			dateTimeAxis.IntervalLength = Math.Max(50, widthPerLabel);
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
				IsPanEnabled = false,
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
				IsPanEnabled = false,
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

			foreach (OxyPlot.Annotations.Annotation annotation in plotModel.Annotations)
			{
				if (annotation is OxyPlot.Annotations.LineAnnotation lineAnnotation)
					maximum = Math.Max(lineAnnotation.Y * 1.1, maximum);
			}

			valueAxis.MinimumMajorStep = hasFraction ? 0 : 1;

			if (ListGroup.MinValue is double minValue)
				minimum = minValue;

			var margin = (maximum - minimum) * MarginPercent;
			if (minimum == maximum)
				margin = Math.Abs(minimum);

			valueAxis.Minimum = minimum - margin;
			valueAxis.Maximum = maximum + margin;
		}

		private void UpdateDateTimeAxis()
		{
			if (dateTimeAxis == null)
				return;

			//UpdateDateTimeInterval(double duration);
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
			else
			{
				return d.Formatted();
			}
		}

		public class DateTimeFormat
		{
			public double Maximum { get; set; }
			public TimeSpan StepSize { get; set; }
			public string TextFormat { get; set; }

			public DateTimeFormat(double maximum, TimeSpan stepSize, string textFormat)
			{
				Maximum = maximum;
				StepSize = stepSize;
				TextFormat = textFormat;
			}
		}

		public List<DateTimeFormat> dateFormats = new List<DateTimeFormat>
		{
			new DateTimeFormat(2 * 60, TimeSpan.FromSeconds(1), "H:mm:ss"),
			//new DateTimeFormat(60 * 60, 1, "H:mm"),
			new DateTimeFormat(24 * 60 * 60, TimeSpan.FromMinutes(1), "H:mm"),
			new DateTimeFormat(3 * 24 * 60 * 60, TimeSpan.FromMinutes(1), "M/d H:mm"),
			new DateTimeFormat(6 * 30 * 24 * 60 * 60, TimeSpan.FromDays(1), "M/d"),
			new DateTimeFormat(1000.0 * 12 * 30 * 24 * 60 * 60, TimeSpan.FromDays(1), "yyyy-M-d"),
		};

		public DateTimeFormat GetDateTimeFormat(double duration)
		{
			foreach (var format in dateFormats)
			{
				if (duration < format.Maximum)
					return format;
			}

			return null;
		}

		private void UnloadModel()
		{
			plotView.Model = null;
			linearAxis = null;
			dateTimeAxis = null;
			legend?.Unload();
			oxyListSeriesList.Clear();
			ListToTabSeries.Clear();
			ListToTabIndex.Clear();
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
			string xTrackerFormat = listSeries.xPropertyName ?? "Index: {2:#,0.###}";
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

			var oxyListSeries = new OxyListSeries(listSeries, lineSeries);

			if (listSeries.iList is INotifyCollectionChanged iNotifyCollectionChanged)
				//iNotifyCollectionChanged.CollectionChanged += INotifyCollectionChanged_CollectionChanged;
				iNotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(delegate (object sender, NotifyCollectionChangedEventArgs e)
				{
					// can we remove this later when disposing?
					SeriesChanged(listSeries, e.NewItems, lineSeries);
				});

			oxyListSeriesList.Add(oxyListSeries);
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
					dataPoints = BinDataPoints(listSeries, dataPoints);
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

		private static List<DataPoint> BinDataPoints(ListSeries listSeries, List<DataPoint> dataPoints)
		{
			double firstBin = dataPoints.First().X;
			double lastBin = dataPoints.Last().X;
			int numBins = (int)Math.Ceiling((lastBin - firstBin) / listSeries.xBinSize) + 1;
			double[] bins = new double[numBins];
			foreach (DataPoint dataPoint in dataPoints)
			{
				int bin = (int)((dataPoint.X - firstBin) / listSeries.xBinSize);
				bins[bin] += dataPoint.Y;
			}
			var binDataPoints = new List<DataPoint>();
			for (int i = 0; i < numBins; i++)
			{
				binDataPoints.Add(new DataPoint(firstBin + i * listSeries.xBinSize, bins[i]));
			}

			return binDataPoints;
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

			Dispatcher.UIThread.InvokeAsync(() => plotModel.InvalidatePlot(true), DispatcherPriority.Background);
		}

		private void AddNowTime()
		{
			var now = DateTime.UtcNow;
			if (ListGroup.EndTime < now.AddMinutes(1))
				return;
			var annotation = new OxyPlot.Annotations.LineAnnotation
			{
				Type = LineAnnotationType.Vertical,
				X = OxyPlot.Axes.DateTimeAxis.ToDouble(now.ToUniversalTime()),
				Color = nowColor,
				// LineStyle = LineStyle.Dot, // doesn't work for vertical?
			};

			plotModel.Annotations.Add(annotation);
		}

		private OxyPlot.Annotations.LineAnnotation trackerAnnotation;
		private void AddTrackerLine()
		{
			var now = DateTime.UtcNow;
			if (ListGroup.EndTime < now.AddMinutes(1))
				return;
			trackerAnnotation = new OxyPlot.Annotations.LineAnnotation
			{
				Type = LineAnnotationType.Vertical,
				//X = OxyPlot.Axes.DateTimeAxis.ToDouble(now.ToUniversalTime()),
				Color = nowColor,
				// LineStyle = LineStyle.Dot, // doesn't work for vertical?
			};

			plotModel.Annotations.Add(trackerAnnotation);
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
