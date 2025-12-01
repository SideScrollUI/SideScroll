using SideScroll.Charts;
using SideScroll.Time;

namespace SideScroll.Tabs.Samples.Charts;

public class TabSampleChartNoData : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
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
