using SideScroll.Tasks;
using System.Collections;

namespace SideScroll.Tabs.Toolbar;

public interface IToolComboBox
{
	string Label { get; }
	object? SelectedObject { get; }

	IList GetItems();
}

public class ToolComboBox<T> : IToolComboBox
{
	public string Label { get; }
	public List<T> Items { get; }
	public T? SelectedItem { get; set; }

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

	public CallAction? Action { get; set; }
	public CallActionAsync? ActionAsync { get; set; }

	public ToolComboBox(string label, List<T> items, T selectedItem, CallAction? action = null)
	{
		Label = label;
		Items = items;
		SelectedItem = selectedItem;
		Action = action;
	}

	public ToolComboBox(string label, List<T> items, CallActionAsync actionAsync)
	{
		Label = label;
		Items = items;
		ActionAsync = actionAsync;
	}

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
