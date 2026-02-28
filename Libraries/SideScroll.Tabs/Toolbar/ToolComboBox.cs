using SideScroll.Tasks;
using System.Collections;

namespace SideScroll.Tabs.Toolbar;

/// <summary>
/// Interface for toolbar combo boxes
/// </summary>
public interface IToolComboBox
{
	/// <summary>
	/// The label displayed for the combo box
	/// </summary>
	string Label { get; }

	/// <summary>
	/// The currently selected item as an object
	/// </summary>
	object? SelectedObject { get; }

	/// <summary>
	/// Gets the list of items including the selected item if not already in the list
	/// </summary>
	IList GetItems();
}

/// <summary>
/// Represents a toolbar combo box with typed items and selection changed actions
/// </summary>
public class ToolComboBox<T> : IToolComboBox
{
	/// <summary>
	/// The label displayed for the combo box
	/// </summary>
	public string Label { get; }

	/// <summary>
	/// The list of items to display in the combo box
	/// </summary>
	public List<T> Items { get; }

	/// <summary>
	/// The currently selected item
	/// </summary>
	public T? SelectedItem { get; set; }

	/// <summary>
	/// The currently selected item as an object. Setting this invokes the associated action
	/// </summary>
	public object? SelectedObject
	{
		get => SelectedItem;
		set
		{
			SelectedItem = (T?)value;

			var call = new Call();

			Action?.Invoke(call);
			ActionAsync?.Invoke(call);
		}
	}

	/// <summary>
	/// The synchronous action to execute when the selection changes
	/// </summary>
	public CallAction? Action { get; set; }

	/// <summary>
	/// The asynchronous action to execute when the selection changes
	/// </summary>
	public CallActionAsync? ActionAsync { get; set; }

	/// <summary>
	/// Initializes a new toolbar combo box with a synchronous action
	/// </summary>
	public ToolComboBox(string label, List<T> items, T? selectedItem = default, CallAction? action = null)
	{
		Label = label;
		Items = items;
		SelectedItem = selectedItem;
		Action = action;
	}

	/// <summary>
	/// Initializes a new toolbar combo box with an asynchronous action
	/// </summary>
	public ToolComboBox(string label, List<T> items, CallActionAsync actionAsync)
	{
		Label = label;
		Items = items;
		ActionAsync = actionAsync;
	}

	/// <summary>
	/// Gets the list of items, including the selected item if not already in the list
	/// </summary>
	public IList GetItems()
	{
		if (Items.Contains(SelectedItem!))
			return Items;

		return new List<T>(Items)
		{
			SelectedItem!
		};
	}
}
