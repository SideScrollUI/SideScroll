using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Atlas.Tabs;

public interface IPropertyEditable
{
	bool Editable { get; }
}

public class ListProperty : ListMember, IPropertyEditable
{
	public readonly PropertyInfo PropertyInfo;
	public bool Cachable;

	private bool _valueCached;
	private object _valueObject;

	[HiddenColumn]
	public override bool Editable // rename to IsReadOnly?
	{
		get
		{
			bool propertyReadOnly = (PropertyInfo.GetCustomAttribute<ReadOnlyAttribute>() != null);
			return PropertyInfo.CanWrite && !propertyReadOnly;
		}
	}

	[Hidden]
	public bool IsFormatted => (PropertyInfo.GetCustomAttribute<FormattedAttribute>() != null);

	[Editing, InnerValue, WordWrap]
	public override object Value
	{
		get
		{
			try
			{
				if (Cachable)
				{
					if (!_valueCached)
					{
						_valueCached = true;
						_valueObject = PropertyInfo.GetValue(Object);

						if (IsFormatted)
							_valueObject = _valueObject.Formatted();
					}
					return _valueObject;
				}
				return PropertyInfo.GetValue(Object);
			}
			catch (Exception)
			{
				return null;
			}
		}
		set
		{
			if (PropertyInfo.CanWrite)
			{
				Type type = PropertyInfo.PropertyType;
				if (value != null)
				{
					type = type.GetNonNullableType();
				}

				if (!type.IsInstanceOfType(value))
				{
					value = Convert.ChangeType(value, type);
				}

				PropertyInfo.SetValue(Object, value);

				/*if (Object is INotifyPropertyChanged notifyPropertyChanged)
				{
					//notifyPropertyChanged.PropertyChanged?.Invoke(obj, new PropertyChangedEventArgs(propertyName));
				}*/
			}
		}
	}

	[Hidden]
	public Type UnderlyingType => PropertyInfo.PropertyType.GetNonNullableType();

	public override string ToString() => Name;

	public ListProperty(object obj, PropertyInfo propertyInfo, bool cachable = true) :
		base(obj, propertyInfo)
	{
		PropertyInfo = propertyInfo;
		Cachable = cachable;

		var accessors = propertyInfo.GetAccessors(true);
		AutoLoad = !accessors[0].IsStatic;

		NameAttribute attribute = propertyInfo.GetCustomAttribute<NameAttribute>();

		Name = attribute?.Name ?? propertyInfo.Name.WordSpaced();

		if (PropertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
			Name = "* " + Name;
	}

	public static new ItemCollection<ListProperty> Create(object obj, bool includeBaseTypes = true)
	{
		// this doesn't work for virtual methods (or any method modifier?)
		var propertyInfos = obj.GetType().GetProperties()
			.Where(p => IsVisible(p))
			.Where(p => includeBaseTypes || p.DeclaringType == obj.GetType())
			.OrderBy(p => p.MetadataToken);

		var listProperties = new ItemCollection<ListProperty>();
		var propertyToIndex = new Dictionary<string, int>();
		foreach (PropertyInfo propertyInfo in propertyInfos)
		{
			var listProperty = new ListProperty(obj, propertyInfo);
			if (!listProperty.IsObjectVisible())
				continue;

			if (propertyToIndex.TryGetValue(propertyInfo.Name, out int index))
			{
				listProperties.RemoveAt(index);
				listProperties.Insert(index, listProperty);
			}
			else
			{
				propertyToIndex[propertyInfo.Name] = listProperties.Count;
				listProperties.Add(listProperty);
			}
		}
		return listProperties;
	}

	public static bool IsVisible(PropertyInfo propertyInfo)
	{
		if (propertyInfo.DeclaringType.IsNotPublic)
			return false;

#if !DEBUG
			if (propertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return propertyInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			propertyInfo.GetCustomAttribute<HiddenRowAttribute>() == null; // [HiddenRow]
	}

	public bool IsObjectVisible()
	{
		if (PropertyInfo.GetCustomAttribute<HideNullAttribute>() != null)
		{
			if (Value == null)
				return false;
		}

		var hideAttribute = PropertyInfo.GetCustomAttribute<HideAttribute>();
		if (hideAttribute?.Values != null)
		{
			return !hideAttribute.Values.Any(v => ObjectUtils.IsEqual(Value, v));
		}
		return true;
	}

	// This can be slow due to lazy property loading
	public static ItemCollection<ListProperty> Sort(ItemCollection<ListProperty> listProperties)
	{
		var sortedProperties = listProperties
			.OrderByDescending(i => i.PropertyInfo.GetCustomAttribute<AutoSelectAttribute>() != null)
			.ThenByDescending(i => TabUtils.ObjectHasLinks(i, true));

		var linkSorted = new ItemCollection<ListProperty>(sortedProperties);
		return linkSorted;
	}
}

/*
What do we do about child objects?
	can't edit child objects

	DataBinding

This class only works alone, no fields
Just expand these properties only
*/
