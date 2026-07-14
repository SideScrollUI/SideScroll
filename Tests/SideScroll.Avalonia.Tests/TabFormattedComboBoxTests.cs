using Avalonia.Controls;
using NUnit.Framework;
using SideScroll.Avalonia.Controls;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.ComponentModel;

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

	[Test]
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

	[Test]
	public void SetSelectedItemToNull_WithNullItem_SelectsNullItem()
	{
		var testItem = new TestItem { Text = "A" };
		TabFormattedComboBox comboBox = CreateComboBox(testItem, new List<string> { "A", "B" });
		FormattedItem nullItem = AddNullItem(comboBox);

		comboBox.SelectedItem = null;

		Assert.That(((ComboBox)comboBox).SelectedItem, Is.SameAs(nullItem));
		Assert.That(comboBox.SelectedItem, Is.Null, "The unwrapped selected value should be null");
	}

	[Test]
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

	[Test]
	public void SelectedFormattedItem_NullPropertyValue_ReturnsNullItem()
	{
		var testItem = new TestItem { Text = "A" };
		TabFormattedComboBox comboBox = CreateComboBox(testItem, new List<string> { "A", "B" });
		FormattedItem nullItem = AddNullItem(comboBox);

		testItem.Text = null;

		Assert.That(comboBox.SelectedFormattedItem, Is.SameAs(nullItem));
	}

	[Test]
	public void PropertyChangedToNull_WithNullItem_SelectsNullItem()
	{
		var testItem = new TestItem { Text = "A" };
		TabFormattedComboBox comboBox = CreateComboBox(testItem, new List<string> { "A", "B" });
		FormattedItem nullItem = AddNullItem(comboBox);

		testItem.Text = null;

		Assert.That(((ComboBox)comboBox).SelectedItem, Is.SameAs(nullItem));
	}

	[Test]
	public void PropertyChangedToNull_WithoutNullItem_ClearsSelection()
	{
		var testItem = new TestItem { Text = "A" };
		TabFormattedComboBox comboBox = CreateComboBox(testItem, new List<string> { "A", "B" });

		testItem.Text = null;

		Assert.That(((ComboBox)comboBox).SelectedItem, Is.Null);
		Assert.That(comboBox.SelectedItem, Is.Null);
	}
}
