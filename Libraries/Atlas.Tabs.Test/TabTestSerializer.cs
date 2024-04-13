using Atlas.Core;

namespace Atlas.Tabs.Test;

public class TabSerializer : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private ItemCollectionUI<ListItem>? _items;

		public override void Load(Call call, TabModel model)
		{
			_items = new ItemCollectionUI<ListItem>();
			model.Items = _items;

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

			Project.DataApp.Save(sampleItem, call);
			_items!.Add(new ListItem("Sample Item", sampleItem));
		}

		private void Deserialize(Call call)
		{
			SampleItem sampleItem = Project.DataApp.Load<SampleItem>(false, false, call)!;
			_items!.Add(new ListItem("Deserialized Sample Item", sampleItem));
		}

		private void SerializeOneMillionObjects(Call call)
		{
			var sampleItems = new List<SampleItem>();
			for (int i = 0; i < 1000000; i++)
			{
				sampleItems.Add(new SampleItem(i, "Item " + i));
			}
			Project.DataApp.Save(sampleItems, call);
			_items!.Add(new ListItem("SerializeOneMillionObjects", sampleItems));
		}

		private void DeserializeOneMillionObjects(Call call)
		{
			List<SampleItem> sampleItems = Project.DataApp.Load<List<SampleItem>>(false, false, call)!;
			_items!.Add(new ListItem("DeserializeOneMillionObjects", sampleItems));
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
