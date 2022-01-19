using Atlas.Extensions;
using Atlas.UI.Avalonia.View;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Atlas.UI.Avalonia
{
	// Rename to DataGridBoundTextDataColumn?
	public class DataGridBoundTextColumn : DataGridTextColumn
	{
		public DataGrid DataGrid;
		public DataColumn DataColumn;
		public int MaxDesiredWidth = 500;
		public bool WordWrap;

		public DataGridBoundTextColumn(DataGrid dataGrid, DataColumn dataColumn)
		{
			DataGrid = dataGrid;
			DataColumn = dataColumn;
			//AddHeaderContextMenu();
		}

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			cell.MaxHeight = 100; // don't let them have more than a few lines each

			TextBlock textBlock = CreateTextBlock();
			//TextBlock textBlock = (TextBlock)base.GenerateElement(cell, dataItem);
			textBlock.TextAlignment = DataGridUtils.GetTextAlignment(DataColumn.DataType);
			AddTextBlockContextMenu(textBlock);
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

		protected TextBlock CreateTextBlock()
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
			if (WordWrap)
			{
				textBlockElement.TextWrapping = TextWrapping.Wrap;
			}
			return textBlockElement;
		}

		// Adds a context menu to the text block
		/*private void AddHeaderContextMenu()
		{
			ContextMenu contextMenu = new();

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
		}*/

		// Adds a context menu to the text block
		private void AddTextBlockContextMenu(TextBlock textBlock)
		{
			var contextMenu = new ContextMenu();

			var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

			var list = new AvaloniaList<object>();

			var menuItemCopy = new TabMenuItem("_Copy - Cell Contents");
			menuItemCopy.Click += delegate
			{
				ClipBoardUtils.SetText(textBlock.Text);
			};
			list.Add(menuItemCopy);

			list.Add(new Separator());

			var menuItemCopyDataGrid = new TabMenuItem("Copy - _DataGrid");
			menuItemCopyDataGrid.Click += delegate
			{
				string text = DataGrid.ToStringTable();
				if (text != null)
					ClipBoardUtils.SetText(text);
			};
			list.Add(menuItemCopyDataGrid);

			var menuItemCopyDataGridCsv = new TabMenuItem("Copy - DataGrid - C_SV");
			menuItemCopyDataGridCsv.Click += delegate
			{
				string text = DataGrid.ToCsv();
				if (text != null)
					ClipBoardUtils.SetText(text);
			};
			list.Add(menuItemCopyDataGridCsv);

			//list.Add(new Separator());

			contextMenu.Items = list;

			textBlock.ContextMenu = contextMenu;
		}
	}
}
