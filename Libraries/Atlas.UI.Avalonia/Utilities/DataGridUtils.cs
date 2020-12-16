using Atlas.Extensions;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Atlas.UI.Avalonia
{
	public class DataGridUtils
	{
		// private const int MaxColumnWidth = 100;

		public class ColumnInfo
		{
			public string Name { get; set; }
			public TextAlignment RightAlign { get; set; }

			public ColumnInfo(string name)
			{
				Name = name;
			}
		}

		public static string TableToString(List<ColumnInfo> columns, List<List<string>> contentRows)
		{
			var columnWidths = new List<int>();
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

			var stringBuilder = new StringBuilder(line);

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
					string text = value?.Replace("\n", "").Replace("\r", "") ?? "";
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
