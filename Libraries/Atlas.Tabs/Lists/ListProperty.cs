using Atlas.Core;
using Atlas.Extensions;
using System.ComponentModel;
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
	private object? _valueObject;

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
	public override object? Value
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

					if (!type.IsInstanceOfType(value))
					{
						value = Convert.ChangeType(value, type);
					}
				}

				PropertyInfo.SetValue(Object, value);
				_valueCached = false;
				ValueChanged();
			}
		}
	}

	[Hidden]
	public Type UnderlyingType => PropertyInfo.PropertyType.GetNonNullableType();

	[Hidden]
	public bool IsPropertyVisible => PropertyInfo.IsRowVisible();

	public override string? ToString() => Name;

	public ListProperty(object obj, PropertyInfo propertyInfo, bool cachable = true) :
		base(obj, propertyInfo)
	{
		PropertyInfo = propertyInfo;
		Cachable = cachable;

		// [ListItem] uses static properties, remove?
		// var accessors = propertyInfo.GetAccessors(true);
		// AutoLoad = !accessors[0].IsStatic;

		NameAttribute? nameAttribute = propertyInfo.GetCustomAttribute<NameAttribute>();

		Name = nameAttribute?.Name ?? propertyInfo.Name.WordSpaced();

		if (PropertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
			Name = "* " + Name;

		if (obj is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged += ListProperty_PropertyChanged;
		}
	}

	protected void ListProperty_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != MemberInfo.Name) return;
		
		_valueCached = false;
		ValueChanged();
	}

	public static new ItemCollection<ListProperty> Create(object obj, bool includeBaseTypes = true)
	{
		// this doesn't work for virtual methods (or any method modifier?)
		var propertyInfos = obj.GetType().GetProperties()
			.Where(p => p.IsRowVisible())
			.Where(p => includeBaseTypes || p.DeclaringType == obj.GetType())
			.OrderBy(p => p.MetadataToken);

		var listProperties = new ItemCollection<ListProperty>();
		var propertyToIndex = new Dictionary<string, int>();
		foreach (PropertyInfo propertyInfo in propertyInfos)
		{
			var listProperty = new ListProperty(obj, propertyInfo);
			if (!listProperty.IsRowVisible())
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
		return ExpandInlined(listProperties, includeBaseTypes);
	}

	// If a member specifies [Inline], replace this member with all it's members
	public static ItemCollection<ListProperty> ExpandInlined(ItemCollection<ListProperty> listProperties, bool includeBaseTypes)
	{
		ItemCollection<ListProperty> newProperties = new();
		foreach (ListProperty listProperty in listProperties)
		{
			if (listProperty.GetCustomAttribute<InlineAttribute>() != null)
			{
				if (listProperty.Value is object value)
				{
					ItemCollection<ListProperty> inlinedProperties = Create(value, includeBaseTypes);
					newProperties.AddRange(inlinedProperties);
				}
			}
			else
			{
				newProperties.Add(listProperty);
			}
		}
		return newProperties;
	}

	public bool IsRowVisible()
	{
		var hideAttribute = PropertyInfo.GetCustomAttribute<HideAttribute>();
		if (hideAttribute?.Values != null)
		{
			if (hideAttribute.Values.Any(v => ObjectUtils.AreEqual(Value, v)))
				return false;
		}

		var classHideAttribute = PropertyInfo.DeclaringType!.GetCustomAttribute<HideAttribute>();
		if (classHideAttribute?.Values != null)
		{
			if (classHideAttribute.Values.Any(v => ObjectUtils.AreEqual(Value, v)))
				return false;
		}

		var hideRowAttribute = PropertyInfo.GetCustomAttribute<HideRowAttribute>();
		if (hideRowAttribute?.Values != null)
		{
			if (hideRowAttribute.Values.Any(v => ObjectUtils.AreEqual(Value, v)))
				return false;
		}
		return true;
	}

	public bool IsColumnVisible()
	{
		var hideAttribute = PropertyInfo.GetCustomAttribute<HideAttribute>();
		if (hideAttribute?.Values != null)
		{
			if (hideAttribute.Values.Any(v => ObjectUtils.AreEqual(Value, v)))
				return false;
		}

		var hideColumnAttribute = PropertyInfo.GetCustomAttribute<HideColumnAttribute>();
		if (hideColumnAttribute?.Values != null)
		{
			if (hideColumnAttribute.Values.Any(v => ObjectUtils.AreEqual(Value, v)))
				return false;
		}
		return true;
	}

	// This can be slow due to lazy property loading
	public static ItemCollection<ListProperty> Sort(ItemCollection<ListProperty> listProperties)
	{
		var sortedProperties = listProperties
			.OrderByDescending(i => i.GetCustomAttribute<AutoSelectAttribute>() != null)
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
