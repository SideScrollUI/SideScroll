using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Atlas.Core;

namespace Atlas.Tabs.Test.Chart
{
	public class TabTestChartList : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			//private ItemCollection<ListItem> items = new ItemCollection<ListItem>();
			private List<ItemCollection<int>> series = new List<ItemCollection<int>>();
			//private ItemCollection<double> samples = new ItemCollection<ChartSample>();
			private Random random = new Random();
			private bool ChartInitialized = false;

			public class TestItem
			{
				public int Amount { get; set; }
			}

			public override void Load(Call call)
			{
				//items.Add(new ListItem("Log", series));
				//tabModel.Items = items;

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add Entry", AddEntry),
					new TaskDelegate("Start: 1 Entry / second", StartTask, true),
				};

				ChartSettings chartSettings = new ChartSettings();
				for (int i = 0; i < 2; i++)
				{
					var list = new ItemCollection<int>();
					chartSettings.AddList("Series " + i, list);
					series.Add(list);
				}

				for (int i = 0; i < 10; i++)
				{
					AddSample(i);
				}
				tabModel.AddObject(chartSettings);
				//tabModel.ChartSettings.ListSeries.
			}

			private void AddEntry(Call call)
			{
				Invoke(call, AddSampleCallback);
			}

			private void StartTask(Call call)
			{
				CancellationToken token = call.taskInstance.tokenSource.Token;
				for (int i = 0; !token.IsCancellationRequested; i++)
				{
					Invoke(AddSampleCallback, call);
					Thread.Sleep(1000);
				}
			}

			private void AddSample(int i)
			{
				//series.Add(random.Next(1050, 1095));

				foreach (var list in series)
				{
					int amount = random.Next();
					list.Add(amount);
				}
			}

			private void Initialize()
			{
				if (ChartInitialized)
					return;
				ChartInitialized = true;

				//tabChart.chart.Series.Clear();
				//tabChart.BindListToChart(samples);

				//tabChart.chart.DataBindTable(samples); // databinds class Properties, throws exception when binding non Primitive properties
				//tabChart.chart.DataBind();


				//tabChart.chart.Update();
			}

			// GUI context
			private void AddSampleCallback(Call call, object state)
			{
				//state;
				Initialize();

				call.log.Add("test");
				//call.log.Add("New Log entry", new Tag("name", "value"));

				Random random = new Random();
				AddSample(series.Count);
				//tabChart.chart.Series[0].Points.DataBindY(tabModel.Chart); // required to refresh, any alternatives?
				//tabChart.chart.DataBind();
				//tabChart.chart.In
			}
		}
	}
}
/*
Can we bind sub-properties?
	Can we use instances if we can't?

*/
