using Atlas.Core;
using Atlas.Core.Charts;

namespace Atlas.Tabs.Test.Chart;

public class TabTestChartNoData : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			var chartView = new ChartView("Animals");
			chartView.AddSeries("Cats", new List<TimeRangeValue>());
			chartView.AddSeries("Dogs", new List<TimeRangeValue>());

			model.AddObject(chartView);
		}
	}
}
