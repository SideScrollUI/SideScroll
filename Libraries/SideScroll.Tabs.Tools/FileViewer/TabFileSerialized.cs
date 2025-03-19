using SideScroll.Collections;
using SideScroll.Serialize.Atlas;
using SideScroll.Tabs.Lists;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Tools.FileViewer;

public class TabFileSerialized(string path) : ITab
{
	public string Path => path;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabFileSerialized tab) : TabInstance
	{
		private SerializerFileAtlas? _serializerFile;
		private ItemCollectionUI<ListItem> _items = [];

		public override void Load(Call call, TabModel model)
		{
			_serializerFile = new SerializerFileAtlas(System.IO.Path.GetDirectoryName(tab.Path)!);

			model.Items = _items = [];
			try
			{
				var serializer = _serializerFile.LoadSchema(call);

				_items.Add(new ListItem("Schema", serializer.TypeSchemas));

				model.Actions = new List<TaskCreator>
				{
					new TaskDelegate("Load Data", LoadData, true, true)
				};
			}
			catch (Exception ex)
			{
				call.Log.Add(ex);
			}

			_items.Add(new ListItem("Bytes", ListByte.Load(_serializerFile.DataPath!)));
		}

		private void LoadData(Call call)
		{
			var obj = _serializerFile!.Load(call, logLevel: Logs.LogLevel.Info);
			_items.Add(new ListItem("Loaded", obj));
		}
	}
}
