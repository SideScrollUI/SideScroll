using Atlas.Core;
using System.Collections.Generic;

namespace Atlas.Tabs.Test.Actions
{
	public class TabParamsDataGrid : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private LogEntry _logEntry = new LogEntry()
			{
				Text = "Test Entry",
			};

			public override void Load(Call call, TabModel model)
			{
				// uses DataGrid internally, doesn't work well yet
				model.AddData(_logEntry);
				model.Editing = true;

				model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Add Log Entry", AddEntry),
				};

				model.Notes = "You can specify parameters for an action.\n\nSpecify the values for a new Log Entry and click the Add button to add it";
			}

			private void AddEntry(Call call)
			{
				call.Log.AddLogEntry(_logEntry);
			}

		}
	}
}
