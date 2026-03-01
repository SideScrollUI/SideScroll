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
	/// Creates a collection of list members (properties, methods, fields) from an object using reflection
	/// </summary>
	/// <param name="obj">The object to extract members from</param>
	/// <param name="includeBaseTypes">Whether to include members from base types</param>
	/// <param name="includeStatic">Whether to include static members</param>
	public static ItemCollection<ListMember> Create(object obj, bool includeBaseTypes = true, bool includeStatic = true)
	{
		var methodMembers = new SortedDictionary<string, ListMember>();

		var properties = ListProperty.Create(obj, includeBaseTypes, includeStatic);
		foreach (ListProperty listProperty in properties)
		{
			// MetadataTokens are only unique across modules
			MethodInfo getMethod = listProperty.PropertyInfo.GetGetMethod(false)!;
			string id = $"{getMethod.Module.Name}:{getMethod.MetadataToken:D10}";
			methodMembers.Add(id, listProperty);
		}

		var methods = ListMethod.Create(obj, includeBaseTypes, includeStatic);
		foreach (ListMethod listMethod in methods)
		{
			string id = $"{listMethod.MethodInfo.Module.Name}:{listMethod.MethodInfo.MetadataToken:D10}";
			methodMembers.Add(id, listMethod);
		}

		var listMembers = methodMembers.Values.ToList();

		// Field MetadataToken's don't line up with the method or property tokens and are added to end
		// Use property's backing field? (confirmed field order matches)
		// No simple way to link property and backing fields
		// .GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		var listFields = ListField.Create(obj, includeBaseTypes, includeStatic);
		listMembers.AddRange(listFields);

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
			if (listMember.GetCustomAttribute<InlineAttribute>() != null)
			{
				if (listMember.Value is object value)
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
