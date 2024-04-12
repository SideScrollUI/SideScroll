using Atlas.Core;
using Atlas.Core.Charts;

namespace Atlas.Tabs.Test.Chart;

public class TabTestChartLists : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private List<ItemCollection<int>>? _series;
		private readonly Random _random = new();

		public override void Load(Call call, TabModel model)
		{
			_series = new List<ItemCollection<int>>();

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Add Entry", AddEntry),
				new TaskDelegate("Start: 1 Entry / second", StartTask, true),
			};

			var chartView = new ChartView();
			for (int i = 0; i < 2; i++)
			{
				var list = new ItemCollection<int>();
				chartView.AddSeries("Series " + i, list);
				_series.Add(list);
			}

			for (int i = 0; i < 10; i++)
			{
				AddSample();
			}
			model.AddObject(chartView);
		}

		private void AddEntry(Call call)
		{
			Invoke(call, AddSampleUI);
		}

		private void StartTask(Call call)
		{
			CancellationToken token = call.TaskInstance!.TokenSource.Token;
			for (int i = 0; i < 1000 && !token.IsCancellationRequested; i++)
			{
				Invoke(AddSampleUI, call);
				Thread.Sleep(1000);
			}
		}

		private void AddSample()
		{
			int multiplier = 1;
			foreach (var list in _series!)
			{
				int amount = (_random.Next() % 1000) * multiplier;
				list.Add(amount);
				multiplier++;
			}
		}

		// UI context
		private void AddSampleUI(Call call, object state)
		{
			AddSample();
		}
	}
}
