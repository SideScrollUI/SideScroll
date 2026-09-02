using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using NUnit.Framework;
using SideScroll.Avalonia.Controls;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SideScroll.Avalonia.Tests;

public class TabFormattedComboBoxTests
{
	private class TestItem : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private string? _text;
		public string? Text
		{
			get => _text;
			set
			{
				_text = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
			}
		}
	}

	private class ReadOnlyTestItem
	{
		public string Text => "A";
	}

	private static TabFormattedComboBox CreateComboBox(TestItem testItem, IList list)
	{
		var property = new ListProperty(testItem, nameof(TestItem.Text));
		return new TabFormattedComboBox(property, list);
	}

	// FormattedItem.Create() doesn't allow null values, so add one to the created list directly
	private static FormattedItem AddNullItem(TabFormattedComboBox comboBox)
	{
		var items = (List<FormattedItem>)comboBox.ItemsSource!;
		FormattedItem nullItem = new(null);
		items.Add(nullItem);
		return nullItem;
	}

	[AvaloniaTest]
	public void SetSelectedItemToNull_WithoutNullItem_ClearsSelection()
	{
		var testItem = new TestItem { Text = "A" };
		TabFormattedComboBox comboBox = CreateComboBox(testItem, new List<string> { "A", "B" });
		var items = (List<FormattedItem>)comboBox.ItemsSource!;
		int originalCount = items.Count;

		comboBox.SelectedItem = null;

		Assert.That(((ComboBox)comboBox).SelectedItem, Is.Null);
		Assert.That(comboBox.SelectedItem, Is.Null);
		Assert.That(items, Has.Count.EqualTo(originalCount),
			"Setting null should not add a new FormattedItem to the list");
	}

	[AvaloniaTest]
	public void SetSelectedItemToNull_WithNullItem_SelectsNullItem()
	{
		var testItem = new TestItem { Text = "A" };
		TabFormattedComboBox comboBox = CreateComboBox(testItem, new List<string> { "A", "B" });
		FormattedItem nullItem = AddNullItem(comboBox);

		comboBox.SelectedItem = null;

		Assert.That(((ComboBox)comboBox).SelectedItem, Is.SameAs(nullItem));
		Assert.That(comboBox.SelectedItem, Is.Null, "The unwrapped selected value should be null");
	}

	[AvaloniaTest]
	public void SetSelectedItem_WithNullItemInList_MatchesNonNullValue()
	{
		// Regression: GetFormattedItem() used to throw a NullReferenceException
		// when comparing against an item with a null Object
		var testItem = new TestItem { Text = "A" };
		TabFormattedComboBox comboBox = CreateComboBox(testItem, new List<string> { "A", "B" });
		AddNullItem(comboBox);
		var items = (List<FormattedItem>)comboBox.ItemsSource!;
		int originalCount = items.Count;

		comboBox.SelectedItem = "B";

		Assert.That(comboBox.SelectedItem, Is.EqualTo("B"));
		Assert.That(items, Has.Count.EqualTo(originalCount),
			"An existing item should be matched instead of adding a duplicate");
	}

	[AvaloniaTest]
	public void SelectedFormattedItem_NullPropertyValue_ReturnsNullItem()
	{
		var testItem = new TestItem { Text = "A" };
		TabFormattedComboBox comboBox = CreateComboBox(testItem, new List<string> { "A", "B" });
		FormattedItem nullItem = AddNullItem(comboBox);

		testItem.Text = null;

		Assert.That(comboBox.SelectedFormattedItem, Is.SameAs(nullItem));
	}

	[AvaloniaTest]
	public void PropertyChangedToNull_WithNullItem_SelectsNullItem()
	{
		var testItem = new TestItem { Text = "A" };
		TabFormattedComboBox comboBox = CreateComboBox(testItem, new List<string> { "A", "B" });
		FormattedItem nullItem = AddNullItem(comboBox);

		testItem.Text = null;

		Assert.That(((ComboBox)comboBox).SelectedItem, Is.SameAs(nullItem));
	}

	[AvaloniaTest]
	public void PropertyChangedToNull_WithoutNullItem_ClearsSelection()
	{
		var testItem = new TestItem { Text = "A" };
		TabFormattedComboBox comboBox = CreateComboBox(testItem, new List<string> { "A", "B" });

		testItem.Text = null;

		Assert.That(((ComboBox)comboBox).SelectedItem, Is.Null);
		Assert.That(comboBox.SelectedItem, Is.Null);
	}

	[AvaloniaTest]
	public void FixedListDisablesEditingForReadOnlyProperty()
	{
		var testItem = new ReadOnlyTestItem();
		var property = new ListProperty(testItem, nameof(ReadOnlyTestItem.Text));

		var comboBox = new TabFormattedComboBox(property, new List<string> { "A", "B" });

		Assert.That(property.IsEditable, Is.False);
		Assert.That(comboBox.IsEnabled, Is.False);
	}

	// ─── Lifetime ────────────────────────────────────────────────────────

	// Kept out of the test method so the combo box has no local still referencing it
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static WeakReference CreateAndRelease(TestItem testItem, bool dispose)
	{
		TabFormattedComboBox comboBox = CreateComboBox(testItem, new List<string> { "A", "B" });
		if (dispose)
		{
			comboBox.Dispose();
		}
		return new WeakReference(comboBox);
	}

	[AvaloniaTest]
	[TestCase(true, false, TestName = "Disposed combo box is collected")]
	[TestCase(false, true, TestName = "Undisposed combo box is held by the bound object")]
	[NUnit.Framework.Description(
		"The bound object's PropertyChanged holds the combo box, so it outlives the control until " +
		"Dispose() unsubscribes. The undisposed case proves the collection check can actually fail")]
	public void DisposeReleasesBoundObject(bool dispose, bool expectedAlive)
	{
		var testItem = new TestItem { Text = "A" };

		WeakReference reference = CreateAndRelease(testItem, dispose);

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		Assert.That(reference.IsAlive, Is.EqualTo(expectedAlive));

		// The bound object has to stay reachable, it's what would be holding the combo box
		GC.KeepAlive(testItem);
	}
}
