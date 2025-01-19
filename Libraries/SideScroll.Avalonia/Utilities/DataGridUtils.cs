using Avalonia.Media;
using SideScroll.Extensions;
using System.Collections;
using System.Text;

namespace SideScroll.Avalonia.Utilities;

public static class DataGridUtils
{
	public static int MaxColumnWidth { get; set; } = 100;

	public class ColumnInfo(string name)
	{
		public string Name { get; set; } = name;
		public TextAlignment RightAlign { get; set; }
	}

	public static string TableToString(List<ColumnInfo> columns, List<List<string>> contentRows, int? maxColumnWidth = null)
	{
		List<int> columnNameWidths = columns
			.Select(c => c.Name.Length)
			.ToList();

		List<List<string>> cellValues = GetCellValues(contentRows, maxColumnWidth ?? MaxColumnWidth, columnNameWidths);

		return TableValuesToString(columns, columnNameWidths, cellValues);
	}

	private static List<List<string>> GetCellValues(List<List<string>> contentRows, int maxColumnWidth, List<int> columnNameWidths)
	{
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
						{
							text = text[..position];
						}
					}

					string remaining = value[text.Length..];
					rowValues[column] = remaining;
					lineValues.Add(text);
					overflowed |= (remaining.Length > 0);
					columnNameWidths[column] = Math.Max(text.Length, columnNameWidths[column]);
				}

				cellValues.Add(lineValues);
				if (!overflowed)
					break;
			}
		}

		return cellValues;
	}

	private static string TableValuesToString(List<ColumnInfo> columns, List<int> columnNameWidths, List<List<string>> cellValues)
	{
		string line = "-";
		foreach (int value in columnNameWidths)
		{
			line += new string('-', value + 3);
		}
		line += '\n';

		var sb = new StringBuilder(line);

		// Column Headers
		sb.Append('|');
		int columnIndex = 0;
		foreach (var columnInfo in columns)
		{
			int columnWidth = columnNameWidths[columnIndex++];
			int leftPadding = (columnWidth - columnInfo.Name.Length) / 2;
			int rightPadding = columnWidth - columnInfo.Name.Length - leftPadding;
			sb.Append(" " + new string(' ', leftPadding) + columnInfo.Name + new string(' ', rightPadding) + " |");
		}
		sb.Append('\n');

		// Separator
		sb.Append('|');
		foreach (int columnWidth in columnNameWidths)
		{
			sb.Append(new string('-', columnWidth + 2));
			sb.Append('|');
		}
		sb.Append('\n');

		// Content Cells
		foreach (var row in cellValues)
		{
			sb.Append('|');
			columnIndex = 0;
			foreach (string value in row)
			{
				if (columns[columnIndex].RightAlign == TextAlignment.Right)
				{
					sb.Append(" " + value.PadLeft(columnNameWidths[columnIndex++], ' ') + " |");
				}
				else
				{
					sb.Append(" " + value.PadRight(columnNameWidths[columnIndex++], ' ') + " |");
				}
			}
			sb.Append('\n');
		}
		sb.Append(line);
		return sb.ToString();
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
