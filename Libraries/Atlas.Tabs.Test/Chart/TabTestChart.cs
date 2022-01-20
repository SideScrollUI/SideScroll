using Atlas.Core;
using System.Collections.Generic;

namespace Atlas.Tabs.Test.Chart;

public class TabTestChart : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>()
			{
				new("List", new TabTestChartList()),
				//new("Split", new TabTestChartSplit()),
				new("Overlay", new TabTestChartOverlay()),
				new("Time Range", new TabTestChartTimeRangeValue()),
			};
		}
	}
}
