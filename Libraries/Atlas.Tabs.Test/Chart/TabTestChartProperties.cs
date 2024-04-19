using Atlas.Core;
using Atlas.Core.Charts;
using Atlas.Core.Tasks;

namespace Atlas.Tabs.Test.Chart;

public class TabTestChartProperties : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private readonly ItemCollection<ChartSample> _samples = [];
		private readonly Random _random = new();

		public class TestItem
		{
			public int Amount { get; set; }
		}

		public class ChartSample
		{
			public string? Name { get; set; }

			public int Alpha { get; set; }
			public int Beta { get; set; }
			public int Gamma { get; set; }
			public int Epsilon { get; set; }  // High Value, small delta

			public TestItem TestItem { get; set; } = new();

			public int Amount => TestItem.Amount;
		}

		public override void Load(Call call, TabModel model)
		{
			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Add Entry", AddEntry),
				new TaskDelegate("Start: 1 Entry / second", StartTask, true),
			};

			for (int i = 0; i < 10; i++)
			{
				AddSample(i);
			}

			var chartView = new ChartView();
			chartView.AddSeries("Alpha", _samples, null, nameof(ChartSample.Alpha));
			chartView.AddSeries("Beta", _samples, null, nameof(ChartSample.Beta));
			chartView.AddSeries("Gamma", _samples, null, nameof(ChartSample.Gamma));
			chartView.AddSeries("Epsilon", _samples, null, nameof(ChartSample.Epsilon));
			model.AddObject(chartView);
		}

		private void AddEntry(Call call)
		{
			Invoke(AddSampleCallback, call);
		}

		private void StartTask(Call call)
		{
			CancellationToken token = call.TaskInstance!.TokenSource.Token;
			for (int i = 0; i < 1000 && !token.IsCancellationRequested; i++)
			{
				Invoke(AddSampleCallback, call);
				Thread.Sleep(1000);
			}
		}

		private void AddSample(int i)
		{
			ChartSample sample = new()
			{
				Name = "Name " + i,
				Alpha = _random.Next(0, 100000),
				Beta = _random.Next(0, 100000000),
				Gamma = _random.Next(0, 1000000000),
				Epsilon = 1000000000 + _random.Next(0, 10),
				TestItem = new TestItem
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
