using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Atlas.Tabs.Test
{
	public class TabTestParams : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private const string dataKey = "Params";
			private ItemCollection<ParamTestResult> items = new ItemCollection<ParamTestResult>();
			private ParamTestItem paramTestItem;

			public override void Load(Call call, TabModel model)
			{
				model.Items = items;

				model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Add", Add),
					new TaskDelegate("10s Task", LongTask, true),
				};

				paramTestItem = this.LoadData<ParamTestItem>(dataKey);
				if (paramTestItem.DateTime.Ticks == 0)
					paramTestItem.DateTime = DateTime.Now; // in case the serializer loses it
				model.AddObject(paramTestItem);

				model.Notes = "Adding a class of type [Params] to a tabModel creates a TabControlParam\nParameter values can be saved between Tasks";
			}

			private void Add(Call call)
			{
				SaveData(dataKey, paramTestItem);
				var clone = paramTestItem.Clone<ParamTestItem>(call);
				ParamTestResult result = new ParamTestResult()
				{
					parameters = clone,
				};
				items.Add(result);
			}

			private void LongTask(Call call)
			{
				call.TaskInstance.ProgressMax = 10;
				for (int i = 0; i < 10; i++)
				{
					System.Threading.Thread.Sleep(1000);
					call.Log.Add("Slept 1 second");
					call.TaskInstance.Progress++;
				}
			}
		}

		[Params]
		public class ParamTestItem
		{
			public bool Boolean { get; set; } = true;
			public string String { get; set; } = "Test";
			[ReadOnly(true)]
			public string ReadOnly { get; set; } = "ReadOnly";
			public int Integer { get; set; } = 123;
			public double Double { get; set; } = 3.14;
			public DateTime DateTime { get; set; } = DateTime.Now;
			public AttributeTargets EnumAttributeTargets { get; set; } = AttributeTargets.Event;
			public static List<ParamListItem> ListItems => new List<ParamListItem>()
			{
				new ParamListItem("One", 1),
				new ParamListItem("Two", 2),
				new ParamListItem("Three", 3),
			};
			[BindList(nameof(ListItems))]
			public ParamListItem ListItem { get; set; }

			public ParamTestItem()
			{
				ListItem = ListItems[1];
			}
		}

		public class ParamListItem
		{
			public string Name { get; set; }
			public int Value { get; set; }

			public override string ToString() => Name;

			public ParamListItem()
			{
			}

			public ParamListItem(string name, int value)
			{
				Name = name;
				Value = value;
			}
		}

		public class ParamTestResult
		{
			public ParamTestItem parameters;
			public string String => parameters.String;
		}
	}
}
