using Atlas.Core;
using Atlas.Serialize;
using Atlas.Tabs;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs.Tools
{
	public class TabFileSerialized : ITab
	{
		public string path;

		public TabFileSerialized(string path)
		{
			this.path = path;
		}

		public TabInstance Create() => new Instance(this);

		public class Instance : TabInstance
		{
			private TabFileSerialized tab;

			public object Object;
			public Serializer serializer;
			private ListItem listData = new ListItem("Object", null);

			public Instance(TabFileSerialized tab)
			{
				this.tab = tab;
			}

			public override void Load(Call call, TabModel model)
			{
				var items = new ItemCollection<ListItem>();

				var serializerFile = new SerializerFile(tab.path);

				serializer = serializerFile.LoadSchema(call);

				items.Add(new ListItem("Schema", serializer));
				items.Add(listData);
				model.Items = items;

				var actions = new ItemCollection<TaskCreator>();
				if (Object == null)
					actions.Add(new TaskDelegate("Load Data", LoadData));
				model.Actions = actions;
			}

			private void LoadData(Call call)
			{
				var serializerFile = new SerializerFile(tab.path);

				Object = serializerFile.Load(call);
				listData.Value = Object;
			}
		}
	}
}
