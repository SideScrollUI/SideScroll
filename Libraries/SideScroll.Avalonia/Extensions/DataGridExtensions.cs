using Avalonia.Controls;
using Avalonia.Data;
using SideScroll.Avalonia.Controls.DataGrids;
using SideScroll.Extensions;
using SideScroll.Utilities;
using System.Collections;
using System.Reflection;
using System.Text;
using static SideScroll.Avalonia.Utilities.DataGridUtils;

namespace SideScroll.Avalonia.Extensions;

public static class DataGridExtensions
{
	public static int MaxValueLength { get; set; } = 2000;

	public static string ColumnToStringTable(this DataGrid dataGrid, DataGridBoundColumn column)
	{
		ArgumentNullException.ThrowIfNull(dataGrid);
		ArgumentNullException.ThrowIfNull(column);

		var sb = new StringBuilder();
		foreach (var item in dataGrid.ItemsSource)
		{
			string? value = GetCellValue(column, item);
			sb.AppendLine(value);
		}
		return sb.ToString();
	}

	public static string SelectedColumnToString(this DataGrid dataGrid, DataGridBoundColumn column)
	{
		ArgumentNullException.ThrowIfNull(dataGrid);
		ArgumentNullException.ThrowIfNull(column);

		var sb = new StringBuilder();
		foreach (var item in dataGrid.SelectedItems)
		{
			string? value = GetCellValue(column, item);
			sb.AppendLine(value);
		}
		return sb.ToString();
	}

	private static string? GetCellValue(DataGridBoundColumn column, object item)
	{
		Binding binding = (Binding)column.Binding;
		string propertyName = binding.Path;
		Type type = item.GetType();
		PropertyInfo? propertyInfo = type.GetProperty(propertyName);
		if (propertyInfo != null)
		{
			object? obj = propertyInfo.GetValue(item);
			string? value = GetFormattedCellText(column, obj);
			return value;
		}
		else
		{
			return '(' + propertyName + ')';
		}
		//object content = column.GetCellValue(item, column.ClipboardContentBinding);
	}

	public static string? RowToString(this DataGrid dataGrid, object? obj)
	{
		ArgumentNullException.ThrowIfNull(dataGrid);

		if (obj == null)
			return null;

		Type type = obj.GetType();
		var sb = new StringBuilder();
		foreach (DataGridColumn column in dataGrid.Columns)
		{
			// Buttons don't have a binding
			if (column is not DataGridBoundColumn boundColumn ||
				boundColumn.Binding is not Binding binding)
				continue;

			string propertyName = binding.Path;
			sb.Append(propertyName + ": ");
			PropertyInfo? propertyInfo = type.GetProperty(propertyName);
			if (propertyInfo != null)
			{
				object? value = propertyInfo.GetValue(obj);
				string? valueText = GetFormattedCellText(boundColumn, value);
				sb.AppendLine(valueText);
			}
			else
			{
				sb.AppendLine('(' + propertyName + ')');
			}
		}
		return sb.ToString();
	}

	public static string SelectedToString(this DataGrid dataGrid)
	{
		ArgumentNullException.ThrowIfNull(dataGrid);

		GetDataGridContents(dataGrid, dataGrid.SelectedItems,
			out List<ColumnInfo> columns,
			out List<List<string>> contentRows);

		return TableToString(columns, contentRows);
	}

	public static string SelectedToCsv(this DataGrid dataGrid)
	{
		ArgumentNullException.ThrowIfNull(dataGrid);

		GetDataGridContents(dataGrid, dataGrid.SelectedItems,
			out List<ColumnInfo> columns,
			out List<List<string>> contentRows);

		return TableToCsv(columns, contentRows);
	}

	public static string ToStringTable(this DataGrid dataGrid)
	{
		ArgumentNullException.ThrowIfNull(dataGrid);

		GetDataGridContents(dataGrid, dataGrid.ItemsSource,
			out List<ColumnInfo> columns,
			out List<List<string>> contentRows);

		return TableToString(columns, contentRows);
	}

	public static string ToCsv(this DataGrid dataGrid)
	{
		ArgumentNullException.ThrowIfNull(dataGrid);

		GetDataGridContents(dataGrid, dataGrid.ItemsSource,
			out List<ColumnInfo> columns,
			out List<List<string>> contentRows);

		return TableToCsv(columns, contentRows);
	}

	private static string TableToCsv(List<ColumnInfo> columns, List<List<string>> contentRows)
	{
		var stringBuilder = new StringBuilder();
		bool addComma = false;
		foreach (ColumnInfo columnInfo in columns)
		{
			if (addComma)
				stringBuilder.Append(',');
			addComma = true;
			stringBuilder.Append(columnInfo.Name);
		}
		stringBuilder.Append('\n');

		foreach (var row in contentRows)
		{
			addComma = false;
			foreach (string value in row)
			{
				if (addComma)
					stringBuilder.Append(',');
				addComma = true;

				string text = value ?? "";
				text = text.Replace("\"", "\"\""); // escape double quote
				stringBuilder.Append('"');
				stringBuilder.Append(text);
				stringBuilder.Append('"');
			}
			stringBuilder.Append('\n');
		}

		return stringBuilder.ToString();
	}

	private static void GetDataGridContents(DataGrid dataGrid, IEnumerable items, out List<ColumnInfo> columns, out List<List<string>> contentRows, int? maxValueLength = null)
	{
		columns = [];
		contentRows = [];
		if (dataGrid == null || items == null) return;

		var visibleColumns = new Dictionary<int, DataGridColumn>();

		foreach (DataGridColumn dataColumn in dataGrid.Columns)
		{
			if (dataColumn.IsVisible && dataColumn is DataGridBoundColumn boundColumn && boundColumn.Binding != null)
			{
				visibleColumns[dataColumn.DisplayIndex] = dataColumn;
			}
		}

		foreach (DataGridColumn dataColumn in visibleColumns.Values)
		{
			var columnInfo = new ColumnInfo((string)dataColumn.Header);
			if (dataColumn is DataGridPropertyTextColumn propertyColumn)
			{
				columnInfo.RightAlign = GetTextAlignment(propertyColumn.PropertyInfo.PropertyType);
			}
			else if (dataColumn is DataGridBoundTextColumn boundColumn)
			{
				columnInfo.RightAlign = GetTextAlignment(boundColumn.DataColumn.DataType);
			}
			columns.Add(columnInfo);
		}

		//var collection = (ICollectionView)dataGrid.Items;
		int maxLength = maxValueLength ?? MaxValueLength;
		foreach (var item in items)
		{
			var stringCells = new List<string>();
			foreach (DataGridColumn dataColumn in visibleColumns.Values)
			{
				if (dataColumn is DataGridBoundColumn boundColumn)
				{
					Binding binding = (Binding)boundColumn.Binding;
					string propertyPath = binding.Path;
					object? obj = ReflectorUtil.FollowPropertyPath(item, propertyPath);
					string? value = GetFormattedCellText(boundColumn, obj, maxLength);
					value = value?.Replace('\n', ' '); // remove newlines
					stringCells.Add(value ?? "");
				}
				//object content = dataColumn.GetCellValue(item, dataColumn.ClipboardContentBinding);
			}

			contentRows.Add(stringCells);
		}
	}

	private static string? GetFormattedCellText(DataGridBoundColumn boundColumn, object? obj, int? maxLength = null)
	{
		int maxValueLength = maxLength ?? MaxValueLength;
		if (boundColumn is DataGridPropertyTextColumn textColumn)
		{
			return textColumn.FormatConverter.ObjectToString(obj, maxValueLength);
		}
		else
		{
			return obj.Formatted(maxValueLength);
		}
	}
}
