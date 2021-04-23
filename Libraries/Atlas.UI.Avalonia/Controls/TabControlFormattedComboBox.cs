using Atlas.Extensions;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlFormattedComboBox : ComboBox, IStyleable, ILayoutable
	{
		Type IStyleable.StyleKey => typeof(ComboBox);

		public ListProperty Property;

		private List<FormattedItem> _items;

		public TabControlFormattedComboBox(ListProperty property)
		{
			Property = property;

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Center;

			Bind();
		}

		public void Bind()
		{
			var binding = new Binding(nameof(SelectedFormattedItem))
			{
				Mode = BindingMode.TwoWay,
				Source = this,
			};
			this.Bind(SelectedItemProperty, binding);
		}

		private FormattedItem GetFormattedItem(object obj)
		{
			foreach (FormattedItem item in base.Items)
			{
				if (item.Object.ToString() == obj.ToString())
					return item;
			}
			return null;
		}

		public new IEnumerable Items
		{
			get => base.Items; // todo: return original?
			set
			{
				base.Items = _items = FormattedItem.Create(value);

				SelectPropertyValue();
			}
		}

		private void SelectPropertyValue()
		{
			base.SelectedItem = GetFormattedItem(Property.Value);

			if ((SelectedItem == null) && Items.GetEnumerator().MoveNext())
				SelectedIndex = 0;
		}

		public object SelectedFormattedItem
		{
			set
			{
				if (value is FormattedItem item && item.Object.ToString() != Property.Value.ToString())
					Property.Value = item.Object;
			}
			get => GetFormattedItem(Property.Value);
		}

		public new object SelectedItem
		{
			get
			{
				if (base.SelectedItem is FormattedItem item)
					return item.Object;
				return null;
			}
			set
			{
				FormattedItem formattedItem = GetFormattedItem(value);
				if (formattedItem == null)
				{
					formattedItem = new FormattedItem(value);
					_items.Add(formattedItem);
				}
				base.SelectedItem = formattedItem;
			}
		}
	}

	public class FormattedItem
	{
		public object Object { get; set; }

		public override string ToString() => Object.Formatted();

		public FormattedItem(object obj)
		{
			Object = obj;
		}

		public static List<FormattedItem> Create(IEnumerable items)
		{
			var formattedItems = new List<FormattedItem>();
			foreach (object obj in items)
			{
				formattedItems.Add(new FormattedItem(obj));
			}
			return formattedItems;
		}
	}
}
