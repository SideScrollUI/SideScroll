using System;
using System.ComponentModel;
using Atlas.Core;
using Atlas.Tabs;

namespace Atlas.Tabs.Test
{
	public class TabTestParams : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private const string dataKey = "Params";
			private ItemCollection<ParamTestResult> items = new ItemCollection<ParamTestResult>();
			private ParamTestItem paramTestItem;

			public override void Load()
			{
				tabModel.Items = items;

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add", Add),
					new TaskDelegate("10s Task", LongTask, true),
				};

				paramTestItem = this.LoadData<ParamTestItem>(dataKey);
				tabModel.AddObject(paramTestItem);

				tabModel.Notes = "Adding a class of type [Params] to a tabModel creates a TabControlParam\nParameter values can be saved between Tasks";
			}

			private void Add(Call call)
			{
				this.SaveData(dataKey, paramTestItem);
				var clone = Serialize.SerializerMemory.Clone<ParamTestItem>(call, paramTestItem);
				ParamTestResult result = new ParamTestResult()
				{
					parameters = clone,
				};
				items.Add(result);
			}

			private void LongTask(Call call)
			{
				call.taskInstance.ProgressMax = 10;
				for (int i = 0; i < 10; i++)
				{
					System.Threading.Thread.Sleep(1000);
					call.log.Add("Slept 1 second");
					call.taskInstance.Progress++;
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
			public AttributeTargets AttributeTargets { get; set; } = AttributeTargets.Event;
		}

		public class ParamTestResult
		{
			public ParamTestItem parameters;
			public string String { get { return parameters.String; } }
		}
	}
}
/*
*/
