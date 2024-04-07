using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Atlas.UI.Avalonia.Themes.Tabs;

public class TabAvaloniaToolTipSample : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonNew { get; set; } = new("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save);
	}

	public class Instance : TabInstance
	{
		private Grid? _grid;

		public override void LoadUI(Call call, TabModel model)
		{
			model.AddObject(new Toolbar());

			_grid = new Grid
			{
				RowDefinitions = new("*"),
				ColumnDefinitions = new("*"),
			};

			_grid.ActualThemeVariantChanged += TextBox_ActualThemeVariantChanged;
			model.AddObject(_grid);

			UpdateTheme();
		}

		private void UpdateTheme()
		{
			var textBox = new TabControlTextBox
			{
				[ToolTip.TipProperty] = "When does a ToolTip get too long and when should\nit wrap lines?",
				[ToolTip.PlacementProperty] = PlacementMode.Right,
			};
			_grid!.Children.Clear();
			_grid.Children.Add(textBox);

			Dispatcher.UIThread.Post(() =>
			{
				if (textBox?.IsLoaded == true)
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
