using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Avalonia.Extensions;
using SideScroll.Avalonia.Utilities;
using System.Data;

namespace SideScroll.Avalonia.Controls.DataGrids;

// Rename to DataGridBoundTextDataColumn?
public class DataGridBoundTextColumn : DataGridTextColumn
{
	public DataGrid DataGrid { get; }
	public DataColumn DataColumn { get; }

	public int MaxDesiredWidth { get; set; } = 500;
	public bool WordWrap { get; set; }

	public DataGridBoundTextColumn(DataGrid dataGrid, DataColumn dataColumn)
	{
		DataGrid = dataGrid;
		DataColumn = dataColumn;
		//AddHeaderContextMenu();
	}

	protected override Control GenerateElement(DataGridCell cell, object dataItem)
	{
		cell.MaxHeight = 100; // don't let them have more than a few lines each
		cell.BorderThickness = new Thickness(0, 0, 1, 1);

		TextBlock textBlock = CreateTextBlock();
		//TextBlock textBlock = (TextBlock)base.GenerateElement(cell, dataItem);
		textBlock.TextAlignment = TableUtils.GetTextAlignment(DataColumn.DataType);
		AddTextBlockContextMenu(textBlock);
		return textBlock;
	}

	public class TextBlockElement : TextBlock
	{
		protected override Type StyleKeyOverride => typeof(TextBlock);

		public double MaxDesiredWidth = 500;

		// can't override DesiredSize
		protected override Size MeasureCore(Size availableSize)
		{
			double maxDesiredWidth = MaxDesiredWidth;
			availableSize = availableSize.WithWidth(Math.Min(maxDesiredWidth, availableSize.Width));
			Size measured = base.MeasureCore(availableSize);
			measured = measured.WithWidth(Math.Min(maxDesiredWidth, measured.Width));
			return measured;
		}
	}

	protected TextBlock CreateTextBlock()
	{
		var textBlockElement = new TextBlockElement
		{
			Margin = new Thickness(4),
			VerticalAlignment = VerticalAlignment.Center,
			MaxDesiredWidth = this.MaxDesiredWidth,
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
			ClipboardUtils.SetTextAsync(ColumnText);
		};
		list.Add(menuItemCopy);

		//list.Add(new Separator());

		contextMenu.Items = list;

		//this.ContextMenu = contextMenu;
	}*/

	// Adds a context menu to the text block
	private void AddTextBlockContextMenu(TextBlock textBlock)
	{
		var list = new AvaloniaList<object>();

		var menuItemCopy = new TabMenuItem("_Copy - Cell Contents");
		menuItemCopy.Click += async delegate
		{
			await ClipboardUtils.SetTextAsync(DataGrid, textBlock.Text ?? "");
		};
		list.Add(menuItemCopy);

		list.Add(new Separator());

		var menuItemCopyDataGrid = new TabMenuItem("Copy - _DataGrid");
		menuItemCopyDataGrid.Click += async delegate
		{
			string text = DataGrid.ToStringTable();
			await ClipboardUtils.SetTextAsync(DataGrid, text);
		};
		list.Add(menuItemCopyDataGrid);

		var menuItemCopyDataGridCsv = new TabMenuItem("Copy - DataGrid - C_SV");
		menuItemCopyDataGridCsv.Click += async delegate
		{
			string text = DataGrid.ToCsv();
			await ClipboardUtils.SetTextAsync(DataGrid, text);
		};
		list.Add(menuItemCopyDataGridCsv);

		//list.Add(new Separator());

		ContextMenu contextMenu = new()
		{
			ItemsSource = list,
		};

		textBlock.ContextMenu = contextMenu;
	}
}
