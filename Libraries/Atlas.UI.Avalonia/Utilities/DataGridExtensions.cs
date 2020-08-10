using Atlas.Core;
using Atlas.Extensions;
using Atlas.UI.Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using static Atlas.UI.Avalonia.DataGridUtils;

namespace Atlas.Extensions
{
	public static class DataGridExtensions
	{
		public static string ColumnToStringTable(this DataGrid dataGrid, DataGridBoundColumn column)
		{
			if (dataGrid == null || column == null)
				return null;

			var sb = new StringBuilder();
			foreach (var item in dataGrid.Items)
			{
				Binding binding = (Binding)column.Binding;
				string propertyName = binding.Path;
				Type type = item.GetType();
				PropertyInfo propertyInfo = type.GetProperty(propertyName);
				if (propertyInfo != null)
				{
					object obj = propertyInfo.GetValue(item);
					string value = obj.Formatted();
					sb.AppendLine(value);
				}
				else
				{
					sb.AppendLine('(' + propertyName + ')');
				}
				//object content = dataColumn.GetCellValue(item, dataColumn.ClipboardContentBinding);
			}
			string text = sb.ToString();
			return text;
		}

		public static string RowToString(this DataGrid dataGrid, object obj)
		{
			if (dataGrid == null || obj == null)
				return null;

			Type type = obj.GetType();
			var sb = new StringBuilder();
			foreach (DataGridBoundColumn column in dataGrid.Columns)
			{
				Binding binding = (Binding)column.Binding;
				if (binding == null) // Buttons don't have a binding
					continue;
				string propertyName = binding.Path;
				sb.Append(propertyName + ": ");
				PropertyInfo propertyInfo = type.GetProperty(propertyName);
				if (propertyInfo != null)
				{
					object value = propertyInfo.GetValue(obj);
					string valueText = value.Formatted();
					sb.AppendLine(valueText);
				}
				else
				{
					sb.AppendLine('(' + propertyName + ')');
				}
			}
			string text = sb.ToString();
			return text;
		}

		public static string SelectedToString(this DataGrid dataGrid)
		{
			if (dataGrid == null)
				return null;

			GetDataGridContents(dataGrid, dataGrid.SelectedItems,
				out List<ColumnInfo> columns,
				out List<List<string>> contentRows);

			string text = TableToString(columns, contentRows);
			return text;
		}

		public static string ToStringTable(this DataGrid dataGrid)
		{
			if (dataGrid == null)
				return null;

			GetDataGridContents(dataGrid, dataGrid.Items,
				out List<ColumnInfo> columns,
				out List<List<string>> contentRows);

			string text = TableToString(columns, contentRows);
			return text;
		}

		public static string ToCsv(this DataGrid dataGrid)
		{
			if (dataGrid == null)
				return null;

			GetDataGridContents(dataGrid, dataGrid.Items,
				out List<ColumnInfo> columns,
				out List<List<string>> contentRows);

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

		private static void GetDataGridContents(DataGrid dataGrid, IEnumerable items, out List<ColumnInfo> columns, out List<List<string>> contentRows)
		{
			columns = new List<ColumnInfo>();
			contentRows = new List<List<string>>();
			var visibleColumns = new Dictionary<int, DataGridColumn>();

			foreach (DataGridColumn dataColumn in dataGrid.Columns)
			{
				if (dataColumn.IsVisible && dataColumn is DataGridBoundColumn boundColumn && boundColumn.Binding != null)
					visibleColumns[dataColumn.DisplayIndex] = dataColumn;
			}

			foreach (DataGridColumn dataColumn in visibleColumns.Values)
			{
				var columnInfo = new ColumnInfo((string)dataColumn.Header);
				if (dataColumn is DataGridPropertyTextColumn propertyColumn)
					columnInfo.RightAlign = GetTextAlignment(propertyColumn.propertyInfo.PropertyType);
				else if (dataColumn is DataGridBoundTextColumn boundColumn)
					columnInfo.RightAlign = GetTextAlignment(boundColumn.dataColumn.DataType);
				columns.Add(columnInfo);
			}

			//var collection = (ICollectionView)dataGrid.Items;
			foreach (var item in items)
			{
				var stringCells = new List<string>();
				foreach (DataGridColumn dataColumn in visibleColumns.Values)
				{
					if (dataColumn is DataGridBoundColumn boundColumn)
					{
						Binding binding = (Binding)boundColumn.Binding;
						string propertyPath = binding.Path;
						object obj = ReflectorUtil.FollowPropertyPath(item, propertyPath);

						string value = obj.Formatted();
						value = value?.Replace('\n', ' '); // remove newlines
						stringCells.Add(value);
					}
					//object content = dataColumn.GetCellValue(item, dataColumn.ClipboardContentBinding);
				}

				contentRows.Add(stringCells);
			}
		}

	}
}
