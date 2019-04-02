using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Core;
using Atlas.Serialize;

namespace Atlas.Tabs.Test
{
	public class TabSerializer : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private ItemCollection<ListItem> items = new ItemCollection<ListItem>();

			public override void Load(Call call)
			{
				tabModel.Items = items;

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Serialize 1 object", Serialize, true),
					new TaskDelegate("Deserialize 1 object", Deserialize, true),
					new TaskDelegate("Serialize 1 million objects", SerializeOneMillionObjects, true),
					new TaskDelegate("Deserialize 1 million objects", DeserializeOneMillionObjects, true),
				};

				tabModel.Notes = "";
			}

			// GUI thread
			private void AddListItem(ListItem listItem)
			{
				items.Add(listItem);
			}

			// Task threads
			private void Serialize(Call call)
			{
				SampleItem sampleItem = new SampleItem(1, "Sample Item");
				
				project.DataApp.Save(sampleItem, call);
				call.taskInstance.OnComplete = () => AddListItem(new ListItem("Sample Item", sampleItem));
			}

			private void Deserialize(Call call)
			{
				SampleItem sampleItem = project.DataApp.Load<SampleItem>(false, false, call);
				call.taskInstance.OnComplete = () => AddListItem(new ListItem("Deserialized Sample Item", sampleItem));
			}

			private void SerializeOneMillionObjects(Call call)
			{
				List<SampleItem> sampleItems = new List<SampleItem>();
				for (int i = 0; i < 1000000; i++)
				{
					sampleItems.Add(new SampleItem(i, "Item " + i.ToString()));
				}
				project.DataApp.Save(sampleItems, call);
				call.taskInstance.OnComplete = () => AddListItem(new ListItem("SerializeOneMillionObjects", sampleItems));
			}

			private void DeserializeOneMillionObjects(Call call)
			{
				List<SampleItem> sampleItems = project.DataApp.Load<List<SampleItem>>(false, false, call);
				call.taskInstance.OnComplete = () => AddListItem(new ListItem("DeserializeOneMillionObjects", sampleItems));
			}
		}

		public class SampleItem
		{
			public int ID { get; set; }
			public string Name { get; set; }

			private SampleItem()
			{

			}

			public SampleItem(int id, string name)
			{
				this.ID = id;
				this.Name = name;
			}

			public override string ToString()
			{
				return Name;
			}
		}
	}
}
/*
*/
