using SideScroll;
using SideScroll.Charts;

namespace SideScroll.Tabs.Samples.Chart;

public class TabSampleChartNoData : ITab
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
