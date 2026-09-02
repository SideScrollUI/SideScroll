using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.NUnit;
using NUnit.Framework;
using SideScroll.Avalonia.Extensions;

namespace SideScroll.Avalonia.Tests;

public class DataGridExtensionsTests
{
	private class Row
	{
		public string A { get; set; } = "a1";
		public string B { get; set; } = "b1";
		public string C { get; set; } = "c1";
	}

	private static DataGrid CreateGrid()
	{
		var dataGrid = new DataGrid();
		foreach (string name in new[] { "A", "B", "C" })
		{
			dataGrid.Columns.Add(new DataGridTextColumn
			{
				Header = name,
				Binding = new Binding(name),
			});
		}
		dataGrid.ItemsSource = new List<Row> { new() };
		return dataGrid;
	}

	[AvaloniaTest, Description(
		"DataGrid.Columns keeps its insertion order and DisplayIndex is the only record of what's " +
		"on screen, so exporting through a plain Dictionary kept the original order after a move")]
	public void ToCsvFollowsTheDisplayOrder()
	{
		DataGrid dataGrid = CreateGrid();

		dataGrid.Columns[2].DisplayIndex = 0;

		Assert.That(dataGrid.ToCsv(), Is.EqualTo("\"C\",\"A\",\"B\"\n\"c1\",\"a1\",\"b1\"\n"));
	}

	[AvaloniaTest, Description("The string table follows the display order the same way")]
	public void ToStringTableFollowsTheDisplayOrder()
	{
		DataGrid dataGrid = CreateGrid();

		dataGrid.Columns[2].DisplayIndex = 0;

		Assert.That(dataGrid.ToStringTable(), Does.Contain("| C  | A  | B  |"));
	}

	[AvaloniaTest, Description("Control: an unmoved grid exports in its column order")]
	public void ToCsvUsesTheColumnOrderWhenNothingMoved()
	{
		DataGrid dataGrid = CreateGrid();

		Assert.That(dataGrid.ToCsv(), Is.EqualTo("\"A\",\"B\",\"C\"\n\"a1\",\"b1\",\"c1\"\n"));
	}

	[AvaloniaTest, Description("Control: hidden columns stay out of the export")]
	public void ToCsvSkipsHiddenColumns()
	{
		DataGrid dataGrid = CreateGrid();

		dataGrid.Columns[1].IsVisible = false;

		Assert.That(dataGrid.ToCsv(), Is.EqualTo("\"A\",\"C\"\n\"a1\",\"c1\"\n"));
	}

	[AvaloniaTest]
	[TestCase(-1)]
	[TestCase(int.MinValue)]
	[NonParallelizable] // MaxValueLength is static
	[Description("A negative maximum reaches Formatted() for every exported cell, where it throws")]
	public void RejectsNegativeMaxValueLength(int value)
	{
		int original = DataGridExtensions.MaxValueLength;
		try
		{
			ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
				() => DataGridExtensions.MaxValueLength = value)!;

			Assert.That(exception.ParamName, Is.EqualTo(nameof(DataGridExtensions.MaxValueLength)));
			Assert.That(DataGridExtensions.MaxValueLength, Is.EqualTo(original), "The rejected value isn't stored.");
			Assert.That(CreateGrid().ToCsv(), Is.EqualTo("\"A\",\"B\",\"C\"\n\"a1\",\"b1\",\"c1\"\n"));
		}
		finally
		{
			DataGridExtensions.MaxValueLength = original;
		}
	}

	[AvaloniaTest, NonParallelizable, Description("Control: zero exports empty cells rather than being invalid")]
	public void AllowsZeroMaxValueLength()
	{
		int original = DataGridExtensions.MaxValueLength;
		try
		{
			DataGridExtensions.MaxValueLength = 0;

			Assert.That(CreateGrid().ToCsv(), Is.EqualTo("\"A\",\"B\",\"C\"\n\"\",\"\",\"\"\n"));
		}
		finally
		{
			DataGridExtensions.MaxValueLength = original;
		}
	}
}
