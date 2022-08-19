using Atlas.Core;
using Atlas.Tabs;
using Avalonia.Interactivity;

namespace Atlas.Start.Avalonia.Tabs;

public class TabCustomControl : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private ItemCollection<MyParams> _items = new();
		private MyParams? _myParams;
		private TabControlSearchToolbar? _toolbar;

		public override void LoadUI(Call call, TabModel model)
		{
			_myParams = new MyParams();
			var tabMyParams = new TabControlMyParams(this, _myParams);
			model.AddObject(tabMyParams);

			_toolbar = new TabControlSearchToolbar(this);
			model.AddObject(_toolbar);

			_toolbar.ButtonSearch.Click += ButtonSearch_Click;  // move logic into SearchToolbar Command
			_toolbar.ButtonLoadNext.Click += ButtonLoadNext_Click;
			_toolbar.ButtonCopyClipBoard.Click += ButtonCopyClipBoard_Click;

			_items = new ItemCollection<MyParams>();
			for (int i = 0; i < 10; i++)
			{
				var item = new MyParams()
				{
					Name = "Item " + i.ToString(),
					Amount = i,
				};
				_items.Add(item);
			}
			model.Items = _items;
		}

		private void ButtonSearch_Click(object? sender, RoutedEventArgs e)
		{
			_toolbar!.TextBoxStatus.Text = "Searching";

			StartTask(Search, true, true);
		}

		private void Search(Call call)
		{
			System.Threading.Thread.Sleep(2000);

			Invoke(ShowSearchResults, 1, "abc");
		}

		private void ShowSearchResults(Call call, params object[] objects)
		{
			_toolbar!.TextBoxStatus.Text = "Finished";
		}

		private void ButtonLoadNext_Click(object? sender, RoutedEventArgs e)
		{
			StartTask(LoadNext, true, false);
		}

		private void LoadNext(Call call)
		{
		}

		private void ButtonCopyClipBoard_Click(object? sender, RoutedEventArgs e)
		{
		}
	}
}
