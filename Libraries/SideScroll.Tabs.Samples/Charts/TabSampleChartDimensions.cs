using SideScroll.Attributes;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using SideScroll.Time;

namespace SideScroll.Tabs.Samples.Charts;

public class TabSampleChartDimensions : ITab
{
	public TabInstance Create() => new Instance();

	private class ChartSample
	{
		public string? Animal { get; set; }

		[XAxis]
		public DateTime Timestamp { get; set; }

		public int? Value { get; set; }

		public TestItem TestItem { get; set; } = new();

		public int Amount => TestItem.Amount;

		public override string ToString() => $"{Animal}: {Value}";
	}

	private class TestItem
	{
		public int Amount { get; set; }
	}

	private class Toolbar : TabToolbar
	{
		public ToolButton ButtonAdd { get; } = new("Add", Icons.Svg.Add);

		[Separator]
		public ToolButton ButtonStart { get; } = new("Start", Icons.Svg.Play, backgroundThread: true);
		public ToolButton ButtonStop { get; } = new("Stop", Icons.Svg.Stop);
	}

	private class Instance : TabInstance, ITabSelector
	{
		private const int MaxValue = 100;
		private const int SampleCount = 10;

		private readonly Random _random = new();

		private readonly DateTime _baseDateTime = TimeZoneView.Now
			.Trim(TimeSpan.TicksPerMinute)
			.AddMinutes(-SampleCount);

		private ItemCollection<ChartSample> _samples = [];

		public new event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;

		public override void Load(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonAdd.Action = AddEntry;
			toolbar.ButtonStart.ActionAsync = StartTaskAsync;
			toolbar.ButtonStop.Action = StopTask;
			model.AddObject(toolbar);

			_samples = [];
			AddSeries("Cats");
			AddSeries("Dogs");

			ChartView chartView = new();
			chartView.AddDimensions(_samples,
				nameof(ChartSample.Timestamp),
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
			for (int i = 0; i < SampleCount; i++)
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
			Post(call, AddSampleUI, param1, param2);
		}

		private Call? _addCall;
		private async Task StartTaskAsync(Call call)
		{
			_addCall = call;

			CancellationToken cancelToken = call.TaskInstance!.CancelToken;
			for (int i = 0; i < 20 && !cancelToken.IsCancellationRequested; i++)
			{
				Post(call, AddSampleUI);
				await Task.Delay(1000, cancelToken);
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
				Timestamp = _baseDateTime.AddMinutes(i),
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
				Timestamp = _baseDateTime.AddMinutes(i),
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
