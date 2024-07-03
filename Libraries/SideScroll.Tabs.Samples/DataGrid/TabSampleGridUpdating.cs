using SideScroll.Collections;
using SideScroll.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleGridUpdating : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		protected SynchronizationContext Context = SynchronizationContext.Current ?? new SynchronizationContext();

		private ItemCollection<TestItem> _items = [];
		private Call? _counterCall;

		public override void Load(Call call, TabModel model)
		{
			_items = [];
			AddEntries();
			model.Items = _items;

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Start Counter Thread", StartCounter, true),
				new TaskDelegate("Stop Counter Thread", StopCounter, true),
			};
		}

		private void AddEntries()
		{
			for (int i = 0; i < 20; i++)
			{
				var testItem = new TestItem(Context)
				{
					SmallNumber = i
				};
				testItem.BigNumber += i;
				_items.Add(testItem);
			}
		}

		private void StartCounter(Call call)
		{
			_counterCall = call;
			for (int i = 0; i < 1000 && !call.TaskInstance!.CancelToken.IsCancellationRequested; i++)
			{
				foreach (TestItem testItem in _items)
				{
					testItem.BigNumber++;
					testItem.Update();
				}
				Thread.Sleep(500);
			}
		}

		private void StopCounter(Call call)
		{
			_counterCall?.TaskInstance?.Cancel();
			_counterCall = null;
		}
	}

	public class TestItem(SynchronizationContext context) : INotifyPropertyChanged
	{
		public int SmallNumber { get; set; } = 123;
		public long BigNumber { get; set; } = 1234567890123456789;

		protected SynchronizationContext Context = context;

		public event PropertyChangedEventHandler? PropertyChanged;

		public override string ToString() => SmallNumber.ToString();

		public void Update()
		{
			NotifyPropertyChanged(nameof(BigNumber));
		}

		public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			Context.Post(NotifyPropertyChangedContext, propertyName);
		}

		private void NotifyPropertyChangedContext(object? state)
		{
			string propertyName = (string)state!;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
