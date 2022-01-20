using Atlas.Extensions;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Atlas.UI.Avalonia;

public class DataGridUtils
{
	private const int MaxColumnWidth = 100;

	public class ColumnInfo
	{
		public string Name { get; set; }
		public TextAlignment RightAlign { get; set; }

		public ColumnInfo(string name)
		{
			Name = name;
		}
	}

	public static string TableToString(List<ColumnInfo> columns, List<List<string>> contentRows, int maxColumnWidth = MaxColumnWidth)
	{
		var columnWidths = new List<int>();
		for (int column = 0; column < columns.Count; column++)
		{
			string header = columns[column].Name;
			columnWidths.Add(header.Length);
		}

		// Get formatted cell values and wrap text across multiple lines
		var cellValues = new List<List<string>>();
		foreach (List<string> row in contentRows)
		{
			var rowValues = new List<string>();
			foreach (string value in row)
			{
				string text = value?
					.Replace("\n", " ")
					.Replace("\r", "")
					.Replace("\t", "    ")
					?? "";
				rowValues.Add(text);
			}

			while (true)
			{
				bool overflowed = false;
				var lineValues = new List<string>();
				for (int column = 0; column < rowValues.Count; column++)
				{
					string value = rowValues[column];
					string text = value;
					if (text.Length > maxColumnWidth)
					{
						text = text[..maxColumnWidth];
						int position = text.LastIndexOf(' ');
						if (position > 0)
							text = text[..position];
					}

					string remaining = value[text.Length..];
					rowValues[column] = remaining;
					lineValues.Add(text);
					overflowed |= (remaining.Length > 0);
					columnWidths[column] = Math.Max(text.Length, columnWidths[column]);
				}

				cellValues.Add(lineValues);
				if (!overflowed)
					break;
			}
		}

		string line = "-";
		foreach (int value in columnWidths)
		{
			line += new string('-', value + 3);
		}
		line += '\n';

		var stringBuilder = new StringBuilder(line);

		// Column Headers
		stringBuilder.Append('|');
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
		stringBuilder.Append('|');
		foreach (int columnWidth in columnWidths)
		{
			stringBuilder.Append(new string('-', columnWidth + 2));
			stringBuilder.Append('|');
		}
		stringBuilder.Append('\n');

		// Content Cells
		foreach (var row in cellValues)
		{
			stringBuilder.Append('|');
			columnIndex = 0;
			foreach (string value in row)
			{
				if (columns[columnIndex].RightAlign == TextAlignment.Right)
					stringBuilder.Append(" " + value.PadLeft(columnWidths[columnIndex++], ' ') + " |");
				else
					stringBuilder.Append(" " + value.PadRight(columnWidths[columnIndex++], ' ') + " |");
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
