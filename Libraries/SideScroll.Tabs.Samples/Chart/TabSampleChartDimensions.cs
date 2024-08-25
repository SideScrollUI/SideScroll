using SideScroll.Attributes;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Chart;

public class TabSampleChartDimensions : ITab
{
	public TabInstance Create() => new Instance();

	public class ChartSample
	{
		public string? Animal { get; set; }

		[XAxis]
		public DateTime TimeStamp { get; set; }

		public int? Value { get; set; }

		public TestItem TestItem { get; set; } = new();

		public int Amount => TestItem.Amount;

		public override string ToString() => $"{Animal}: {Value}";
	}

	public class TestItem
	{
		public int Amount { get; set; }
	}

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonAdd { get; set; } = new("Add", Icons.Svg.Add);

		[Separator]
		public ToolButton ButtonStart { get; set; } = new("Start", Icons.Svg.Play);
		public ToolButton ButtonStop { get; set; } = new("Stop", Icons.Svg.Stop);
	}

	public class Instance : TabInstance, ITabSelector
	{
		private const int MaxValue = 100;

		private readonly Random _random = new();
		private readonly DateTime _baseDateTime = DateTime.Now.Trim(TimeSpan.FromMinutes(1));

		private ItemCollection<ChartSample> _samples = [];

		public new event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;

		public override void Load(Call call, TabModel model)
		{
			model.ReloadOnThemeChange = true;

			Toolbar toolbar = new();
			toolbar.ButtonAdd.Action = AddEntry;
			toolbar.ButtonStart.ActionAsync = StartTaskAsync;
			toolbar.ButtonStop.Action = StopTask;
			model.AddObject(toolbar);

			_samples = [];
			AddSeries("Cats");
			AddSeries("Dogs");

			var chartView = new ChartView();
			chartView.AddDimensions(_samples,
				nameof(ChartSample.TimeStamp),
				nameof(ChartSample.Value),
				nameof(ChartSample.Animal));
			chartView.SelectionChanged += ChartView_SelectionChanged;
			model.AddObject(chartView, true);
		}

		private void ChartView_SelectionChanged(object? sender, SeriesSelectedEventArgs e)
		{
			SelectedItems = e.Series
				.Select(s => new ListItem(s.Name!, s.TimeRangeValues))
				.ToList();
			OnSelectionChanged?.Invoke(sender, new TabSelectionChangedEventArgs());
		}

		private void AddSeries(string dimension)
		{
			for (int i = 0; i < 10; i++)
			{
				if (i is 4 or 6)
				{
					AddNullSample(dimension, i);
				}
				else
				{
					AddSample(dimension, i);
				}
			}
		}

		private void AddEntry(Call call)
		{
			int param1 = 1;
			string param2 = "abc";
			Invoke(call, AddSampleUI, param1, param2);
		}

		private Call? _addCall;
		private async Task StartTaskAsync(Call call)
		{
			_addCall = call;

			CancellationToken token = call.TaskInstance!.TokenSource.Token;
			for (int i = 0; i < 20 && !token.IsCancellationRequested; i++)
			{
				Invoke(call, AddSampleUI);
				await Task.Delay(1000);
			}
		}

		private void StopTask(Call call)
		{
			_addCall?.TaskInstance?.Cancel();
			_addCall = null;
		}

		private void AddSample(string animal, int i)
		{
			ChartSample sample = new()
			{
				Animal = animal,
				TimeStamp = _baseDateTime.AddMinutes(i),
				Value = _random.Next(50, MaxValue),
				TestItem = new TestItem
				{
					Amount = _random.Next(0, MaxValue),
				},
			};
			_samples.Add(sample);
		}

		private void AddNullSample(string animal, int i)
		{
			ChartSample sample = new()
			{
				Animal = animal,
				TimeStamp = _baseDateTime.AddMinutes(i),
				TestItem = new TestItem
				{
					Amount = _random.Next(0, MaxValue),
				},
			};
			_samples.Add(sample);
		}

		// UI context
		private void AddSampleUI(Call call, object state)
		{
			AddSample("Cats", _samples.Count);
			AddSample("Dogs", _samples.Count);
		}
	}
}
