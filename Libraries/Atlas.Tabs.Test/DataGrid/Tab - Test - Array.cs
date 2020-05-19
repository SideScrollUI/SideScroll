using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestArray : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public Instance()
			{
			}

			public override void Load(Call call, TabModel model)
			{
				var classes = new MyClass[] { new MyClass(), new MyClass() };

				model.Items = new ItemCollection<ListItem>()
				{
					new ListItem("2 Items", classes),
					new ListItem("String Array", new string[] { "abc", "123" }),
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
}
