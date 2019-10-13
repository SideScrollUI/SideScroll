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

namespace Atlas.GUI.Avalonia
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
					string value = obj.ObjectToString();
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

		public static string DataGridToStringTable(DataGrid dataGrid)
		{
			if (dataGrid == null)
				return null;

			List<string> columns = new List<string>();
			List<List<string>> contentRows = new List<List<string>>();
			Dictionary<int, DataGridColumn> visibleColumns = new Dictionary<int, DataGridColumn>();

			foreach (DataGridColumn dataColumn in dataGrid.Columns)
			{
				if (dataColumn.IsVisible)
					visibleColumns[dataColumn.DisplayIndex] = dataColumn;
			}

			foreach (DataGridColumn dataColumn in visibleColumns.Values)
			{
				columns.Add((string)dataColumn.Header);
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

						string value = obj.ObjectToString();
						value = value?.Replace('\n', ' '); // remove newlines
						stringCells.Add(value);
					}
					//object content = dataColumn.GetCellValue(item, dataColumn.ClipboardContentBinding);
				}

				contentRows.Add(stringCells);
			}

			string text = TableToString(columns, contentRows);
			return text;
		}

		public static string TableToString(List<string> columns, List<List<string>> contentRows)
		{
			List<int> columnWidths = new List<int>();
			for (int column = 0; column < columns.Count; column++)
			{
				string header = columns[column];
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
			foreach (string value in columns)
			{
				int columnWidth = columnWidths[columnIndex++];
				int leftPadding = (columnWidth - value.Length) / 2;
				int rightPadding = columnWidth - value.Length - leftPadding;
				stringBuilder.Append(" " + new string(' ', leftPadding) + value + new string(' ', rightPadding) + " |");
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

		public static TextAlignment GetTextAlignment(Type type)
		{
			type = type.GetNonNullableType();

			if (type.IsNumeric() ||
				type == typeof(TimeSpan) ||
				typeof(ICollection).IsAssignableFrom(type))
			{
				return TextAlignment.Right;
			}

			return TextAlignment.Left;
		}
	}
}
