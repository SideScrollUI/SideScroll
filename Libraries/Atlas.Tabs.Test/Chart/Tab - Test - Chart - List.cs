using Atlas.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Atlas.Tabs.Test.Chart
{
	public class TabTestChartList : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			//private ItemCollection<ListItem> items = new ItemCollection<ListItem>();
			private List<ItemCollection<int>> series = new List<ItemCollection<int>>();
			private Random random = new Random();
			private bool ChartInitialized = false;

			public class TestItem
			{
				public int Amount { get; set; }
			}

			public override void Load(Call call, TabModel model)
			{
				//items.Add(new ListItem("Log", series));
				//model.Items = items;
				model.MinDesiredWidth = 600;

				model.Actions =  new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add Entry", AddEntry),
					new TaskDelegate("Start: 1 Entry / second", StartTask, true),
				};

				ChartSettings chartSettings = new ChartSettings();
				for (int i = 0; i < 3; i++)
				{
					var list = new ItemCollection<int>();
					chartSettings.AddList("Series " + i, list);
					series.Add(list);
				}

				for (int i = 0; i < 10; i++)
				{
					AddSample();
				}
				model.AddObject(chartSettings);
				//tabModel.ChartSettings.ListSeries.
			}

			private void AddEntry(Call call)
			{
				Invoke(call, AddSampleCallback);
			}

			private void StartTask(Call call)
			{
				CancellationToken token = call.TaskInstance.tokenSource.Token;
				for (int i = 0; !token.IsCancellationRequested; i++)
				{
					Invoke(AddSampleCallback, call);
					Thread.Sleep(1000);
				}
			}

			private void AddSample()
			{
				int multiplier = 1;
				foreach (var list in series)
				{
					int amount = (random.Next() % 1000) * multiplier;
					list.Add(amount);
					multiplier++;
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

			// UI context
			private void AddSampleCallback(Call call, object state)
			{
				Initialize();

				AddSample();
			}
		}
	}
}
