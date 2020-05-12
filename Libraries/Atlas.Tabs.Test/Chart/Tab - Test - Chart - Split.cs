using Atlas.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Atlas.Tabs.Test.Chart
{
	public class TabTestChartSplit : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			//private ItemCollection<ListItem> items = new ItemCollection<ListItem>();
			//private ItemCollection<int> series = new ItemCollection<int>();
			private ItemCollection<ChartSample> samples = new ItemCollection<ChartSample>();
			private Random random = new Random();
			private bool ChartInitialized = false;

			public class TestItem
			{
				public int Amount { get; set; }
			}

			public class ChartSample
			{
				//public string Group { get; set; } = "Group";
				public string Name { get; set; }
				// Add [UnitType]
				public int SeriesAlpha { get; set; }
				// Add [UnitType]
				public int SeriesBeta { get; set; }
				// Add [UnitType]
				public int SeriesGamma { get; set; }
				// Add [UnitType]
				public int SeriesEpsilon { get; set; }  // High Value, small delta
				public TestItem testItem { get; set; } = new TestItem();
				public int InstanceAmount => testItem.Amount;
			}

			public override void Load(Call call, TabModel model)
			{
				//items.Add(new ListItem("Log", series));
				//tabModel.Items = items;

				model.Actions =  new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add Entry", AddEntry),
					new TaskDelegate("Start: 1 Entry / second", StartTask, true),
				};

				for (int i = 0; i < 10; i++)
				{
					AddSample(i);
				}

				ChartSettings chartSettings = new ChartSettings(samples);
				model.AddObject(chartSettings);
				//model.ChartSettings.ListSeries.
			}

			private void AddEntry(Call call)
			{
				Invoke(new SendOrPostCallback(AddSampleCallback), call);
				//context.Send(, log);
			}

			private void StartTask(Call call)
			{
				CancellationToken token = call.TaskInstance.tokenSource.Token;
				for (int i = 0; !token.IsCancellationRequested; i++)
				{
					Invoke(new SendOrPostCallback(AddSampleCallback), call);
					Thread.Sleep(1000);
				}
			}

			private void AddSample(int i)
			{
				//series.Add(random.Next(1050, 1095));

				var sample = new ChartSample()
				{
					Name = "Name " + i.ToString(),
					SeriesAlpha = random.Next(0, 100),
					SeriesBeta = random.Next(50, 100),
					SeriesGamma = random.Next(0, 1000000000),
					SeriesEpsilon = 1000000000 + random.Next(0, 10),
					testItem = new TestItem()
					{
						Amount = random.Next(0, 100),
					},
				};
				samples.Add(sample);
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
			private void AddSampleCallback(object state)
			{
				Call call = (Call)state;
				Initialize();

				call.Log.Add("test");
				//call.log.Add("New Log entry", new Tag("name", "value"));

				Random random = new Random();
				AddSample(samples.Count);
				//tabChart.chart.Series[0].Points.DataBindY(tabModel.Chart); // required to refresh, any alternatives?
				//tabChart.chart.DataBind();
				//tabChart.chart.In
			}
		}
	}
}
