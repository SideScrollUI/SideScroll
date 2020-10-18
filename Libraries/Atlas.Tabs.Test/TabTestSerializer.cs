using Atlas.Core;
using System.Collections.Generic;

namespace Atlas.Tabs.Test
{
	public class TabSerializer : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private ItemCollectionUI<ListItem> items;

			public override void Load(Call call, TabModel model)
			{
				items = new ItemCollectionUI<ListItem>();
				model.Items = items;

				model.Actions = new List<TaskCreator>()
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
				items.Add(new ListItem("Sample Item", sampleItem));
			}

			private void Deserialize(Call call)
			{
				SampleItem sampleItem = Project.DataApp.Load<SampleItem>(false, false, call);
				items.Add(new ListItem("Deserialized Sample Item", sampleItem));
			}

			private void SerializeOneMillionObjects(Call call)
			{
				var sampleItems = new List<SampleItem>();
				for (int i = 0; i < 1000000; i++)
				{
					sampleItems.Add(new SampleItem(i, "Item " + i.ToString()));
				}
				Project.DataApp.Save(sampleItems, call);
				items.Add(new ListItem("SerializeOneMillionObjects", sampleItems));
			}

			private void DeserializeOneMillionObjects(Call call)
			{
				List<SampleItem> sampleItems = Project.DataApp.Load<List<SampleItem>>(false, false, call);
				items.Add(new ListItem("DeserializeOneMillionObjects", sampleItems));
			}
		}

		public class SampleItem
		{
			public int ID { get; set; }
			public string Name { get; set; }

			public SampleItem()
			{
			}

			public SampleItem(int id, string name)
			{
				ID = id;
				Name = name;
			}

			public override string ToString()
			{
				return Name;
			}
		}
	}
}
