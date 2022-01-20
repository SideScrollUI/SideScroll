using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs.Tools;

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
		public Serializer Serializer;

		private readonly ListItem _listData = new("Object", null);

		public Instance(TabFileSerialized tab)
		{
			Tab = tab;
		}

		public override void Load(Call call, TabModel model)
		{
			var items = new List<ListItem>();

			var serializerFile = new SerializerFileAtlas(System.IO.Path.GetDirectoryName(Tab.Path));

			Serializer = serializerFile.LoadSchema(call);

			items.Add(new ListItem("Schema", Serializer));
			items.Add(_listData);
			model.Items = items;

			var actions = new List<TaskCreator>();
			if (Object == null)
				actions.Add(new TaskDelegate("Load Data", LoadData));
			model.Actions = actions;
		}

		private void LoadData(Call call)
		{
			var serializerFile = new SerializerFileAtlas(Tab.Path);

			Object = serializerFile.Load(call);
			_listData.Value = Object;
		}
	}
}
