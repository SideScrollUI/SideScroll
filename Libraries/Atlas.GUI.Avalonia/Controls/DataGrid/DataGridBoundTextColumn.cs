using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace Atlas.GUI.Avalonia
{
	public class DataGridBoundTextColumn : DataGridTextColumn
	{
		public DataGridBoundTextColumn()
		{
			//AddHeaderContextMenu();
		}

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			//cell.Background = GetCellBrush(cell, dataItem);
			cell.MaxHeight = 100; // don't let them have more than a few lines each

			TextBlock textBlock = (TextBlock)base.GenerateElement(cell, dataItem);
			AddTextBoxContextMenu(textBlock);
			return textBlock;
		}

		// Adds a context menu to the text block
		/*private void AddHeaderContextMenu()
		{
			ContextMenu contextMenu = new ContextMenu();

			var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

			var list = new AvaloniaList<object>();

			MenuItem menuItemCopy = new MenuItem() { Header = "_Copy - Column" };
			menuItemCopy.Click += delegate
			{
				((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(ColumnText);
			};
			list.Add(menuItemCopy);

			//list.Add(new Separator());

			contextMenu.Items = list;

			//this.ContextMenu = contextMenu;
		}


		public string ColumnText
		{
			get
			{
				return this.
			}
		}*/

		// Adds a context menu to the text block
		private void AddTextBoxContextMenu(TextBlock textBlock)
		{
			ContextMenu contextMenu = new ContextMenu();

			var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

			var list = new AvaloniaList<object>();

			MenuItem menuItemCopy = new MenuItem() { Header = "_Copy - Cell Contents" };
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