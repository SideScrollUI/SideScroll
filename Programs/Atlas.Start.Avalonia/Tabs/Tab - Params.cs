using System;
using Atlas.Core;
using Atlas.GUI.Avalonia.Controls;
using Atlas.Tabs;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabParams : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private ItemCollection<ParamTestResult> items = new ItemCollection<ParamTestResult>();
			//private TabControlParams tabParams;
			private ParamTestItem paramTestItem;

			public override void Load(Call call)
			{
				tabModel.Items = items;

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Run", Run),
					new TaskDelegate("10s Task", LongTask, true),
				};

				paramTestItem = this.LoadData<ParamTestItem>("Params");
				//paramTestItem = new ParamTestItem();
				//TabControlParams tabParams = new TabControlParams(this, paramTestItem);
				tabModel.AddObject(paramTestItem);
			}

			private void Run(Call call)
			{
				this.SaveData("Params", paramTestItem);
				ParamTestResult result = new ParamTestResult()
				{
					parameters = paramTestItem,
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
The TabControlParams replaces all of this current logic
	Remove?
	Add more features?
*/
