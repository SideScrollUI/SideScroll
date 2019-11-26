using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using System;
using System.Data;

namespace Atlas.GUI.Avalonia
{
	// Rename to DataGridBoundTextDataColumn?
	public class DataGridBoundTextColumn : DataGridTextColumn
	{
		private DataGrid dataGrid;
		public DataColumn dataColumn;
		public int MaxDesiredWidth = 500;

		public DataGridBoundTextColumn(DataGrid dataGrid, DataColumn dataColumn)
		{
			this.dataGrid = dataGrid;
			this.dataColumn = dataColumn;
			//AddHeaderContextMenu();
		}

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			//cell.Background = GetCellBrush(cell, dataItem);
			cell.MaxHeight = 100; // don't let them have more than a few lines each

			TextBlock textBlock = CreateTextBlock(cell, dataItem);
			//TextBlock textBlock = (TextBlock)base.GenerateElement(cell, dataItem);
			textBlock.TextAlignment = DataGridUtils.GetTextAlignment(dataColumn.DataType);
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
				availableSize = new Size(Math.Min(maxDesiredWidth, availableSize.Width), availableSize.Height);
				Size measured = base.MeasureCore(availableSize);
				measured = new Size(Math.Min(maxDesiredWidth, measured.Width), measured.Height);
				return measured;
			}
		}

		protected TextBlock CreateTextBlock(DataGridCell cell, object dataItem)
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

			MenuItem menuItemCopyDataGridCsv = new MenuItem() { Header = "Copy - DataGrid - C_SV" };
			menuItemCopyDataGridCsv.Click += delegate
			{
				string text = DataGridUtils.DataGridToCsv(dataGrid);
				if (text != null)
					((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(text);
			};
			list.Add(menuItemCopyDataGridCsv);

			//list.Add(new Separator());

			contextMenu.Items = list;

			textBlock.ContextMenu = contextMenu;
		}
	}
}