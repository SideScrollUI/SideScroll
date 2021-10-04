using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Atlas.Tabs.Test.Chart
{
	public class TabTestChartList : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private List<ItemCollection<int>> _series;
			private Random _random = new Random();

			public override void Load(Call call, TabModel model)
			{
				_series = new List<ItemCollection<int>>();
				//model.Items = items;

				model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Add Entry", AddEntry),
					new TaskDelegate("Start: 1 Entry / second", StartTask, true),
				};

				var chartSettings = new ChartSettings();
				for (int i = 0; i < 2; i++)
				{
					var list = new ItemCollection<int>();
					chartSettings.AddList("Series " + i, list);
					_series.Add(list);
				}

				for (int i = 0; i < 10; i++)
				{
					AddSample();
				}
				model.AddObject(chartSettings);
			}

			private void AddEntry(Call call)
			{
				Invoke(call, AddSampleUI);
			}

			private void StartTask(Call call)
			{
				CancellationToken token = call.TaskInstance.TokenSource.Token;
				for (int i = 0; !token.IsCancellationRequested; i++)
				{
					Invoke(AddSampleUI, call);
					Thread.Sleep(1000);
				}
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
}
