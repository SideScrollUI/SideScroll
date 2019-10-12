using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using System;

namespace Atlas.GUI.Avalonia
{
	public class DataGridBoundTextColumn : DataGridTextColumn
	{
		private DataGrid dataGrid;
		public int MaxDesiredWidth = 500;

		public DataGridBoundTextColumn(DataGrid dataGrid)
		{
			this.dataGrid = dataGrid;
			//AddHeaderContextMenu();
		}

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			//cell.Background = GetCellBrush(cell, dataItem);
			cell.MaxHeight = 100; // don't let them have more than a few lines each

			TextBlock textBlock = GetTextBlock(cell, dataItem);
			//TextBlock textBlock = (TextBlock)base.GenerateElement(cell, dataItem);
			AddTextBoxContextMenu(textBlock);
			return textBlock;
		}

		public class SubTextBlock : TextBlock
		{
			public double MaxDesiredWidth = 500;

			// can't override DesiredSize
			protected override Size MeasureCore(Size availableSize)
			{
				double maxDesiredWidth = MaxDesiredWidth;

				Size measured = base.MeasureCore(availableSize);
				Size maxSize = new Size(Math.Min(maxDesiredWidth, measured.Width), measured.Height);
				return maxSize;
			}
		}

		protected TextBlock GetTextBlock(DataGridCell cell, object dataItem)
		{
			SubTextBlock textBlockElement = new SubTextBlock()
			{
				Margin = new Thickness(4),
				VerticalAlignment = VerticalAlignment.Center,
				MaxDesiredWidth = this.MaxDesiredWidth,
				//FontFamily
				//FontSize
				//FontStyle
				//FontWeight
				//Foreground
			};

			if (Binding != null)
			{
				textBlockElement.Bind(TextBlock.TextProperty, Binding);
			}
			return textBlockElement;
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

			list.Add(new Separator());

			MenuItem menuItemCopyDataGrid = new MenuItem() { Header = "Copy - _DataGrid" };
			menuItemCopyDataGrid.Click += delegate
			{
				string text = DataGridUtils.DataGridToStringTable(dataGrid);
				if (text != null)
					((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(text);
			};
			list.Add(menuItemCopyDataGrid);

			//list.Add(new Separator());

			contextMenu.Items = list;

			textBlock.ContextMenu = contextMenu;
		}
	}
}