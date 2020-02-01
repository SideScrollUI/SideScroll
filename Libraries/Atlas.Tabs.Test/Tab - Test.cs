using System;
using System.Collections.Generic;
using Atlas.Core;
using Atlas.Tabs.Test.Actions;
using Atlas.Tabs.Test.Chart;
using Atlas.Tabs.Test.DataGrid;
using Atlas.Tabs.Test.Objects;

namespace Atlas.Tabs.Test
{
	public class TabTest : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public Instance()
			{
			}

			public Instance(Project project)
			{
				this.project = project;
				LoadDefaultBookmark();

				tabModel.Name = "Start";

				tabModel.Bookmarks = new BookmarkCollection(project);
			}

			public override void Load(Call call)
			{
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Sample", new TabSample()),

					new ListItem("Objects", new TabTestObjects()),
					new ListItem("Data Grid", new TabTestDataGrid()),
					new ListItem("Params", new TabTestParams()),
					new ListItem("Log", new TabTestLog()),
					new ListItem("Actions", new TabActions()),
					new ListItem("Json", new TabTestJson()),
					new ListItem("Bookmarks", new TabTestBookmarks()),
					new ListItem("Skip", new TabTestSkip()), // move into new Lists?
					new ListItem("Exceptions", new TabTestExceptions()), // move into new Lists?

					new ListItem("Web Browser", new TabTestBrowser()),
					new ListItem("Text Editor", new TabTestTextEditor()),
					new ListItem("Chart", new TabTestChart()),
					new ListItem("Serializer", new TabSerializer()),
					new ListItem("Process", new TabTestProcess()),
					new ListItem("Loading", new TabTestLoading()),
					new ListItem("Data Repos", new TabTestDataRepo()),
				};
			}
		}
	}
}
