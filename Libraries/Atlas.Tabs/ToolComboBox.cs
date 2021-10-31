using Atlas.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Atlas.Tabs
{
	public interface IToolComboBox
	{
		string Label { get; }
		object SelectedObject { get; }

		IList GetItems();
	}

	public class ToolComboBox<T> : IToolComboBox
	{
		public string Label { get; set; }
		public List<T> Items { get; set; }
		public T SelectedItem { get; set; }

		public object SelectedObject
		{
			get => SelectedItem;
			set
			{
				SelectedItem = (T)value;

				var call = new Call();

				if (Action != null)
					Action.Invoke(call);

				if (ActionAsync != null)
					ActionAsync.Invoke(call);
			}
		}

		public TaskDelegate.CallAction Action { get; set; }
		public TaskDelegateAsync.CallActionAsync ActionAsync { get; set; }

		public ToolComboBox(string label, List<T> items, T selectedItem, TaskDelegate.CallAction action = null)
		{
			Label = label;
			Items = items;
			SelectedItem = selectedItem;
			Action = action;
		}

		public ToolComboBox(string label, List<T> items, TaskDelegateAsync.CallActionAsync actionAsync)
		{
			Label = label;
			Items = items;
			ActionAsync = actionAsync;
		}

		public IList GetItems()
		{
			if (Items.Contains(SelectedItem))
				return Items;

			return new List<T>(Items)
			{
				SelectedItem
			};
		}
	}
}
