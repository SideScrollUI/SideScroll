using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleEnums : ITab
{
	public TabInstance Create() => new Instance();

	public enum Priority
	{
		Low = 0,
		Normal = 1,
		High = 2,
		Critical = 3,
	}

	[Flags]
	public enum FilePermissions
	{
		None = 0,
		Read = 1,
		Write = 2,
		Execute = 4,
		Delete = 8,
		ReadWrite = Read | Write,
		FullControl = Read | Write | Execute | Delete,
	}

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("Regular Enum - Low", Priority.Low),
				new("Regular Enum - High", Priority.High),
				new("Flags - None", FilePermissions.None),
				new("Flags - Read", FilePermissions.Read),
				new("Flags - ReadWrite", FilePermissions.ReadWrite),
				new("Flags - FullControl", FilePermissions.FullControl),
			};
		}
	}
}
