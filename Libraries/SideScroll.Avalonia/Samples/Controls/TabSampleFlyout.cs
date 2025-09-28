using Avalonia;
using Avalonia.Controls;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Controls.Flyouts;
using SideScroll.Tabs;
using System.Reflection;

namespace SideScroll.Avalonia.Samples.Controls;

public class TabSampleFlyout : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private Border? _border;

		public override void LoadUI(Call call, TabModel model)
		{
			model.MinDesiredWidth = 300;
			//model.ReloadOnThemeChange = true; // todo: Doesn't work for custom controls yet

			Control? flyoutControl = CreateFlyout();

			_border = new()
			{
				Child = flyoutControl,
				Margin = new Thickness(8),
			};
			model.AddObject(_border);
			_border.ActualThemeVariantChanged += Border_ActualThemeVariantChanged;
		}

		private static Control? CreateFlyout()
		{
			var flyout = new ConfirmationFlyout(() => { }, "Confirm all the things?")
			{
				Placement = PlacementMode.BottomEdgeAlignedLeft,
				ShowMode = FlyoutShowMode.Transient,
			};

			MethodInfo createPresenterMethod = flyout.GetType()
				.GetMethod("CreatePresenter", BindingFlags.NonPublic | BindingFlags.Instance)!;
			object? flyoutControl = createPresenterMethod.Invoke(flyout, []);
			return flyoutControl as Control;
		}

		private void Border_ActualThemeVariantChanged(object? sender, EventArgs e)
		{
			if (sender is Control control && control.IsLoaded)
			{
				_border!.Child = CreateFlyout();
			}
		}
	}
}
