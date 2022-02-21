using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid;

public class TabTestArray : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			var classes = new MyClass[]
			{
				new(), 
				new(),
			};

			model.Items = new ItemCollection<ListItem>()
			{
				new("2 Items", classes),
				new("String Array", new string[] { "abc", "123" }),
			};

			/*tabModel.Actions = new List<TaskCreator>() {
			//new TaskAction("Add Entries", AddEntries),
			};*/
		}
	}


	public class MyClass
	{
		public string Name { get; set; } = "Eve";
	}
}
