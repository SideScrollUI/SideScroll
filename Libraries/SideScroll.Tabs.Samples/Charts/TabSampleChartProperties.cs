using SideScroll.Attributes;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Charts;

public class TabSampleChartProperties : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonAdd { get; } = new("Add", Icons.Svg.Add);

		[Separator]
		public ToolButton ButtonStart { get; } = new("Start", Icons.Svg.Play, backgroundThread: true);
		public ToolButton ButtonStop { get; } = new("Stop", Icons.Svg.Stop);
	}

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
			Toolbar toolbar = new();
			toolbar.ButtonAdd.Action = AddEntry;
			toolbar.ButtonStart.ActionAsync = StartTaskAsync;
			toolbar.ButtonStop.Action = StopTask;
			model.AddObject(toolbar);

			for (int i = 0; i < 10; i++)
			{
				AddSample(i);
			}

			ChartView chartView = new();
			chartView.AddSeries("Alpha", _samples, null, nameof(ChartSample.Alpha));
			chartView.AddSeries("Beta", _samples, null, nameof(ChartSample.Beta));
			chartView.AddSeries("Gamma", _samples, null, nameof(ChartSample.Gamma));
			chartView.AddSeries("Epsilon", _samples, null, nameof(ChartSample.Epsilon));
			model.AddObject(chartView);
		}

		private void AddEntry(Call call)
		{
			Post(AddSampleCallback, call);
		}

		private Call? _addCall;
		private async Task StartTaskAsync(Call call)
		{
			_addCall = call;

			CancellationToken cancelToken = call.TaskInstance!.CancelToken;
			for (int i = 0; i < 1000 && !cancelToken.IsCancellationRequested; i++)
			{
				Post(AddSampleCallback, call);
				await Task.Delay(1000, cancelToken);
			}
		}

		private void StopTask(Call call)
		{
			_addCall?.TaskInstance?.Cancel();
			_addCall = null;
		}

		private void AddSample(int i)
		{
			ChartSample sample = new()
			{
				Name = "Name " + i,
				Alpha = _random.Next(0, 100_000),
				Beta = _random.Next(0, 100_000_000),
				Gamma = _random.Next(0, 1_000_000_000),
				Epsilon = 1_000_000_000 + _random.Next(0, 10),
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
