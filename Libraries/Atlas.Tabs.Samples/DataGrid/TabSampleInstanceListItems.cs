using Atlas.Core;

namespace Atlas.Tabs.Samples.DataGrid;

public class TabSampleInstanceListItems : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public bool BooleanField = true; // Ignored for now, GetListItems() doesn't support fields
		public bool BooleanProperty { get; set; } = true;

		public string StringProperty => "StringProperty";

		[Item]
		public string Method() => "Result";

		[Item]
		public async Task<string> AsyncMethodAsync() => await MethodAsync();

		[Item]
		public Task<string> TaskMethodAsync() => MethodAsync();

		public override void Load(Call call, TabModel model)
		{
			model.Items = GetListItems();
		}

		private static async Task<string> MethodAsync()
		{
			await Task.Delay(10);
			return "Result";
		}
	}
}
