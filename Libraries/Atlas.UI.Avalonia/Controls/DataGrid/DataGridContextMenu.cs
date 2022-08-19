using Atlas.Extensions;
using Atlas.UI.Avalonia.View;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace Atlas.UI.Avalonia;

public class DataGridContextMenu : ContextMenu, IStyleable, IDisposable
{
	Type IStyleable.StyleKey => typeof(ContextMenu);

	private const int MaxCellValueLength = 10000;

	public DataGrid DataGrid;
	public DataGridPropertyTextColumn? Column;
	public DataGridCell? Cell;

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

		list.Add(new Separator());

		var menuItemCopyDataGrid = new TabMenuItem("Copy - _DataGrid");
		menuItemCopyDataGrid.Click += MenuItemCopyDataGrid_Click;
		list.Add(menuItemCopyDataGrid);

		var menuItemCopyDataGridCsv = new TabMenuItem("Copy - DataGrid - CS_V");
		menuItemCopyDataGridCsv.Click += MenuItemCopyDataGridCsv_Click;
		list.Add(menuItemCopyDataGridCsv);

		Items = list;

		DataGrid.CellPointerPressed += DataGrid_CellPointerPressed;
	}

	private void DataGrid_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
	{
		Cell = e.Cell;
		Column = e.Column as DataGridPropertyTextColumn;
	}

	private async void MenuItemCopyDataGrid_Click(object? sender, RoutedEventArgs e)
	{
		string? text = DataGrid.ToStringTable();
		if (text != null)
			await ClipBoardUtils.SetTextAsync(text);
	}

	private async void MenuItemCopyCellContents_Click(object? sender, RoutedEventArgs e)
	{
		if (Column == null)
			return;

		object content = Cell!.Content;
		if (content is Border border)
			content = border.Child;

		if (content is TextBlock textBlock)
		{
			object textBlockValue = Column.PropertyInfo.GetValue(textBlock.DataContext)!;
			string value = FormatValueConverter.ObjectToString(textBlockValue, MaxCellValueLength, Column.FormatConverter.IsFormatted)!;
			await ClipBoardUtils.SetTextAsync(value);
		}
	}

	private async void MenuItemCopyColumn_Click(object? sender, RoutedEventArgs e)
	{
		if (Column is DataGridPropertyTextColumn column)
		{
			string? text = DataGrid.ColumnToStringTable(column);
			if (text != null)
				await ClipBoardUtils.SetTextAsync(text);
		}
	}

	private async void MenuItemCopyRow_Click(object? sender, RoutedEventArgs e)
	{
		string? text = DataGrid.RowToString(Cell!.DataContext);
		if (text != null)
			await ClipBoardUtils.SetTextAsync(text);
	}

	private async void MenuItemCopySelected_Click(object? sender, RoutedEventArgs e)
	{
		string? text = DataGrid.SelectedToString();
		if (text != null)
			await ClipBoardUtils.SetTextAsync(text);
	}

	private async void MenuItemCopySelectedCsv_Click(object? sender, RoutedEventArgs e)
	{
		string? text = DataGrid.SelectedToCsv();
		if (text != null)
			await ClipBoardUtils.SetTextAsync(text);
	}

	private async void MenuItemCopyDataGridCsv_Click(object? sender, RoutedEventArgs e)
	{
		string? text = DataGrid.ToCsv();
		if (text != null)
			await ClipBoardUtils.SetTextAsync(text);
	}

	public void Dispose()
	{
		DataGrid.CellPointerPressed -= DataGrid_CellPointerPressed;
		Column = null;
		Cell = null;
		Items = null;
	}
}
