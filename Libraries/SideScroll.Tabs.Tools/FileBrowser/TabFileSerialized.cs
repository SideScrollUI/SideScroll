using SideScroll;
using SideScroll.Tasks;
using SideScroll.Serialize;

namespace SideScroll.Tabs.Tools;

public class TabFileSerialized(string path) : ITab
{
	public string Path = path;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabFileSerialized tab) : TabInstance
	{
		private SerializerFileSideScroll? _serializerFile;
		private ItemCollectionUI<ListItem> _items = [];

		public override void Load(Call call, TabModel model)
		{
			_serializerFile = new SerializerFileSideScroll(System.IO.Path.GetDirectoryName(tab.Path)!);

			var serializer = _serializerFile.LoadSchema(call);

			_items = [];
			_items.Add(new ListItem("Schema", serializer.TypeSchemas));
			model.Items = _items;

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Load Data", LoadData)
			};
		}

		private void LoadData(Call call)
		{
			var obj = _serializerFile!.Load(call);
			_items.Add(new ListItem("Loaded", obj));
		}
	}
}
