using Atlas.Core;
using Atlas.Core.Tasks;
using Atlas.Serialize;

namespace Atlas.Tabs.Tools;

public class TabFileSerialized(string path) : ITab
{
	public string Path = path;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabFileSerialized tab) : TabInstance
	{
		public object? Object;
		public Serializer? Serializer;

		private readonly ListItem _listData = new("Object", null);

		public override void Load(Call call, TabModel model)
		{
			var items = new List<ListItem>();

			var serializerFile = new SerializerFileAtlas(System.IO.Path.GetDirectoryName(tab.Path)!);

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
			var serializerFile = new SerializerFileAtlas(tab.Path);

			Object = serializerFile.Load(call);
			_listData.Value = Object;
		}
	}
}
