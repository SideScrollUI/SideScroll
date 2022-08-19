using Atlas.Core;

namespace Atlas.Tabs.Test.Chart;

public class TabTestChartSplit : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private readonly ItemCollection<ChartSample> _samples = new();
		private readonly Random _random = new();

		public class TestItem
		{
			public int Amount { get; set; }
		}

		public class ChartSample
		{
			public string? Name { get; set; }
			// Add [UnitType]
			public int SeriesAlpha { get; set; }
			// Add [UnitType]
			public int SeriesBeta { get; set; }
			// Add [UnitType]
			public int SeriesGamma { get; set; }
			// Add [UnitType]
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
			model.AddObject(chartSettings);
		}

		private void AddEntry(Call call)
		{
			Invoke(AddSampleCallback, call);
		}

		private void StartTask(Call call)
		{
			CancellationToken token = call.TaskInstance!.TokenSource.Token;
			for (int i = 0; !token.IsCancellationRequested; i++)
			{
				Invoke(AddSampleCallback, call);
				Thread.Sleep(1000);
			}
		}

		private void AddSample(int i)
		{
			ChartSample sample = new()
			{
				Name = "Name " + i.ToString(),
				SeriesAlpha = _random.Next(0, 100),
				SeriesBeta = _random.Next(50, 100),
				SeriesGamma = _random.Next(0, 1000000000),
				SeriesEpsilon = 1000000000 + _random.Next(0, 10),
				TestItem = new TestItem()
				{
					Amount = _random.Next(0, 100),
				},
			};
			_samples.Add(sample);
		}

		// UI context
		private void AddSampleCallback(object? state)
		{
			Call call = (Call)state!;

			call.Log.Add("test");

			AddSample(_samples.Count);
		}
	}
}
