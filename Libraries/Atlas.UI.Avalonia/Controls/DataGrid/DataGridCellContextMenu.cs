using Atlas.Extensions;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using System;

namespace Atlas.UI.Avalonia
{
	public class DataGridCellContextMenu : ContextMenu, IStyleable
	{
		Type IStyleable.StyleKey => typeof(ContextMenu);

		public DataGrid DataGrid;
		public DataGridPropertyTextColumn Column;
		public DataGridCell Cell;
		public TextBlock TextBlock;

		public DataGridCellContextMenu(DataGrid dataGrid, DataGridPropertyTextColumn column, DataGridCell cell, TextBlock textBlock)
		{
			DataGrid = dataGrid;
			Column = column;
			Cell = cell;
			TextBlock = textBlock;

			Initialize();
		}

		private void Initialize()
		{
			var list = new AvaloniaList<object>();

			var menuItemCopyCellContents = new MenuItem() { Header = "Copy - _Cell Contents" };
			menuItemCopyCellContents.Click += MenuItemCopyCellContents_Click;
			list.Add(menuItemCopyCellContents);

			list.Add(new Separator());

			var menuItemCopyColumn = new MenuItem() { Header = "Copy - Co_lumn" };
			menuItemCopyColumn.Click += MenuItemCopyColumn_Click;
			list.Add(menuItemCopyColumn);

			var menuItemCopyRow = new MenuItem() { Header = "Copy - _Row" };
			menuItemCopyRow.Click += MenuItemCopyRow_Click;
			list.Add(menuItemCopyRow);

			var menuItemCopySelected = new MenuItem() { Header = "Copy - _Selected" };
			menuItemCopySelected.Click += MenuItemCopySelected_Click;
			list.Add(menuItemCopySelected);

			var menuItemCopyDataGrid = new MenuItem() { Header = "Copy - _DataGrid" };
			menuItemCopyDataGrid.Click += MenuItemCopyDataGrid_Click;
			list.Add(menuItemCopyDataGrid);

			var menuItemCopyDataGridCsv = new MenuItem() { Header = "Copy - DataGrid - CS_V" };
			menuItemCopyDataGridCsv.Click += MenuItemCopyDataGridCsv_Click;
			list.Add(menuItemCopyDataGridCsv);

			Items = list;
		}

		private async void MenuItemCopyDataGrid_Click(object sender, RoutedEventArgs e)
		{
			string text = DataGrid.ToStringTable();
			if (text != null)
				await ClipBoardUtils.SetTextAsync(text);
		}

		private async void MenuItemCopyCellContents_Click(object sender, RoutedEventArgs e)
		{
			await ClipBoardUtils.SetTextAsync(TextBlock.Text);
		}

		private async void MenuItemCopyColumn_Click(object sender, RoutedEventArgs e)
		{
			string text = DataGrid.ColumnToStringTable(Column);
			if (text != null)
				await ClipBoardUtils.SetTextAsync(text);
		}

		private async void MenuItemCopyRow_Click(object sender, RoutedEventArgs e)
		{
			string text = DataGrid.RowToString(Cell.DataContext);
			if (text != null)
				await ClipBoardUtils.SetTextAsync(text);
		}

		private async void MenuItemCopySelected_Click(object sender, RoutedEventArgs e)
		{
			string text = DataGrid.SelectedToString();
			if (text != null)
				await ClipBoardUtils.SetTextAsync(text);
		}

		private async void MenuItemCopyDataGridCsv_Click(object sender, RoutedEventArgs e)
		{
			string text = DataGrid.ToCsv();
			if (text != null)
				await ClipBoardUtils.SetTextAsync(text);
		}
	}
}