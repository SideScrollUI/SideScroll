using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace Atlas.GUI.Avalonia
{
	public class DataGridBoundTextColumn : DataGridTextColumn
	{
		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			//cell.Background = GetCellBrush(cell, dataItem);
			cell.MaxHeight = 100; // don't let them have more than a few lines each

			TextBlock textBlock = (TextBlock)base.GenerateElement(cell, dataItem);
			textBlock.DoubleTapped += delegate
			{
				((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(textBlock.Text);
			};
			AddTextBoxContextMenu(textBlock);
			return textBlock;
		}

		private void AddTextBoxContextMenu(TextBlock textBlock)
		{
			ContextMenu contextMenu = new ContextMenu();

			var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

			var list = new AvaloniaList<object>();

			MenuItem menuItemCopy = new MenuItem() { Header = "_Copy" };
			menuItemCopy.Click += delegate
			{
				((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(textBlock.Text);
			};
			list.Add(menuItemCopy);

			//list.Add(new Separator());

			contextMenu.Items = list;

			textBlock.ContextMenu = contextMenu;
		}
	}
}