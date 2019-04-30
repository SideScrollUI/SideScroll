using System;
using Atlas.Core;
using Atlas.Tabs;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabCustomControl : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private MyParams myParams;
			private TabControlSearchToolbar searchToolbar;

			public override void Load(Call call)
			{
				myParams = new MyParams();
				TabControlMyParams tabMyParams = new TabControlMyParams(this, myParams);
				tabModel.AddObject(tabMyParams);

				searchToolbar = new TabControlSearchToolbar();
				tabModel.AddObject(searchToolbar);

				searchToolbar.buttonSearch.Click += ButtonSearch_Click;  // move logic into SearchToolbar Command
				searchToolbar.buttonLoadNext.Click += ButtonLoadNext_Click;
				searchToolbar.buttonCopyClipBoard.Click += ButtonCopyClipBoard_Click; ;
			}

			private void ButtonSearch_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
				searchToolbar.textBlockStatus.Text = "Searching";
				StartTask(Search, true);
			}

			private void Search(Call call)
			{
				System.Threading.Thread.Sleep(2000);
				Invoke(ShowSearchResults, 1, "abc");
			}

			private void ShowSearchResults(Call call, params object[] objects)
			{
				searchToolbar.textBlockStatus.Text = "Finished";
			}

			private void ButtonLoadNext_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
				StartTask(LoadNext, true);
			}

			private void LoadNext(Call call)
			{
			}

			private void ButtonCopyClipBoard_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
			}
		}
	}
}
