using SideScroll.Logs;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Samples.Params;

public class TabSampleParamsDataGrid : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private readonly LogEntry _logEntry = new()
		{
			Text = "Test Entry",
		};

		public override void Load(Call call, TabModel model)
		{
			// uses DataGrid internally, doesn't work well yet
			model.AddData(_logEntry);
			model.Editing = true;

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Add Log Entry", AddEntry),
			};
		}

		private void AddEntry(Call call)
		{
			call.Log.AddLogEntry(_logEntry);
		}
	}
}
