using System;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestArray : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public Instance()
			{
			}

			public override void Load(Call call)
			{
				MyClass[] classes = new MyClass[] { new MyClass(), new MyClass() };

				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("2 Items", classes),
					new ListItem("String Array", new string[] { "abc", "123" }),
				};

				/*tabModel.Actions = new ItemCollection<TaskCreator>() {
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
