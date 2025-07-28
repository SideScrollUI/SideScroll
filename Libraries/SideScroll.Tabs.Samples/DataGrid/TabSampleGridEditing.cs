using SideScroll.Logs;

namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleGridEditing : ITab
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
		}
	}
}
