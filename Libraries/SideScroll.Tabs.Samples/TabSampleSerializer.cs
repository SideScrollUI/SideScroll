using SideScroll.Collections;
using SideScroll.Tabs.Lists;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Samples;

public class TabSampleSerializer : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private ItemCollectionUI<ListItem> _items = [];

		public override void Load(Call call, TabModel model)
		{
			model.Items = _items = [];

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Serialize 1 object", Serialize, true, true),
				new TaskDelegate("Deserialize 1 object", Deserialize, true, true),
				new TaskDelegate("Serialize 1 million objects", SerializeOneMillionObjects, true, true),
				new TaskDelegate("Deserialize 1 million objects", DeserializeOneMillionObjects, true, true),
			};
		}

		private void Serialize(Call call)
		{
			var sampleItem = new SampleItem(1, "Sample Item");

			Project.Data.App.Save(sampleItem, call);
			_items.Add(new ListItem("Sample Item", sampleItem));
		}

		private void Deserialize(Call call)
		{
			SampleItem sampleItem = Project.Data.App.Load<SampleItem>(false, call)!;
			_items.Add(new ListItem("Deserialized Sample Item", sampleItem));
		}

		private void SerializeOneMillionObjects(Call call)
		{
			var sampleItems = new List<SampleItem>(1_000_000);
			for (int i = 0; i < 1_000_000; i++)
			{
				sampleItems.Add(new SampleItem(i, "Item " + i));
			}
			Project.Data.App.Save(sampleItems, call);
			_items.Add(new ListItem("SerializeOneMillionObjects", sampleItems));
		}

		private void DeserializeOneMillionObjects(Call call)
		{
			List<SampleItem> sampleItems = Project.Data.App.Load<List<SampleItem>>(false, call)!;
			_items.Add(new ListItem("DeserializeOneMillionObjects", sampleItems));
		}
	}

	public class SampleItem
	{
		public int Id { get; set; }
		public string? Name { get; set; }

		public override string? ToString() => Name;

		public SampleItem() { }

		public SampleItem(int id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}
