using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using SideScroll.Avalonia.Controls;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Avalonia.Themes.Tabs;

public class TabAvaloniaToolTipSample : ITab
{
	public TabInstance Create() => new Instance();

	private class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; } = new("Refresh", Icons.Svg.Refresh);
	}

	private class Instance : TabInstance
	{
		private Grid? _grid;

		public override void LoadUI(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Refresh;
			model.AddObject(toolbar);

			_grid = new Grid
			{
				RowDefinitions = new("*"),
				ColumnDefinitions = new("*"),
			};

			_grid.ActualThemeVariantChanged += TextBox_ActualThemeVariantChanged;
			model.AddObject(_grid);

			UpdateTheme();
		}

		private void Refresh(Call call)
		{
			Reload();
		}

		private void UpdateTheme()
		{
			TabTextBox textBox = new()
			{
				[ToolTip.TipProperty] = "When does a ToolTip get too long and when should\nit wrap lines?",
				[ToolTip.PlacementProperty] = PlacementMode.Right,
				Margin = new Thickness(150, 6),
			};
			_grid!.Children.Clear();
			_grid.Children.Add(textBox);

			Dispatcher.UIThread.Post(() =>
			{
				if (textBox.IsLoaded)
				{
					ToolTip.SetIsOpen(textBox, true);
				}
			}, DispatcherPriority.Background);
		}

		private void TextBox_ActualThemeVariantChanged(object? sender, EventArgs e)
		{
			Dispatcher.UIThread.Post(UpdateTheme, DispatcherPriority.Background);
		}
	}
}
