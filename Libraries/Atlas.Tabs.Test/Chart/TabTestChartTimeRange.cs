using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atlas.Tabs.Test.Chart;

public class TabTestChartTimeRangeValue : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance, ITabAsync
	{
		private readonly Random _random = new();

		public async Task LoadAsync(Call call, TabModel model)
		{
			await Task.Delay(10);

			var list = new List<TimeRangeValue>();
			var chartSettings = new ChartSettings(list, "Active Connection Count");

			DateTime startTime = DateTime.Now;
			for (int i = 0; i < 24; i++)
			{
				var value = new TimeRangeValue()
				{
					StartTime = startTime,
					EndTime = startTime.AddHours(1),
					Value = (_random.Next() % 5),
				};
				list.Add(value);
				startTime = startTime.AddHours(1);
			}
			model.AddObject(chartSettings);
		}
	}
}
