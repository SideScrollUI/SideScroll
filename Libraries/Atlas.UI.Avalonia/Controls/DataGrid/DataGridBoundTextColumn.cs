using Atlas.Extensions;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Styling;
using System;
using System.Data;

namespace Atlas.UI.Avalonia
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

		public class TextBlockElement : TextBlock, IStyleable, ILayoutable
		{
			Type IStyleable.StyleKey => typeof(TextBlock);

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
			var textBlockElement = new TextBlockElement()
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
				ClipBoardUtils.SetTextAsync(ColumnText);
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
			var contextMenu = new ContextMenu();

			var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

			var list = new AvaloniaList<object>();

			var menuItemCopy = new MenuItem() { Header = "_Copy - Cell Contents" };
			menuItemCopy.Click += delegate
			{
				ClipBoardUtils.SetTextAsync(textBlock.Text);
			};
			list.Add(menuItemCopy);

			list.Add(new Separator());

			var menuItemCopyDataGrid = new MenuItem() { Header = "Copy - _DataGrid" };
			menuItemCopyDataGrid.Click += delegate
			{
				string text = dataGrid.ToStringTable();
				if (text != null)
					ClipBoardUtils.SetTextAsync(text);
			};
			list.Add(menuItemCopyDataGrid);

			var menuItemCopyDataGridCsv = new MenuItem() { Header = "Copy - DataGrid - C_SV" };
			menuItemCopyDataGridCsv.Click += delegate
			{
				string text = dataGrid.ToCsv();
				if (text != null)
					ClipBoardUtils.SetTextAsync(text);
			};
			list.Add(menuItemCopyDataGridCsv);

			//list.Add(new Separator());

			contextMenu.Items = list;

			textBlock.ContextMenu = contextMenu;
		}
	}
}