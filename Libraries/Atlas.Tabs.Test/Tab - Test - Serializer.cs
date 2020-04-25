using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabSerializer : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private ThreadedItemCollection<ListItem> items;

			public override void Load(Call call, TabModel model)
			{
				items = new ThreadedItemCollection<ListItem>();
				model.Items = items;

				model.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Serialize 1 object", Serialize, true, true),
					new TaskDelegate("Deserialize 1 object", Deserialize, true, true),
					new TaskDelegate("Serialize 1 million objects", SerializeOneMillionObjects, true, true),
					new TaskDelegate("Deserialize 1 million objects", DeserializeOneMillionObjects, true, true),
				};
			}

			private void Serialize(Call call)
			{
				SampleItem sampleItem = new SampleItem(1, "Sample Item");
				
				project.DataApp.Save(sampleItem, call);
				items.Add(new ListItem("Sample Item", sampleItem));
			}

			private void Deserialize(Call call)
			{
				SampleItem sampleItem = project.DataApp.Load<SampleItem>(false, false, call);
				items.Add(new ListItem("Deserialized Sample Item", sampleItem));
			}

			private void SerializeOneMillionObjects(Call call)
			{
				var sampleItems = new List<SampleItem>();
				for (int i = 0; i < 1000000; i++)
				{
					sampleItems.Add(new SampleItem(i, "Item " + i.ToString()));
				}
				project.DataApp.Save(sampleItems, call);
				items.Add(new ListItem("SerializeOneMillionObjects", sampleItems));
			}

			private void DeserializeOneMillionObjects(Call call)
			{
				List<SampleItem> sampleItems = project.DataApp.Load<List<SampleItem>>(false, false, call);
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
