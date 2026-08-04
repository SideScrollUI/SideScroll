using NUnit.Framework;
using static SideScroll.Avalonia.Utilities.TableUtils;

namespace SideScroll.Avalonia.Tests;

public class TableUtilsTests
{
	private static List<ColumnInfo> Columns(params string[] names) =>
		[.. names.Select(n => new ColumnInfo(n))];

	[Test, Description(
		"Headers used to be written raw while cells were quoted, so a comma in a column name " +
		"split it into two columns and shifted every column after it")]
	public void TableToCsvEscapesHeaders()
	{
		List<ColumnInfo> columns = Columns("Name, Full", "Value");
		List<List<string>> rows = [["a", "b"]];

		string csv = TableToCsv(columns, rows);

		Assert.That(csv, Is.EqualTo("\"Name, Full\",\"Value\"\n\"a\",\"b\"\n"));
	}

	[Test, Description("A quote in a header is doubled the same way it already was in cells")]
	public void TableToCsvEscapesQuotesInHeaders()
	{
		string csv = TableToCsv(Columns("He said \"hi\""), [["x"]]);

		Assert.That(csv, Is.EqualTo("\"He said \"\"hi\"\"\"\n\"x\"\n"));
	}

	[Test, Description("Cell escaping is unchanged")]
	public void TableToCsvEscapesCells()
	{
		string csv = TableToCsv(Columns("A", "B"), [["has, comma", "has \" quote"]]);

		Assert.That(csv, Is.EqualTo("\"A\",\"B\"\n\"has, comma\",\"has \"\" quote\"\n"));
	}

	[Test]
	public void TableToCsvWithNoRowsStillWritesHeaders()
	{
		Assert.That(TableToCsv(Columns("A", "B"), []), Is.EqualTo("\"A\",\"B\"\n"));
	}

	[TestCase(0)]
	[TestCase(-1)]
	public void TableToStringRejectsNonPositiveColumnWidth(int maxColumnWidth)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			TableToString(Columns("A"), [["value"]], maxColumnWidth));
	}
}
