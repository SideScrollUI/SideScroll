using Atlas.Core;
using Atlas.Tabs;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace Atlas.UI.Wpf
{
	public partial class TabChart : UserControl, IDisposable
	{
		public TabModel TabModel { get; set; }
		public ChartSettings ChartSettings { get; set; }

		public SeriesCollection SeriesCollection { get; set; }
		public string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }

		public TabChart(TabModel tabModel, ChartSettings chartSettings)
		{
			this.TabModel = tabModel;
			this.ChartSettings = chartSettings;

			InitializeComponent();

			this.SeriesCollection = new SeriesCollection();

			//Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
			//YFormatter = value => value.ToString("C");

			//modifying the series collection will animate and update the chart
			foreach (ListSeries listSeries in ChartSettings.ListSeries)
				AddSeries(listSeries);
			//LineSeries series = new LineSeries();
			//series.Values.AddRange(list);
			/*foreach (LineSeries series in SeriesCollection)
			{
				for (int i = 0; i < 100; i++)
				{
					series.Values.Add((double)i);
				}
			}*/

			//modifying any series values will also animate and update the chart
			//SeriesCollection[0].Values.Add(5d);

			DataContext = this;
		}

		private void AddSeries(ListSeries listSeries)
		{
			LineSeries lineSeries = new LineSeries
			{
				Title = listSeries.Name,
				Values = new ChartValues<double>(), // use specific type?
				LineSmoothness = 0, //0: straight lines, 1: really smooth lines
									//PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
									//PointGeometrySize = 2,
									//PointForeground = Brushes.Gray
				PointGeometry = null,
				//Fill = null, // how to disable 
			};

			AddItems(lineSeries, listSeries, listSeries.iList);

			INotifyCollectionChanged iNotifyCollectionChanged = listSeries.iList as INotifyCollectionChanged;
			if (iNotifyCollectionChanged != null)
			{
				iNotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(delegate (object sender, NotifyCollectionChangedEventArgs e)
				{
					// can we remove this later when disposing?
					SeriesChanged(listSeries, e.NewItems, lineSeries);
				});
			}
			//iNotifyCollectionChanged.CollectionChanged += INotifyCollectionChanged_CollectionChanged;

			this.SeriesCollection.Add(lineSeries);
		}

		private void AddItems(LineSeries lineSeries, ListSeries listSeries, IList iList)
		{
			if (listSeries.yPropertyInfo != null)
			{
				foreach (object obj in iList)
				{
					object value = listSeries.yPropertyInfo.GetValue(obj);
					lineSeries.Values.Add((double)(dynamic)value);
				}
			}
			else
			{
				foreach (object obj in iList)
				{
					lineSeries.Values.Add((double)(dynamic)obj);
				}
			}
		}

		/*private void INotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			AddItems((LineSeries)SeriesCollection[0], ChartSettings.ListSeries[0], e.NewItems);
		}*/

		private void SeriesChanged(ListSeries listSeries, IList iList, LineSeries lineSeries)
		{
			/*lock (this.plotModel.SyncRoot)
			{
				//this.Update();
				AddPoints(listSeries, iList, lineSeries);
			}*/
			AddItems(lineSeries, listSeries, iList);

			//Dispatcher.UIThread.InvokeAsync(() => this.plotModel.InvalidatePlot(true), DispatcherPriority.Background);
		}

		public void Dispose()
		{
			foreach (ListSeries listSeries in ChartSettings.ListSeries)
			{
				INotifyCollectionChanged iNotifyCollectionChanged = listSeries.iList as INotifyCollectionChanged;
				//if (iNotifyCollectionChanged != null)
				//	iNotifyCollectionChanged.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;
			}
		}
	}
}
