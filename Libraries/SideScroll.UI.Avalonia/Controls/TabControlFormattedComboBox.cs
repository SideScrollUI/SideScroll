using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace SideScroll.UI.Avalonia.Controls;

public class TabControlFormattedComboBox : ComboBox
{
	protected override Type StyleKeyOverride => typeof(ComboBox);

	public ListProperty Property;

	private List<FormattedItem>? _items;

	public TabControlFormattedComboBox(ListProperty property, IList list)
	{
		Property = property;
		Items = list;

		InitializeComponent();
	}

	public TabControlFormattedComboBox(ListProperty property, string? listPropertyName)
	{
		Property = property;
		IsEnabled = property.Editable;

		InitializeComponent();

		if (listPropertyName != null)
		{
			PropertyInfo? propertyInfo = property.Object.GetType().GetProperty(listPropertyName,
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.Static |
				BindingFlags.FlattenHierarchy);

			ArgumentNullException.ThrowIfNull(propertyInfo);

			Items = (IEnumerable)propertyInfo.GetValue(property.Object)!;
		}
		else
		{
			Items = property.UnderlyingType.GetEnumValues();
		}
		Bind();
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
		Bind(SelectedItemProperty, binding);

		if (Property.Object is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
		}
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == Property?.PropertyInfo.Name)
		{
			SelectedItem = Property!.Value;
		}
	}

	private FormattedItem? GetFormattedItem(object? obj)
	{
		if (obj == null) return null;

		foreach (FormattedItem? item in base.Items)
		{
			if (item?.Object!.ToString() == obj.ToString())
				return item;
		}
		return null;
	}

	public new IEnumerable Items
	{
		get => base.Items; // todo: return original?
		set
		{
			ItemsSource = _items = FormattedItem.Create(value);

			SelectPropertyValue();
		}
	}

	private void SelectPropertyValue()
	{
		base.SelectedItem = GetFormattedItem(Property.Value);

		if (SelectedItem == null && Items.GetEnumerator().MoveNext())
		{
			SelectedIndex = 0;
		}
	}

	public object? SelectedFormattedItem
	{
		set
		{
			if (value is FormattedItem item && item.Object?.ToString() != Property.Value?.ToString())
			{
				Property.Value = item.Object;
			}
		}
		get => GetFormattedItem(Property.Value);
	}

	public new object? SelectedItem
	{
		get
		{
			if (base.SelectedItem is FormattedItem item)
				return item.Object;
			return null;
		}
		set
		{
			FormattedItem? formattedItem = GetFormattedItem(value);
			if (formattedItem == null)
			{
				formattedItem = new FormattedItem(value);
				_items!.Add(formattedItem);
			}
			base.SelectedItem = formattedItem;
		}
	}
}

public class FormattedItem(object? obj)
{
	public object? Object { get; set; } = obj;

	public override string? ToString() => Object.Formatted();

	public static List<FormattedItem> Create(IEnumerable items)
	{
		return items.Cast<object>()
			.DistinctBy(obj => obj.ToString())
			.Select(obj => new FormattedItem(obj))
			.ToList();
	}
}
