using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Avalonia;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Threading;

namespace Atlas.UI.Avalonia
{
	public class TabChartGroup : ITab
	{
		//private string name;
		public ChartSettings ChartSettings { get; set; }
		//private List<ListSeries> ListSeries { get; set; }
		//private Dictionary<IList, ListSeries> ListToListSeries { get; set; } = new Dictionary<IList, ListSeries>();
		//private Dictionary<IList, int> ListToTabIndex { get; set; } = new Dictionary<IList, int>(); // not used
		private Dictionary<ListGroup, TabControlChart> ListGroupToTabChart { get; set; } = new Dictionary<ListGroup, TabControlChart>();

		//public SeriesCollection SeriesCollection { get; set; }
		//public string[] Labels { get; set; }
		//public Func<double, string> YFormatter { get; set; }

		// try to change might be lower or higher than the rendering interval
		//private const int UpdateInterval = 20;

		//private bool disposed;
		//private readonly Timer timer;
		//private readonly Stopwatch watch = new Stopwatch();
		//private int numberOfSeries;

		//private TabControlDataGrid tabDataGrid;


		//public event EventHandler<EventArgs> OnSelectionChanged;
		//private bool autoSelectNew = true;

		public TabChartGroup(ChartSettings chartSettings)
		{
			ChartSettings = chartSettings;
		}


		public TabInstance Create() => new Instance(this);

		public class Instance : TabInstance
		{
			public TabChartGroup Tab;
			private TabControlDataGrid tabDataGrid;

			public Instance(TabChartGroup tab)
			{
				Tab = tab;
			}

			//private ItemCollection<ListItem> items = new ItemCollection<ListItem>();
			//private CustomControl control;
			//private TabChart tabChart;

			public override void LoadUI(Call call, TabModel model)
			{
				if (TabViewSettings.ChartDataSettings.Count == 0)
					TabViewSettings.ChartDataSettings.Add(new TabDataSettings());

				//Background = new SolidColorBrush(Theme.BackgroundColor);
				//HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch; // OxyPlot import collision
				//VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
				//Width = 1000;
				//Height = 1000;
				//Children.Add(border);
				//Orientation = Orientation.Vertical;

				// autogenerate columns
				tabDataGrid = new TabControlDataGrid(this, Tab.ChartSettings.ListSeries, true, TabViewSettings.ChartDataSettings[0], model);
				//Grid.SetRow(tabDataGrid, 1);

				//tabDataGrid.AddButtonColumn("<>", nameof(TaskInstance.Cancel));

				//tabDataGrid.AutoLoad = tabModel.AutoLoad;
				tabDataGrid.OnSelectionChanged += TabData_OnSelectionChanged;
				//tabDataGrid.Width = 1000;
				//tabDataGrid.Height = 1000;
				//tabDataGrid.Initialize();
				//bool addSplitter = false;
				//tabParentControls.AddControl(tabDataGrid, true, false);

				LoadSelectedCharts();
				/*plotView.Template = new ControlTemplate() // todo: fix
				{
					Content = new object(),
					TargetType = typeof(object),
				};*/

				Model.AddObject(tabDataGrid);
			}

			private void LoadSelectedCharts()
			{
				foreach (ListSeries listSeries in tabDataGrid.SelectedItems)
				{
					AddSeries(listSeries);
				}

				// would need to be able to disable to use
				//foreach (ListSeries listSeries in ChartSettings.ListSeries)
				//	AddSeries(listSeries);
			}

			private void AddSeries(ListSeries listSeries)
			{
				//TabChart tabChart;
				//if (ListGroupToTabChart.TryGetValue(list)
			}

			private void UnloadCharts()
			{

			}

			private void TabData_OnSelectionChanged(object sender, EventArgs e)
			{
				UnloadCharts();
				LoadSelectedCharts();
			}
		}
	}
}


/*
Still in progress
--
 -
 -
 -
 -
 -

--
 -
 -
 -
 -

--
*/
