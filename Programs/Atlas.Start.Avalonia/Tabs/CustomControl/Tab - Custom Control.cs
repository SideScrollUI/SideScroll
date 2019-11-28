using System;
using System.Threading.Tasks;
using Atlas.Core;
using Atlas.GUI.Avalonia.Controls;
using Atlas.Tabs;
using Avalonia.Animation;
using Avalonia.Threading;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabCustomControl : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private ItemCollection<MyParams> items;
			private MyParams myParams;
			private TabControlSearchToolbar searchToolbar;
			private TabControlLoadingAnimation animation;

			public override void Load(Call call)
			{
				myParams = new MyParams();
				TabControlMyParams tabMyParams = new TabControlMyParams(this, myParams);
				tabModel.AddObject(tabMyParams);

				searchToolbar = new TabControlSearchToolbar();
				tabModel.AddObject(searchToolbar);

				animation = new TabControlLoadingAnimation()
				{
					IsVisible = false,
				};
				tabModel.AddObject(animation);

				searchToolbar.buttonSearch.Click += ButtonSearch_Click;  // move logic into SearchToolbar Command
				searchToolbar.buttonLoadNext.Click += ButtonLoadNext_Click;
				searchToolbar.buttonCopyClipBoard.Click += ButtonCopyClipBoard_Click;
				searchToolbar.buttonSleep.Click += ButtonSleep_Click;

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
				tabModel.Items = items;
			}

			private void ButtonSearch_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
			{
				searchToolbar.textBoxStatus.Text = "Searching";
				StartTask(Search, true, true);
			}

			private void Search(Call call)
			{
				System.Threading.Thread.Sleep(2000);
				Invoke(ShowSearchResults, 1, "abc");
			}

			private void ShowSearchResults(Call call, params object[] objects)
			{
				searchToolbar.textBoxStatus.Text = "Finished";
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
