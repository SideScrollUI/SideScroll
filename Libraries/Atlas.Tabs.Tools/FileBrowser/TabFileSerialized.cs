using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs.Tools
{
	public class TabFileSerialized : ITab
	{
		public string Path;

		public TabFileSerialized(string path)
		{
			Path = path;
		}

		public TabInstance Create() => new Instance(this);

		public class Instance : TabInstance
		{
			public TabFileSerialized Tab;

			public object Object;
			public Serializer serializer;
			private ListItem listData = new ListItem("Object", null);

			public Instance(TabFileSerialized tab)
			{
				Tab = tab;
			}

			public override void Load(Call call, TabModel model)
			{
				var items = new ItemCollection<ListItem>();

				var serializerFile = new SerializerFileAtlas(Tab.Path);

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
				var serializerFile = new SerializerFileAtlas(Tab.Path);

				Object = serializerFile.Load(call);
				listData.Value = Object;
			}
		}
	}
}
