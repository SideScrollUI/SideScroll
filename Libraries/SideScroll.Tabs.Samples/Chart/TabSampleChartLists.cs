using SideScroll.Attributes;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Chart;

public class TabSampleChartLists : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonAdd { get; set; } = new("Add", Icons.Svg.Add);

		[Separator]
		public ToolButton ButtonStart { get; set; } = new("Start", Icons.Svg.Play);
		public ToolButton ButtonStop { get; set; } = new("Stop", Icons.Svg.Stop);
	}

	public class Instance : TabInstance
	{
		private List<ItemCollection<int>> _series = [];
		private readonly Random _random = new();

		public override void Load(Call call, TabModel model)
		{
			_series = [];

			Toolbar toolbar = new();
			toolbar.ButtonAdd.Action = AddEntry;
			toolbar.ButtonStart.ActionAsync = StartTaskAsync;
			toolbar.ButtonStop.Action = StopTask;
			model.AddObject(toolbar);

			ChartView chartView = new();
			for (int i = 0; i < 2; i++)
			{
				ItemCollection<int> list = [];
				chartView.AddSeries($"Series {i}", list);
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

		private Call? _addCall;
		private async Task StartTaskAsync(Call call)
		{
			_addCall = call;

			CancellationToken token = call.TaskInstance!.TokenSource.Token;
			for (int i = 0; i < 1000 && !token.IsCancellationRequested; i++)
			{
				Invoke(AddSampleUI, call);
				await Task.Delay(1000);
			}
		}

		private void StopTask(Call call)
		{
			_addCall?.TaskInstance?.Cancel();
			_addCall = null;
		}

		private void AddSample()
		{
			int multiplier = 1;
			foreach (var list in _series)
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
