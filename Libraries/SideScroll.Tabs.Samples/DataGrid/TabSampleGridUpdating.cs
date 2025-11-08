using SideScroll.Collections;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleGridUpdating : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonStart { get; set; } = new("Start", Icons.Svg.Play, backgroundThread: true);
		public ToolButton ButtonStop { get; set; } = new("Stop", Icons.Svg.Stop);
	}

	public class Instance : TabInstance
	{
		protected SynchronizationContext Context = SynchronizationContext.Current ?? new();

		private ItemCollection<TestItem> _items = [];
		private Call? _counterCall;

		public override void Load(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonStart.ActionAsync = StartCounterAsync;
			toolbar.ButtonStop.Action = StopCounter;
			model.AddObject(toolbar);

			_items = [];
			AddEntries();
			model.Items = _items;
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

		private async Task StartCounterAsync(Call call)
		{
			StopCounter(call);

			_counterCall = call;
			for (int i = 0; i < 1000 && !call.TaskInstance!.CancelToken.IsCancellationRequested; i++)
			{
				foreach (TestItem testItem in _items)
				{
					testItem.BigNumber++;
					testItem.Update();
				}
				await Task.Delay(500);
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
