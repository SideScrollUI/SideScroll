using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using NUnit.Framework;
using SideScroll.Avalonia.Controls.DataGrids;
using SideScroll.Collections;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Tests;

/// <summary>
/// A button column with a <see cref="DataGridButtonColumn.Confirmation"/> prompts before running
/// its action, which the delete column uses for <see cref="IDeletableList.DeleteConfirmation"/>.
/// The flyout is shared by every row, so which row is acted on is tracked separately from it
/// </summary>
public class DataGridButtonColumnTests
{
	private class Item(string name)
	{
		public string Name { get; set; } = name;

		public override string ToString() => Name;
	}

	private readonly List<Item> _invoked = [];

	private Window _window = null!;
	private DataGrid _dataGrid = null!;
	private ItemCollectionUI<Item> _items = null!;

	[SetUp]
	public void SetUp()
	{
		_invoked.Clear();
	}

	private DataGridButtonColumn CreateGrid(ConfirmationFlyoutConfig? confirmation)
	{
		_items = new ItemCollectionUI<Item>([new Item("a"), new Item("b"), new Item("c")]);

		var column = new DataGridButtonColumn("-", obj => _invoked.Add((Item)obj))
		{
			Confirmation = confirmation,
		};

		_dataGrid = new DataGrid
		{
			ItemsSource = _items,
			AutoGenerateColumns = false,
			IsReadOnly = false,
		};
		_dataGrid.Columns.Add(new DataGridTextColumn { Binding = new Binding(nameof(Item.Name)) });
		_dataGrid.Columns.Add(column);

		_window = new Window
		{
			Width = 400,
			Height = 300,
			Content = _dataGrid,
		};
		HeadlessWindow.ShowAndSettle(_window);

		return column;
	}

	// The cell buttons are generated in row order
	private Button RowButton(int index)
	{
		return _dataGrid.GetVisualDescendants()
			.OfType<DataGridButton>()
			.First(button => ReferenceEquals(button.DataContext, _items[index]));
	}

	// The flyout renders into the window's overlay layer, so its buttons are only there while open
	private Button? PromptButton(string text)
	{
		return OverlayLayer.GetOverlayLayer(_window)?
			.GetVisualDescendants()
			.OfType<Button>()
			.FirstOrDefault(button => Equals(button.Content, text));
	}

	private void Click(Button button)
	{
		button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
		HeadlessWindow.Settle(_window);
	}

	[AvaloniaTest, Description("Control: without a confirmation the click action still runs immediately")]
	public void ClickingWithoutAConfirmationInvokesTheAction()
	{
		CreateGrid(null);

		Click(RowButton(1));

		Assert.That(_invoked, Is.EqualTo(new[] { _items[1] }));
	}

	[AvaloniaTest, Description("The prompt is shown instead of the action running, which is the point of it")]
	public void ClickingWithAConfirmationPromptsInsteadOfInvoking()
	{
		CreateGrid(new ConfirmationFlyoutConfig("Delete this item?", "Delete"));

		Click(RowButton(1));

		Assert.That(_invoked, Is.Empty);
		Assert.That(PromptButton("Delete"), Is.Not.Null, "the prompt is showing");
	}

	[AvaloniaTest, Description("Confirming runs the action for the row whose button was clicked")]
	public void ConfirmingInvokesTheClickedRow()
	{
		CreateGrid(new ConfirmationFlyoutConfig("Delete this item?", "Delete"));

		Click(RowButton(1));
		Click(PromptButton("Delete")!);

		Assert.That(_invoked, Is.EqualTo(new[] { _items[1] }));
	}

	[AvaloniaTest, Description("Cancelling leaves the row alone")]
	public void CancellingInvokesNothing()
	{
		CreateGrid(new ConfirmationFlyoutConfig("Delete this item?", "Delete"));

		Click(RowButton(1));
		Click(PromptButton("Cancel")!);

		Assert.That(_invoked, Is.Empty);
	}

	[AvaloniaTest, Description(
		"The row is captured when clicked, the flyout is shared by every row so the cell it's " +
		"anchored to can be recycled for another row before it's confirmed")]
	public void ConfirmingInvokesTheRowClicked_NotWhateverTheCellHoldsNow()
	{
		CreateGrid(new ConfirmationFlyoutConfig("Delete this item?", "Delete"));

		Button button = RowButton(1);
		Click(button);

		// What scrolling a recycled cell to another row does
		var recycled = new Item("recycled");
		button.DataContext = recycled;
		HeadlessWindow.Settle(_window);

		Click(PromptButton("Delete")!);

		Assert.That(_invoked, Is.EqualTo(new[] { _items[1] }));
	}

	[AvaloniaTest, Description("A second row's prompt replaces the first rather than confirming both")]
	public void PromptingASecondRowInvokesOnlyThatRow()
	{
		CreateGrid(new ConfirmationFlyoutConfig("Delete this item?", "Delete"));

		Click(RowButton(0));
		Click(RowButton(2));
		Click(PromptButton("Delete")!);

		Assert.That(_invoked, Is.EqualTo(new[] { _items[2] }));
	}

	[AvaloniaTest, Description("Confirming twice needs a second click on the row, the prompt doesn't stay armed")]
	public void ConfirmingDoesNotInvokeAgainWithoutAnotherClick()
	{
		CreateGrid(new ConfirmationFlyoutConfig("Delete this item?", "Delete"));

		Button rowButton = RowButton(1);
		Click(rowButton);
		Button confirm = PromptButton("Delete")!;
		Click(confirm);
		Click(confirm);

		Assert.That(_invoked, Is.EqualTo(new[] { _items[1] }));
	}
}
