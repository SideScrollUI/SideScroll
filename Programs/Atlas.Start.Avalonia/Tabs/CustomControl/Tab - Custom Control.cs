using System;
using System.Threading.Tasks;
using Atlas.Core;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Avalonia.Animation;
using Avalonia.Threading;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabCustomControl : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private ItemCollection<MyParams> items;
			private MyParams myParams;
			private TabControlSearchToolbar toolbar;
			private TabControlLoadingAnimation animation;

			public override void LoadUI(Call call, TabModel model)
			{
				myParams = new MyParams();
				TabControlMyParams tabMyParams = new TabControlMyParams(this, myParams);
				model.AddObject(tabMyParams);

				toolbar = new TabControlSearchToolbar();
				model.AddObject(toolbar);

				animation = new TabControlLoadingAnimation()
				{
					IsVisible = false,
				};
				model.AddObject(animation);

				toolbar.buttonSearch.Click += ButtonSearch_Click;  // move logic into SearchToolbar Command
				toolbar.buttonLoadNext.Click += ButtonLoadNext_Click;
				toolbar.buttonCopyClipBoard.Click += ButtonCopyClipBoard_Click;
				toolbar.buttonSleep.Click += ButtonSleep_Click;

				items = new ItemCollection<MyParams>();
				for (int i = 0; i < 10; i++)
				{
					var item = new MyParams()
					{
						Name = "Item " + i.ToString(),
						Amount = i,
					};
					items.Add(item);
				}
				model.Items = items;
			}

			private void ButtonSearch_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
				toolbar.textBoxStatus.Text = "Searching";
				StartTask(Search, true, true);
			}

			private void Search(Call call)
			{
				System.Threading.Thread.Sleep(2000);
				Invoke(ShowSearchResults, 1, "abc");
			}

			private void ShowSearchResults(Call call, params object[] objects)
			{
				toolbar.textBoxStatus.Text = "Finished";
			}

			private void ButtonLoadNext_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
				StartTask(LoadNext, true, false);
			}

			private void LoadNext(Call call)
			{
			}

			private void ButtonCopyClipBoard_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
			}

			private async void ButtonSleep_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
				animation.IsVisible = true;

				//Invoke(UpdateAnimationVisible, true);
				Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);
				await Task.Delay(2000);
				animation.IsVisible = false;
				//Invoke(UpdateAnimationVisible, false);
			}

			/*private void ButtonSleep_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
				animation.IsVisible = true;

				Invoke(UpdateAnimationVisible, true);
				Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);
				System.Threading.Thread.Sleep(2000);
				//animation.IsVisible = false;
				Invoke(UpdateAnimationVisible, false);
			}*/

			private void UpdateAnimationVisible(Call call, params object[] objects)
			{
				animation.IsVisible = (bool)objects[0];
			}
		}
	}
}
