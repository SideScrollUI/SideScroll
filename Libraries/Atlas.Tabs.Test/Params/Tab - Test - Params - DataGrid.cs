using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Core;
using Atlas.Tabs;

namespace Atlas.Tabs.Test.Actions
{
	public class TabParamsDataGrid : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			//private ItemCollection<ListItem> items = new ItemCollection<ListItem>();
			private LogEntry logEntry = new LogEntry()
			{
				Text = "Test Entry",
			};

			public override void Load(Call call)
			{
				// uses DataGrid internally, doesn't work well yet
				tabModel.AddData(logEntry);
				tabModel.Editing = true;
				//tabModel.AddInput(logEntry);

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add Log Entry", AddEntry),
				};

				tabModel.Notes = "You can specify parameters for an action.\n\nSpecify the values for a new Log Entry and click the Add button to add it";
			}

			private void AddEntry(Call call)
			{
				//LogEntry logEntry = (LogEntry)call.Params;
				call.log.AddLogEntry(logEntry);
			}

		}
	}
}
/*
*/
