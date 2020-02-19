using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Runtime.CompilerServices;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestGridUpdate : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private ItemCollection<TestItem> items;
			protected SynchronizationContext context;

			public Instance()
			{
				context = SynchronizationContext.Current;
				if (context == null)
					context = new SynchronizationContext();
			}

			public override void Load(Call call)
			{
				items = new ItemCollection<TestItem>();
				AddEntries();
				tabModel.Items = items;

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					//new TaskAction("Add Entries", AddEntries),
					new TaskDelegate("Start bigNumber++ Thread", UpdateCounter, true),
				};
			}

			private void AddEntries()
			{
				for (int i = 0; i < 20; i++)
				{
					TestItem testItem = new TestItem(context);
					testItem.smallNumber = i;
					testItem.bigNumber += i;
					items.Add(testItem);
				}
			}

			private void UpdateCounter(Call call)
			{
				while (true)
				{
					for (int i = 0; i < 10000; i++)
					{
						Thread.Sleep(10);
						foreach (TestItem testItem in items)
						{
							testItem.bigNumber++;
							testItem.Update();
						}
					}
				}
			}
		}

		public class TestItem : INotifyPropertyChanged
		{
			public int smallNumber { get; set; } = 0;
			public long bigNumber { get; set; } = 1234567890123456789;

			protected SynchronizationContext context;

			public TestItem(SynchronizationContext context)
			{
				this.context = context;
			}

			public override string ToString()
			{
				return smallNumber.ToString();
			}

			public event PropertyChangedEventHandler PropertyChanged;

			public void Update()
			{
				//context.Post(new SendOrPostCallback(this.OnUpdateProgress), eventArgs);
				//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("bigNumber"));
				NotifyPropertyChanged(nameof(bigNumber));
			}

			public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
			{
				context.Post(new SendOrPostCallback(NotifyPropertyChangedContext), propertyName);
				//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}

			private void NotifyPropertyChangedContext(object state)
			{
				string propertyName = state as string;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
