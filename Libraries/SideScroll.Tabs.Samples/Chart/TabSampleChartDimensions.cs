using SideScroll.Attributes;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Tasks;

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
	}

	public class TestItem
	{
		public int Amount { get; set; }
	}

	public class Instance : TabInstance
	{
		private const int MaxValue = 100;

		private readonly ItemCollection<ChartSample> _samples = [];
		private readonly Random _random = new();
		private readonly DateTime _baseDateTime = DateTime.Now.Trim(TimeSpan.FromMinutes(1));

		public override void Load(Call call, TabModel model)
		{
			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Add Entry", AddEntry),
				new TaskDelegate("Start: 1 Entry / second", StartTask, true),
			};
			AddSeries("Cats");
			AddSeries("Dogs");

			var chartView = new ChartView();
			chartView.AddDimensions(_samples,
				nameof(ChartSample.TimeStamp),
				nameof(ChartSample.Value),
				nameof(ChartSample.Animal));
			model.AddObject(chartView, true);
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

		private void StartTask(Call call)
		{
			CancellationToken token = call.TaskInstance!.TokenSource.Token;
			for (int i = 0; i < 20 && !token.IsCancellationRequested; i++)
			{
				Invoke(call, AddSampleUI);
				Thread.Sleep(1000);
			}
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
