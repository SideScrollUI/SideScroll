using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Extensions;
using System.ComponentModel;
using System.Reflection;

namespace SideScroll.Tabs.Lists;

/// <summary>
/// Interface for list items that can be automatically selected based on priority order
/// </summary>
public interface IListAutoSelect
{
	/// <summary>
	/// Gets the order priority for auto-selection (higher values selected first)
	/// </summary>
	int Order { get; }
}

/// <summary>
/// Interface for key-value pairs in lists
/// </summary>
public interface IListPair
{
	/// <summary>
	/// Gets the key/name of the pair
	/// </summary>
	[Name("Name")]
	object? Key { get; }

	/// <summary>
	/// Gets the value of the pair
	/// </summary>
	[InnerValue, StyleValue]
	object? Value { get; }
}

/// <summary>
/// Interface for list items with maximum desired width constraint
/// </summary>
public interface IMaxDesiredWidth
{
	/// <summary>
	/// Gets the maximum desired width in pixels
	/// </summary>
	int? MaxDesiredWidth { get; }
}

/// <summary>
/// Interface for list items with maximum desired height constraint
/// </summary>
public interface IMaxDesiredHeight
{
	/// <summary>
	/// Gets the maximum desired height in pixels
	/// </summary>
	int? MaxDesiredHeight { get; }
}

/// <summary>
/// Base class for representing object members (properties, fields, methods) as list items with reflection support
/// </summary>
public abstract class ListMember(object obj, MemberInfo memberInfo) : IListPair, IListItem, INotifyPropertyChanged,
	IListAutoSelect, IMaxDesiredWidth, IMaxDesiredHeight
{
	/// <summary>
	/// Gets or sets the maximum string length to display (default: 1000)
	/// </summary>
	public static int MaxStringLength { get; set; } = 1000;

	/// <summary>
	/// Gets or sets the default maximum desired height (default: 500)
	/// </summary>
	public static int DefaultMaxDesiredHeight { get; set; } = 500;

	/// <summary>
	/// Event raised when a property value changes
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Gets the member info for this list member
	/// </summary>
	[HiddenColumn]
	public MemberInfo MemberInfo => memberInfo;

	/// <summary>
	/// Gets the object that owns this member
	/// </summary>
	[HiddenColumn]
	public object Object => obj;

	/// <summary>
	/// Gets or sets the display name for this member
	/// </summary>
	[AutoSize]
	public string? Name { get; set; }

	/// <summary>
	/// Gets the key (same as Name) for IListPair interface
	/// </summary>
	[HiddenColumn]
	public object? Key => Name;

	/// <summary>
	/// Gets or sets the order priority for auto-selection
	/// </summary>
	[Hidden]
	public int Order { get; set; } = 0;

	/// <summary>
	/// Gets whether this member can be edited
	/// </summary>
	[HiddenColumn]
	public virtual bool IsEditable => true;

	/// <summary>
	/// Gets or sets whether this member can be auto-selected
	/// </summary>
	[HiddenColumn]
	public bool IsAutoSelectable { get; set; } = true;

	/// <summary>
	/// Gets the maximum desired width from MaxWidth attribute if present
	/// </summary>
	[HiddenColumn]
	public int? MaxDesiredWidth => GetCustomAttribute<MaxWidthAttribute>()?.MaxWidth;

	/// <summary>
	/// Gets the maximum desired height from MaxHeight attribute or default value
	/// </summary>
	[HiddenColumn]
	public int? MaxDesiredHeight => GetCustomAttribute<MaxHeightAttribute>()?.MaxHeight ?? DefaultMaxDesiredHeight;

	/// <summary>
	/// Gets or sets the value of this member
	/// </summary>
	[StyleValue, InnerValue, WordWrap]
	public abstract object? Value { get; set; }

	/// <summary>
	/// Gets or sets the value as formatted text, truncated to MaxStringLength
	/// </summary>
	[HiddenColumn]
	public object? ValueText
	{
		get
		{
			try
			{
				object? value = Value;
				if (value == null)
				{
					return null;
				}
				else if (value is string text)
				{
					if (text.Length > MaxStringLength)
					{
						return text[..MaxStringLength];
					}
				}
				else if (!value.GetType().IsPrimitive)
				{
					return value.Formatted();
				}
				return value;
			}
			catch (Exception)
			{
				return null;
			}
		}
		set
		{
			Value = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueText)));
		}
	}

	public override string? ToString() => Name;

	/// <summary>
	/// Raises the PropertyChanged event for the Value property
	/// </summary>
	protected void ValueChanged()
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
	}

	/// <summary>
	/// Gets a custom attribute of the specified type from the member info
	/// </summary>
	public T? GetCustomAttribute<T>() where T : Attribute
	{
		return MemberInfo.GetCustomAttribute<T>();
	}

	/// <summary>
	/// Gets a custom attribute of the specified type from the member info
	/// </summary>
	public bool HasCustomAttribute<T>() where T : Attribute
	{
		return MemberInfo.GetCustomAttribute<T>() != null;
	}

	/// <summary>
	/// Sorts list members by auto-select attribute and link presence
	/// </summary>
	public static ItemCollection<ListMember> Sort(IEnumerable<ListMember> items)
	{
		var sortedMembers = items
			.OrderByDescending(i => i.MemberInfo.GetCustomAttribute<AutoSelectAttribute>() != null)
			.ThenByDescending(i => TabUtils.ObjectHasLinks(i, true));

		var linkSorted = new ItemCollection<ListMember>(sortedMembers);
		return linkSorted;
	}

	/// <summary>
	/// Creates a collection of list members (properties, methods, fields) from an object using reflection.
	/// Uses <see cref="ReflectionCache"/> to avoid rebuilding the filtered/sorted
	/// <see cref="System.Reflection.MemberInfo"/> arrays and the merged
	/// <see cref="SortedDictionary{TKey,TValue}"/> on every call for the same type.
	/// </summary>
	/// <param name="obj">The object to extract members from</param>
	/// <param name="includeBaseTypes">Whether to include members from base types</param>
	/// <param name="includeStatic">Whether to include static members</param>
	public static ItemCollection<ListMember> Create(object obj, bool includeBaseTypes = true, bool includeStatic = true)
	{
		Type type = obj.GetType();

		// Cached: merged (properties + [Item] methods) sorted by MetadataToken,
		// with duplicate-name hiding already applied (most-derived wins).
		(string SortKey, MemberInfo Member)[] mergedMethodMembers =
			ReflectionCache.GetMergedMethodMembers(type, includeBaseTypes, includeStatic);

		// Cached: structurally-filtered, sorted field infos.
		// Field MetadataTokens don't align with property/method tokens, so fields are appended after.
		FieldInfo[] fieldInfos = ReflectionCache.GetFields(type, includeBaseTypes, includeStatic);

		var listMembers = new List<ListMember>(mergedMethodMembers.Length + fieldInfos.Length);

		foreach ((_, MemberInfo info) in mergedMethodMembers)
		{
			ListMember member;
			if (info is PropertyInfo propertyInfo)
			{
				var listProperty = new ListProperty(obj, propertyInfo);
				// IsRowVisible() is unconditionally true when the property has no [Hide]/[HideRow] attributes.
				if (ReflectionCache.PropertyHasValueDependentHide(propertyInfo) && !listProperty.IsRowVisible())
					continue;
				member = listProperty;
			}
			else
			{
				member = new ListMethod(obj, (MethodInfo)info);
			}
			listMembers.Add(member);
		}

		foreach (FieldInfo fieldInfo in fieldInfos)
		{
			var listField = new ListField(obj, fieldInfo);
			// IsRowVisible() is unconditionally true when the field has no [Hide]/[HideRow] attributes.
			if (ReflectionCache.FieldHasValueDependentHide(fieldInfo) && !listField.IsRowVisible())
				continue;
			listMembers.Add(listField);
		}

		return ExpandInlined(listMembers, includeBaseTypes);
	}

	/// <summary>
	/// Expands members marked with [Inline] attribute by replacing them with their inner members
	/// </summary>
	public static ItemCollection<ListMember> ExpandInlined(List<ListMember> listMembers, bool includeBaseTypes, bool includeStatic = true)
	{
		ItemCollection<ListMember> newMembers = [];
		foreach (ListMember listMember in listMembers)
		{
			if (listMember.HasCustomAttribute<InlineAttribute>())
			{
				if (listMember.Value is { } value)
				{
					ItemCollection<ListMember> inlinedProperties = Create(value, includeBaseTypes, includeStatic);
					newMembers.AddRange(inlinedProperties);
				}
			}
			else
			{
				newMembers.Add(listMember);
			}
		}
		return newMembers;
	}
}
