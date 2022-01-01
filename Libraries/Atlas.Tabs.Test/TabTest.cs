using Atlas.Core;
using Atlas.Tabs.Test.Actions;
using Atlas.Tabs.Test.Chart;
using Atlas.Tabs.Test.DataGrid;
using Atlas.Tabs.Test.DataRepo;
using Atlas.Tabs.Test.Exceptions;
using Atlas.Tabs.Test.Loading;
using Atlas.Tabs.Test.Objects;
using Atlas.Tabs.Tools;
using System.Collections.Generic;

namespace Atlas.Tabs.Test
{
	public class TabTest : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new List<ListItem>()
				{
					new("Sample", new TabSample()),

					new("Objects", new TabTestObjects()),
					new("Data Grid", new TabTestDataGrid()),
					new("Params", new TabTestParams()),
					new("Log", new TabTestLog()),
					new("Actions", new TabActions()),
					new("Json", new TabTestJson()),
					new("Bookmarks", new TabTestBookmarks()),
					new("Skip", new TabTestSkip()), // move into new Lists?
					new("Exceptions", new TabTestExceptions()),

					new("Web Browser", new TabTestBrowser()),
					new("Text Editor", new TabTestTextEditor()),
					new("Chart", new TabTestChart()),
					new("Toolbar", new TabTestToolbar()),
					new("Serializer", new TabSerializer()),
					new("Process", new TabTestProcess()),
					new("Loading", new TabTestLoading()),
					new("Data Repos", new TabTestDataRepo()),
					new("Icons", new TabIcons()),
					new("Tools", new TabTools()),
				};
			}
		}
	}
}
