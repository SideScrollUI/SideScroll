using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Atlas.Tabs.Test.Chart;

public class TabTestChartOverlay : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private readonly ItemCollection<ChartSample> _samples = new();
		private readonly Random _random = new();
		private readonly DateTime _baseDateTime = DateTime.Now.Trim(TimeSpan.FromMinutes(1));

		public class TestItem
		{
			public int Amount { get; set; }
		}

		public class ChartSample
		{
			public string Name { get; set; }

			[XAxis]
			public DateTime TimeStamp { get; set; }

			[Unit("B")]
			public int SeriesAlpha { get; set; }

			[Unit("A")]
			public int SeriesBeta { get; set; }

			[Unit("A")]
			public int SeriesGamma { get; set; }

			[Unit("B")]
			public int SeriesEpsilon { get; set; }  // High Value, small delta

			public TestItem TestItem { get; set; } = new();

			public int InstanceAmount => TestItem.Amount;
		}

		public override void Load(Call call, TabModel model)
		{
			//tabModel.Items = items;

			model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Add Entry", AddEntry),
					new TaskDelegate("Start: 1 Entry / second", StartTask, true),
				};

			for (int i = 0; i < 10; i++)
			{
				AddSample(i);
			}
			var chartSettings = new ChartSettings(_samples);
			model.AddObject(chartSettings, true);
		}

		private void AddEntry(Call call)
		{
			int param1 = 1;
			string param2 = "abc";
			Invoke(call, AddSampleUI, param1, param2);
		}

		private void StartTask(Call call)
		{
			CancellationToken token = call.TaskInstance.TokenSource.Token;
			for (int i = 0; i < 20 && !token.IsCancellationRequested; i++)
			{
				Invoke(call, AddSampleUI);
				Thread.Sleep(1000);
			}
		}

		private void AddSample(int i)
		{
			var sample = new ChartSample()
			{
				Name = "Name " + i.ToString(),
				TimeStamp = _baseDateTime.AddMinutes(i),
				SeriesAlpha = _random.Next(50, 100),
				SeriesBeta = _random.Next(50, 100),
				SeriesGamma = _random.Next(50, 100),
				SeriesEpsilon = _random.Next(50, 100),
				TestItem = new TestItem()
				{
					Amount = _random.Next(0, 100),
				},
			};
			_samples.Add(sample);
		}

		// UI context
		private void AddSampleUI(Call call, object state)
		{
			AddSample(_samples.Count);
		}
	}
}
