using Atlas.Core;
using Atlas.Extensions;
using System.ComponentModel;
using System.Reflection;

namespace Atlas.Tabs;

public interface IListAutoSelect
{
	int Order { get; }
}

public interface IListPair
{
	[Name("Name")]
	object? Key { get; }

	[InnerValue, StyleValue]
	object? Value { get; }
}

public interface IMaxDesiredWidth
{
	int? MaxDesiredWidth { get; }
}

public interface IMaxDesiredHeight
{
	int? MaxDesiredHeight { get; }
}

public abstract class ListMember(object obj, MemberInfo memberInfo) : IListPair, IListItem, INotifyPropertyChanged,
	IListAutoSelect, IMaxDesiredWidth, IMaxDesiredHeight
{
	public const int MaxStringLength = 1000;
	private const int DefaultMaxDesiredHeight = 500;

	public event PropertyChangedEventHandler? PropertyChanged;

	public readonly MemberInfo MemberInfo = memberInfo;

	public readonly object Object = obj;

	[AutoSize]
	public string? Name { get; set; }

	[HiddenColumn]
	public object? Key => Name;

	[Hidden]
	public int Order { get; set; } = 0;

	[HiddenColumn]
	public virtual bool Editable => true;

	public bool AutoLoad = true;

	[HiddenColumn]
	public int? MaxDesiredWidth => GetCustomAttribute<MaxWidthAttribute>()?.MaxWidth;

	[HiddenColumn]
	public int? MaxDesiredHeight => GetCustomAttribute<MaxHeightAttribute>()?.MaxHeight ?? DefaultMaxDesiredHeight;

	[StyleValue, InnerValue, WordWrap]
	public abstract object? Value { get; set; }

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

	protected void ValueChanged()
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
	}

	public T? GetCustomAttribute<T>() where T : Attribute
	{
		return MemberInfo.GetCustomAttribute<T>();
	}

	public static ItemCollection<ListMember> Sort(IEnumerable<ListMember> items)
	{
		var sortedMembers = items
			.OrderByDescending(i => i.MemberInfo.GetCustomAttribute<AutoSelectAttribute>() != null)
			.ThenByDescending(i => TabUtils.ObjectHasLinks(i, true));

		var linkSorted = new ItemCollection<ListMember>(sortedMembers);
		return linkSorted;
	}

	public static ItemCollection<ListMember> Create(object obj, bool includeBaseTypes = true)
	{
		var methodMembers = new SortedDictionary<string, ListMember>();

		var properties = ListProperty.Create(obj, includeBaseTypes);
		foreach (ListProperty listProperty in properties)
		{
			// MetadataTokens are only unique across modules
			MethodInfo getMethod = listProperty.PropertyInfo.GetGetMethod(false)!;
			string id = $"{getMethod.Module.Name}:{getMethod.MetadataToken:D10}";
			methodMembers.Add(id, listProperty);
		}

		var methods = ListMethod.Create(obj, includeBaseTypes);
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
		var listFields = ListField.Create(obj, includeBaseTypes);
		listMembers.AddRange(listFields);

		return ExpandInlined(listMembers, includeBaseTypes);
	}

	// If a member specifies [Inline], replace this member with all it's members
	public static ItemCollection<ListMember> ExpandInlined(List<ListMember> listMembers, bool includeBaseTypes)
	{
		ItemCollection<ListMember> newMembers = [];
		foreach (ListMember listMember in listMembers)
		{
			if (listMember.GetCustomAttribute<InlineAttribute>() != null)
			{
				if (listMember.Value is object value)
				{
					ItemCollection<ListMember> inlinedProperties = Create(value, includeBaseTypes);
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
