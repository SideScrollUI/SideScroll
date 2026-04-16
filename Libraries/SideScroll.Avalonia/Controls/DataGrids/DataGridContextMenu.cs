using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SideScroll.Avalonia.Extensions;
using SideScroll.Avalonia.Utilities;

namespace SideScroll.Avalonia.Controls.DataGrids;

/// <summary>A context menu for a <see cref="DataGrid"/> that provides copy, select-all, and filter actions, updating its items based on the currently focused column and cell.</summary>
public class DataGridContextMenu : ContextMenu, IDisposable
{
	protected override Type StyleKeyOverride => typeof(ContextMenu);

	/// <summary>Gets or sets the maximum number of characters copied from a cell value to avoid huge clipboard payloads.</summary>
	public static int MaxCellValueLength { get; set; } = 10_000;

	/// <summary>Gets the data grid this context menu is attached to.</summary>
	public DataGrid DataGrid { get; }

	/// <summary>Gets or sets the column the menu was opened on.</summary>
	public DataGridPropertyTextColumn? Column { get; set; }

	/// <summary>Gets or sets the cell the menu was opened on.</summary>
	public DataGridCell? Cell { get; set; }

	public DataGridContextMenu(DataGrid dataGrid)
	{
		DataGrid = dataGrid;

		Initialize();
	}

	private void Initialize()
	{
		var list = new AvaloniaList<object>();

		var menuItemCopyCellContents = new TabMenuItem("Copy - _Cell Contents");
		menuItemCopyCellContents.Click += MenuItemCopyCellContents_Click;
		list.Add(menuItemCopyCellContents);

		var menuItemCopyCellValue = new TabMenuItem("Copy - _Cell Value");
		menuItemCopyCellValue.Click += MenuItemCopyCellValue_Click;
		list.Add(menuItemCopyCellValue);

		list.Add(new Separator());

		var menuItemCopyColumn = new TabMenuItem("Copy - Co_lumn");
		menuItemCopyColumn.Click += MenuItemCopyColumn_Click;
		list.Add(menuItemCopyColumn);

		var menuItemCopyRow = new TabMenuItem("Copy - _Row");
		menuItemCopyRow.Click += MenuItemCopyRow_Click;
		list.Add(menuItemCopyRow);

		list.Add(new Separator());

		var menuItemCopySelected = new TabMenuItem("Copy - _Selected");
		menuItemCopySelected.Click += MenuItemCopySelected_Click;
		list.Add(menuItemCopySelected);

		var menuItemCopySelectedCsv = new TabMenuItem("Copy - Selected - CSV");
		menuItemCopySelectedCsv.Click += MenuItemCopySelectedCsv_Click;
		list.Add(menuItemCopySelectedCsv);

		var menuItemCopySelectedColumn = new TabMenuItem("Copy - Selected - Column");
		menuItemCopySelectedColumn.Click += MenuItemCopySelectedColumn_Click;
		list.Add(menuItemCopySelectedColumn);

		list.Add(new Separator());

		var menuItemCopyDataGrid = new TabMenuItem("Copy - _DataGrid");
		menuItemCopyDataGrid.Click += MenuItemCopyDataGrid_Click;
		list.Add(menuItemCopyDataGrid);

		var menuItemCopyDataGridCsv = new TabMenuItem("Copy - DataGrid - CS_V");
		menuItemCopyDataGridCsv.Click += MenuItemCopyDataGridCsv_Click;
		list.Add(menuItemCopyDataGridCsv);

		ItemsSource = list;

		DataGrid.CellPointerPressed += DataGrid_CellPointerPressed;
	}

	private void DataGrid_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
	{
		Cell = e.Cell;
		Column = e.Column as DataGridPropertyTextColumn;
	}

	private async void MenuItemCopyDataGrid_Click(object? sender, RoutedEventArgs e)
	{
		string text = DataGrid.ToStringTable();
		await ClipboardUtils.SetTextAsync(DataGrid, text);
	}

	private async void MenuItemCopyCellContents_Click(object? sender, RoutedEventArgs e)
	{
		await CopyCellContents(true);
	}

	private async void MenuItemCopyCellValue_Click(object? sender, RoutedEventArgs e)
	{
		await CopyCellContents(false);
	}

	private async Task CopyCellContents(bool formatted)
	{
		if (Column == null) return;

		object? content = Cell?.Content;
		if (content is Border border)
		{
			content = border.Child;
		}

		if (content is TextBlock textBlock)
		{
			object propertyValue = Column.PropertyInfo.GetValue(textBlock.DataContext)!;
			string value;
			Type valueType = propertyValue.GetType();
			if (formatted || (valueType != typeof(string) && !valueType.IsPrimitive))
			{
				value = Column.FormatConverter.ObjectToString(propertyValue, MaxCellValueLength)!;
			}
			else
			{
				value = propertyValue.ToString() ?? "";
			}

			await ClipboardUtils.SetTextAsync(DataGrid, value);
		}
	}

	private async void MenuItemCopyColumn_Click(object? sender, RoutedEventArgs e)
	{
		if (Column is DataGridBoundColumn column)
		{
			string text = DataGrid.ColumnToStringTable(column);
			await ClipboardUtils.SetTextAsync(DataGrid, text);
		}
	}

	private async void MenuItemCopyRow_Click(object? sender, RoutedEventArgs e)
	{
		string? text = DataGrid.RowToString(Cell!.DataContext);
		if (text != null)
		{
			await ClipboardUtils.SetTextAsync(DataGrid, text);
		}
	}

	private async void MenuItemCopySelected_Click(object? sender, RoutedEventArgs e)
	{
		string text = DataGrid.SelectedToString();
		await ClipboardUtils.SetTextAsync(DataGrid, text);
	}

	private async void MenuItemCopySelectedCsv_Click(object? sender, RoutedEventArgs e)
	{
		string text = DataGrid.SelectedToCsv();
		await ClipboardUtils.SetTextAsync(DataGrid, text);
	}

	private async void MenuItemCopySelectedColumn_Click(object? sender, RoutedEventArgs e)
	{
		if (Column is DataGridBoundColumn column)
		{
			string text = DataGrid.SelectedColumnToString(column);
			await ClipboardUtils.SetTextAsync(DataGrid, text);
		}
	}

	private async void MenuItemCopyDataGridCsv_Click(object? sender, RoutedEventArgs e)
	{
		string text = DataGrid.ToCsv();
		await ClipboardUtils.SetTextAsync(DataGrid, text);
	}

	public void Dispose()
	{
		DataGrid.CellPointerPressed -= DataGrid_CellPointerPressed;
		Column = null;
		Cell = null;
		ItemsSource = null;
	}
}
