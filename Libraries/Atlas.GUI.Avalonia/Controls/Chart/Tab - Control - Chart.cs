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

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlChart : UserControl //, IDisposable
	{
		//private string name;
		private TabInstance tabInstance;
		public ChartSettings ChartSettings { get; set; }
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

		private TabControlDataGrid tabControlDataGrid;
		private PlotModel plotModel;
		private PlotView plotView;
		private PropertyInfo xAxisPropertyInfo;

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

		public TabControlChart(TabInstance tabInstance, ChartSettings chartSettings, ListGroup listGroup)
		{
			this.tabInstance = tabInstance;
			this.ChartSettings = chartSettings;
			this.ListGroup = listGroup;

			InitializeControls();
			//DataContext = new TestNode().Children;
		}

		public override string ToString()
		{
			return ChartSettings.ToString(); // todo: fix for multiple
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

			// autogenerate columns
			if (tabInstance.tabViewSettings.ChartDataSettings.Count == 0)
				tabInstance.tabViewSettings.ChartDataSettings.Add(new TabDataSettings());
			//tabDataGrid = new TabDataGrid(tabInstance, ChartSettings.ListSeries, true, tabInstance.tabViewSettings.ChartDataSettings);
			tabControlDataGrid = new TabControlDataGrid(tabInstance, ListGroup.ListSeries, true, tabInstance.tabViewSettings.ChartDataSettings[0]);
			//Grid.SetRow(tabDataGrid, 1);

			//tabDataGrid.AddButtonColumn("<>", nameof(TaskInstance.Cancel));

			//tabDataGrid.AutoLoad = tabModel.AutoLoad;
			tabControlDataGrid.OnSelectionChanged += TabData_OnSelectionChanged;
			//tabDataGrid.Width = 1000;
			//tabDataGrid.Height = 1000;
			//tabDataGrid.Initialize();
			//bool addSplitter = false;
			//tabParentControls.AddControl(tabDataGrid, true, false);
			
			plotView = new PlotView()
			{
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,
				MaxWidth = 1000,
				MaxHeight = 1000,
				MinHeight = 200,
				
				//[Grid.RowProperty] = 1,
				[Grid.ColumnProperty] = 1,

				//
				//Background = new SolidColorBrush(Colors.White),

				Background = Brushes.Transparent,

				//Foreground = Brushes.LightGray,
				BorderBrush = Brushes.LightGray,
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
				ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
				RowDefinitions = new RowDefinitions("*"), // Header, Body
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
			};
			//containerGrid.Children.Add(borderTitle);

			containerGrid.Children.Add(tabControlDataGrid);
			containerGrid.Children.Add(plotView);

			/*var legend = new TabControlChartLegend(plotView)
			{
				[Grid.ColumnProperty] = 2,
			};
			containerGrid.Children.Add(legend);*/

			//this.watch.Start();
			this.Content = containerGrid;

			this.Focusable = true;
		}

		public string GetDateTimeFormat(double duration)
		{
			if (duration < 60)
				return "H:mm:ss";
			if (duration < 60 * 60)
				return "H:mm";
			if (duration < 24 * 60 * 60)
				return "H:mm";
			if (duration < 30 * 24 * 60 * 60)
				return "M/d";

			return "yyyy-M-d";
		}

		private void LoadPlotModel()
		{
			UnloadModel();

			plotModel = new OxyPlot.PlotModel()
			{
				//Title = name,
				LegendPlacement = LegendPlacement.Outside,
				TitleColor = OxyColors.LightGray,
				PlotAreaBorderColor = OxyColors.LightGray,
				TextColor = OxyColors.Black,
				LegendTextColor = OxyColors.LightGray,
				SelectionColor = OxyColors.Blue,
			};

			//double duration = 0;
			foreach (ListSeries listSeries in tabControlDataGrid.SelectedItems)
			{
				//duration = listSeries.iList[0]
				AddSeries(listSeries);
			}
			AddAxis();

			// would need to be able to disable to use
			//foreach (ListSeries listSeries in ChartSettings.ListSeries)
			//	AddSeries(listSeries);

			plotView.Model = plotModel;
		}

		private void AddAxis()
		{
			if (xAxisPropertyInfo != null && xAxisPropertyInfo.PropertyType == typeof(DateTime))
			{
				var dateTimeAxis = new OxyPlot.Axes.DateTimeAxis
				{
					Position = AxisPosition.Bottom,
					//StringFormat = GetDateTimeFormat(duration),
					//MinorIntervalType = DateTimeIntervalType.Days,
					//IntervalType = DateTimeIntervalType.Days,
					IntervalType = DateTimeIntervalType.Hours,
					MajorGridlineStyle = LineStyle.Solid,
					MajorGridlineColor = OxyColors.Gray,
					//MinorGridlineStyle = LineStyle.None,
					IntervalLength = 100,
					IsAxisVisible = true,
					AxislineColor = OxyColors.Black,
					AxislineStyle = LineStyle.Solid,
					AxislineThickness = 2,
					TickStyle = TickStyle.Outside,
					TicklineColor = OxyColors.Black,
					MajorTickSize = 5,
					MinorTicklineColor = OxyColors.Black,
					MinorTickSize = 5,
					AxisTickToLabelDistance = 2,
					TitleColor = OxyColors.LightGray,
					TextColor = OxyColors.LightGray,
				};
				plotModel.Axes.Add(dateTimeAxis);
				//plotModel.Axes.Add(new OxyPlot.Axes.DateTimeAxis { Position = AxisPosition.Bottom });
			}
			else
			{
				var linearAxis = new OxyPlot.Axes.LinearAxis
				{
					Position = AxisPosition.Bottom,
					MajorGridlineStyle = LineStyle.Solid,
					MajorGridlineColor = OxyColors.Gray,
					TitleColor = OxyColors.LightGray,
					TextColor = OxyColors.LightGray,
				};
				plotModel.Axes.Add(linearAxis);
			}

			var valueAxis = new OxyPlot.Axes.LinearAxis
			{
				Position = AxisPosition.Left,
				//Minimum = minimum - margin,
				//Maximum = maximum + margin,
				IntervalLength = 20,
				MajorGridlineStyle = LineStyle.Solid,
				MajorGridlineColor = OxyColors.Gray,
				MinorGridlineStyle = LineStyle.None,
				MinorTickSize = 0,
				//MinorStep = 10,
				MinimumMinorStep = 10,
				IsAxisVisible = true,
				AxislineColor = OxyColors.Black,
				AxislineStyle = LineStyle.Solid,
				AxislineThickness = 2,
				TickStyle = TickStyle.Outside,
				TicklineColor = OxyColors.Black,
				MajorTickSize = 2,
				TitleColor = OxyColors.LightGray,
				TextColor = OxyColors.LightGray,
			};
			plotModel.Axes.Add(valueAxis);
			{
				//plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left });
			}
		}

		private void UnloadModel()
		{
			//if (plotModel != null)
			//	plotModel.Series.Clear();
			foreach (ListSeries listSeries in ChartSettings.ListSeries)
			{
				INotifyCollectionChanged iNotifyCollectionChanged = listSeries.iList as INotifyCollectionChanged;
				//if (iNotifyCollectionChanged != null)
				//	iNotifyCollectionChanged.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;
			}
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
			};
			AddPoints(listSeries, listSeries.iList, lineSeries);

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

			if (listSeries.propertyInfo != null)
			{
				foreach (PropertyInfo propertyInfo in iList[0].GetType().GetProperties())
				{
					if (propertyInfo.GetCustomAttribute<XAxisAttribute>() != null)
						xAxisPropertyInfo = propertyInfo;
				}
				foreach (object obj in iList)
				{
					object value = listSeries.propertyInfo.GetValue(obj);
					double x = lineSeries.Points.Count;
					if (xAxisPropertyInfo != null)
					{
						object xObj = xAxisPropertyInfo.GetValue(obj);
						if (xObj.GetType() == typeof(DateTime))
						{
							x = OxyPlot.Axes.DateTimeAxis.ToDouble((DateTime)xObj);
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
