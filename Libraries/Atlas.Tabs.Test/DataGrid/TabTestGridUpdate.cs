using Atlas.Core;
using System.ComponentModel;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestGridUpdate : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private ItemCollection<TestItem> _items;
			protected SynchronizationContext Context;

			public Instance()
			{
				Context = SynchronizationContext.Current ?? new SynchronizationContext();
			}

			public override void Load(Call call, TabModel model)
			{
				_items = new ItemCollection<TestItem>();
				AddEntries();
				model.Items = _items;

				model.Actions = new List<TaskCreator>()
				{
					//new TaskAction("Add Entries", AddEntries),
					new TaskDelegate("Start bigNumber++ Thread", UpdateCounter, true),
				};
			}

			private void AddEntries()
			{
				for (int i = 0; i < 20; i++)
				{
					var testItem = new TestItem(Context);
					testItem.SmallNumber = i;
					testItem.BigNumber += i;
					_items.Add(testItem);
				}
			}

			private void UpdateCounter(Call call)
			{
				while (true)
				{
					for (int i = 0; i < 10000; i++)
					{
						Thread.Sleep(10);
						foreach (TestItem testItem in _items)
						{
							testItem.BigNumber++;
							testItem.Update();
						}
					}
				}
			}
		}

		public class TestItem : INotifyPropertyChanged
		{
			public int SmallNumber { get; set; } = 0;
			public long BigNumber { get; set; } = 1234567890123456789;

			protected SynchronizationContext Context;

			public override string ToString() => SmallNumber.ToString();

			public TestItem(SynchronizationContext context)
			{
				Context = context;
			}

			public event PropertyChangedEventHandler PropertyChanged;

			public void Update()
			{
				//context.Post(new SendOrPostCallback(this.OnUpdateProgress), eventArgs);
				//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("bigNumber"));
				NotifyPropertyChanged(nameof(BigNumber));
			}

			public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
			{
				Context.Post(new SendOrPostCallback(NotifyPropertyChangedContext), propertyName);
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
