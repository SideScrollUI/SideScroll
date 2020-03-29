using Atlas.Core;
using Atlas.Extensions;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Atlas.UI.Avalonia
{
	public class DataGridUtils
	{
		public static string DataGridColumnToStringTable(DataGrid dataGrid, DataGridBoundColumn column)
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
		
		public static string DataGridRowToString(DataGrid dataGrid, object obj)
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

		public class ColumnInfo
		{
			public string Name { get; set; }
			public TextAlignment RightAlign { get; set; }

			public ColumnInfo(string name)
			{
				Name = name;
			}
		}

		public static string DataGridToStringTable(DataGrid dataGrid)
		{
			if (dataGrid == null)
				return null;

			List<ColumnInfo> columns;
			List<List<string>> contentRows;
			GetDataGridContents(dataGrid, out columns, out contentRows);

			string text = TableToString(columns, contentRows);
			return text;
		}

		public static string DataGridToCsv(DataGrid dataGrid)
		{
			if (dataGrid == null)
				return null;

			List<ColumnInfo> columns;
			List<List<string>> contentRows;
			GetDataGridContents(dataGrid, out columns, out contentRows);

			StringBuilder stringBuilder = new StringBuilder();
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

		private static void GetDataGridContents(DataGrid dataGrid, out List<ColumnInfo> columns, out List<List<string>> contentRows)
		{
			columns = new List<ColumnInfo>();
			contentRows = new List<List<string>>();
			Dictionary<int, DataGridColumn> visibleColumns = new Dictionary<int, DataGridColumn>();

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
			foreach (var item in dataGrid.Items)
			{
				List<string> stringCells = new List<string>();
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

		public static string TableToString(List<ColumnInfo> columns, List<List<string>> contentRows)
		{
			List<int> columnWidths = new List<int>();
			for (int column = 0; column < columns.Count; column++)
			{
				string header = columns[column].Name;
				int maxWidth = header.Length;
				foreach (List<string> row in contentRows)
				{
					string value = row[column];
					if (value != null)
						maxWidth = Math.Max(maxWidth, value.Length);
				}
				columnWidths.Add(maxWidth);
			}

			string line = "-";
			foreach (int value in columnWidths)
			{
				line += new string('-', value + 3);
			}
			line += '\n';

			StringBuilder stringBuilder = new StringBuilder(line);

			// Column Headers
			stringBuilder.Append("|");
			int columnIndex = 0;
			foreach (var columnInfo in columns)
			{
				int columnWidth = columnWidths[columnIndex++];
				int leftPadding = (columnWidth - columnInfo.Name.Length) / 2;
				int rightPadding = columnWidth - columnInfo.Name.Length - leftPadding;
				stringBuilder.Append(" " + new string(' ', leftPadding) + columnInfo.Name + new string(' ', rightPadding) + " |");
			}
			stringBuilder.Append('\n');

			// Separator
			stringBuilder.Append("|");
			foreach (int columnWidth in columnWidths)
			{
				stringBuilder.Append(new string('-', columnWidth + 2));
				stringBuilder.Append("|");
			}
			stringBuilder.Append('\n');

			// Content Cells
			foreach (var row in contentRows)
			{
				stringBuilder.Append("|");
				columnIndex = 0;
				foreach (string value in row)
				{
					string text = value ?? "";
					if (columns[columnIndex].RightAlign == TextAlignment.Right)
						stringBuilder.Append(" " + text.PadLeft(columnWidths[columnIndex++], ' ') + " |");
					else
						stringBuilder.Append(" " + text.PadRight(columnWidths[columnIndex++], ' ') + " |");
				}
				stringBuilder.Append('\n');
			}
			stringBuilder.Append(line);
			return stringBuilder.ToString();
		}

		public static bool IsTypeSortable(Type type)
		{
			type = type.GetNonNullableType();
			if (type.IsPrimitive ||
				type.IsEnum ||
				type == typeof(decimal) ||
				type == typeof(string) ||
				type == typeof(DateTime) ||
				type == typeof(TimeSpan))
				return true;

			return false;
		}

		public static bool IsTypeAutoSize(Type type)
		{
			type = type.GetNonNullableType();

			if (type == typeof(string))
				return false;

			if (type.IsPrimitive ||
				type.IsEnum ||
				type == typeof(decimal) ||
				type == typeof(DateTime) ||
				type == typeof(TimeSpan) ||
				typeof(IList).IsAssignableFrom(type))
				return true;

			return false;
		}

		public static TextAlignment GetTextAlignment(Type type)
		{
			type = type.GetNonNullableType();

			if (type == typeof(string))
				return TextAlignment.Left;

			if (type.IsNumeric() ||
				type == typeof(TimeSpan) ||
				typeof(IEnumerable).IsAssignableFrom(type))
			{
				return TextAlignment.Right;
			}

			return TextAlignment.Left;
		}
	}
}
