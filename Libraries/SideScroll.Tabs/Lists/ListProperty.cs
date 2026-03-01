using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Utilities;
using System.ComponentModel;
using System.Reflection;

namespace SideScroll.Tabs.Lists;

/// <summary>
/// Interface for properties that can be edited
/// </summary>
public interface IPropertyIsEditable
{
	/// <summary>
	/// Gets whether the property can be edited
	/// </summary>
	bool IsEditable { get; }
}

/// <summary>
/// Represents a property member as a list item with reflection-based value access, editing support, and optional caching
/// </summary>
public class ListProperty : ListMember, IPropertyIsEditable
{
	/// <summary>
	/// Gets the property info for this property
	/// </summary>
	[HiddenColumn]
	public PropertyInfo PropertyInfo { get; }

	/// <summary>
	/// Gets or sets whether the property value should be cached
	/// </summary>
	[HiddenColumn]
	public bool IsCacheable { get; set; }

	private bool _valueCached;
	private object? _valueObject;

	/// <summary>
	/// Gets whether this property can be edited (based on CanWrite, public setter, and ReadOnly attribute)
	/// </summary>
	[HiddenColumn]
	public override bool IsEditable
	{
		get
		{
			var readOnlyAttribute = PropertyInfo.GetCustomAttribute<ReadOnlyAttribute>();
			return PropertyInfo.CanWrite && 
				PropertyInfo.SetMethod?.IsPublic == true && 
				readOnlyAttribute?.IsReadOnly != true;
		}
	}

	/// <summary>
	/// Gets whether the property should be formatted using the Formatted() extension
	/// </summary>
	[Hidden]
	public bool IsFormatted => PropertyInfo.GetCustomAttribute<FormattedAttribute>() != null;

	/// <summary>
	/// Gets or sets the property value, with optional caching and formatting
	/// </summary>
	[EditColumn, InnerValue, WordWrap]
	public override object? Value
	{
		get
		{
			if (Object == null) return null;

			try
			{
				if (IsCacheable)
				{
					if (!_valueCached)
					{
						_valueObject = PropertyInfo.GetValue(Object);

						if (IsFormatted)
						{
							_valueObject = _valueObject.Formatted();
						}
						_valueCached = true;
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
				if (value != null)
				{
					Type type = UnderlyingType;

					if (!type.IsInstanceOfType(value))
					{
						if (value is IConvertible)
						{
							value = Convert.ChangeType(value, type);
						}
						else if (type == typeof(string))
						{
							value = value.ToString();
						}
						else
						{
							throw new InvalidCastException($"Cannot convert {value} to type {type}");
						}
					}
				}

				PropertyInfo.SetValue(Object, value);
				_valueCached = false;
				ValueChanged();
			}
		}
	}

	/// <summary>
	/// Gets the underlying non-nullable type of the property
	/// </summary>
	[Hidden]
	public Type UnderlyingType => PropertyInfo.PropertyType.GetNonNullableType();

	/// <summary>
	/// Gets whether the property should be visible in row displays
	/// </summary>
	[Hidden]
	public bool IsPropertyVisible => PropertyInfo.IsRowVisible();

	public override string? ToString() => Name;

	/// <summary>
	/// Initializes a new ListProperty for the specified property
	/// </summary>
	public ListProperty(object obj, PropertyInfo propertyInfo, bool isCacheable = true) :
		base(obj, propertyInfo)
	{
		PropertyInfo = propertyInfo;
		IsCacheable = isCacheable;

		NameAttribute? nameAttribute = propertyInfo.GetCustomAttribute<NameAttribute>();

		Name = nameAttribute?.Name ?? propertyInfo.Name.WordSpaced();

		if (PropertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null ||
			PropertyInfo.PropertyType.GetCustomAttribute<DebugOnlyAttribute>() != null)
		{
			Name = "* " + Name;
		}

		if (obj is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged += ListProperty_PropertyChanged;
		}
	}

	/// <summary>
	/// Initializes a new ListProperty by property name
	/// </summary>
	public ListProperty(object obj, string propertyName, bool isCacheable = true) :
		this(obj, obj.GetType().GetProperty(propertyName)!, isCacheable)
	{
	}

	/// <summary>
	/// Handles property change notifications from the source object to invalidate cache
	/// </summary>
	protected void ListProperty_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != MemberInfo.Name) return;

		_valueCached = false;
		ValueChanged();
	}

	/// <summary>
	/// Creates a collection of list properties from an object using reflection
	/// </summary>
	/// <param name="obj">The object to extract properties from</param>
	/// <param name="includeBaseTypes">Whether to include properties from base types</param>
	/// <param name="includeStatic">Whether to include static properties</param>
	public new static ItemCollection<ListProperty> Create(object obj, bool includeBaseTypes = true, bool includeStatic = true)
	{
		// this doesn't work for virtual methods (or any method modifier?)
		var propertyInfos = obj.GetType().GetProperties()
			.Where(p => p.IsRowVisible())
			.Where(p => p.GetGetMethod(false)?.GetParameters().Length == 0)
			.Where(p => includeBaseTypes || p.DeclaringType == obj.GetType())
			.Where(p => includeStatic || !p.GetAccessors(nonPublic: true)[0].IsStatic)
			.OrderBy(p => p.Module.Name)
			.ThenBy(p => p.MetadataToken);

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

	/// <summary>
	/// Expands properties marked with [Inline] attribute by replacing them with their inner properties
	/// </summary>
	public static ItemCollection<ListProperty> ExpandInlined(ItemCollection<ListProperty> listProperties, bool includeBaseTypes)
	{
		ItemCollection<ListProperty> newProperties = [];
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

	/// <summary>
	/// Determines whether the property should be visible as a row based on Hide attributes
	/// </summary>
	public bool IsRowVisible()
	{
		var hideAttribute = PropertyInfo.GetCustomAttribute<HideAttribute>();
		if (hideAttribute?.Values.Any(v => ObjectUtils.AreEqual(Value, v)) == true)
			return false;

		var classHideAttribute = PropertyInfo.DeclaringType!.GetCustomAttribute<HideAttribute>();
		if (classHideAttribute?.Values.Any(v => ObjectUtils.AreEqual(Value, v)) == true)
			return false;

		var hideRowAttribute = PropertyInfo.GetCustomAttribute<HideRowAttribute>();
		if (hideRowAttribute?.Values.Any(v => ObjectUtils.AreEqual(Value, v)) == true)
			return false;

		return true;
	}

	/// <summary>
	/// Determines whether the property should be visible as a column based on Hide attributes
	/// </summary>
	public bool IsColumnVisible()
	{
		var hideAttribute = PropertyInfo.GetCustomAttribute<HideAttribute>();
		if (hideAttribute?.Values.Any(v => ObjectUtils.AreEqual(Value, v)) == true)
			return false;

		var hideColumnAttribute = PropertyInfo.GetCustomAttribute<HideColumnAttribute>();
		if (hideColumnAttribute?.Values.Any(v => ObjectUtils.AreEqual(Value, v)) == true)
			return false;

		return true;
	}

	/// <summary>
	/// Sorts properties by auto-select attribute and link presence
	/// </summary>
	public static ItemCollection<ListProperty> Sort(IEnumerable<ListProperty> listProperties)
	{
		var sortedProperties = listProperties
			.OrderByDescending(i => i.GetCustomAttribute<AutoSelectAttribute>() != null)
			.ThenByDescending(i => TabUtils.ObjectHasLinks(i, true));

		var linkSorted = new ItemCollection<ListProperty>(sortedProperties);
		return linkSorted;
	}
}
