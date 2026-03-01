using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Utilities;
using System.Reflection;

namespace SideScroll.Tabs.Lists;

/// <summary>
/// Represents a field member as a list item with reflection-based value access and editing support
/// </summary>
public class ListField : ListMember, IPropertyIsEditable
{
	/// <summary>
	/// Gets the field info for this field
	/// </summary>
	[HiddenColumn]
	public FieldInfo FieldInfo { get; }

	/// <summary>
	/// Gets whether this field can be edited (fields are always editable)
	/// </summary>
	[HiddenColumn]
	public override bool IsEditable => true;

	/// <summary>
	/// Gets whether the field should be formatted using the Formatted() extension
	/// </summary>
	[Hidden]
	public bool IsFormatted => FieldInfo.GetCustomAttribute<FormattedAttribute>() != null;

	/// <summary>
	/// Gets or sets the field value, with optional formatting
	/// </summary>
	[EditColumn, InnerValue]
	public override object? Value
	{
		get
		{
			try
			{
				var value = FieldInfo.GetValue(Object);

				if (IsFormatted)
				{
					value = value.Formatted();
				}

				return value;
			}
			catch (Exception)
			{
				return null;
			}
		}
		set => FieldInfo.SetValue(Object, Convert.ChangeType(value, FieldInfo.FieldType));
	}

	/// <summary>
	/// Gets whether the field should be visible in row displays
	/// </summary>
	[Hidden]
	public bool IsFieldVisible => FieldInfo.IsRowVisible();

	public override string? ToString() => Name;

	/// <summary>
	/// Initializes a new ListField for the specified field
	/// </summary>
	public ListField(object obj, FieldInfo fieldInfo) :
		base(obj, fieldInfo)
	{
		FieldInfo = fieldInfo;
		IsAutoSelectable = !fieldInfo.IsStatic;

		NameAttribute? nameAttribute = fieldInfo.GetCustomAttribute<NameAttribute>();

		Name = nameAttribute?.Name ?? fieldInfo.Name.WordSpaced();

		if (FieldInfo.GetCustomAttribute<DebugOnlyAttribute>() != null &&
			FieldInfo.FieldType.GetCustomAttribute<DebugOnlyAttribute>() != null)
		{
			Name = "* " + Name;
		}
	}

	/// <summary>
	/// Creates a collection of list fields from an object using reflection
	/// </summary>
	/// <param name="obj">The object to extract fields from</param>
	/// <param name="includeBaseTypes">Whether to include fields from base types</param>
	/// <param name="includeStatic">Whether to include static fields</param>
	public new static ItemCollection<ListField> Create(object obj, bool includeBaseTypes = true, bool includeStatic = true)
	{
		var fieldInfos = obj.GetType().GetFields()
			.Where(f => f.IsRowVisible())
			.Where(f => includeBaseTypes || f.DeclaringType == obj.GetType())
			.Where(f => includeStatic || !f.IsStatic)
			.OrderBy(f => f.Module.Name)
			.ThenBy(f => f.MetadataToken);

		ItemCollection<ListField> listFields = [];
		// replace any overriden/new field & properties
		Dictionary<string, int> fieldToIndex = [];
		foreach (FieldInfo fieldInfo in fieldInfos)
		{
			var listField = new ListField(obj, fieldInfo);
			if (!listField.IsRowVisible())
				continue;

			if (fieldToIndex.TryGetValue(fieldInfo.Name, out int index))
			{
				listFields.RemoveAt(index);
				listFields.Insert(index, listField);
			}
			else
			{
				fieldToIndex[fieldInfo.Name] = listFields.Count;
				listFields.Add(listField);
			}
		}
		return listFields;
	}

	/// <summary>
	/// Determines whether the field should be visible as a row based on Hide attributes
	/// </summary>
	public bool IsRowVisible()
	{
		var hideAttribute = FieldInfo.GetCustomAttribute<HideAttribute>();
		if (hideAttribute?.Values.Any(v => ObjectUtils.AreEqual(Value, v)) == true)
			return false;

		var classHideAttribute = FieldInfo.DeclaringType?.GetCustomAttribute<HideAttribute>();
		if (classHideAttribute?.Values.Any(v => ObjectUtils.AreEqual(Value, v)) == true)
			return false;

		var hideRowAttribute = FieldInfo.GetCustomAttribute<HideRowAttribute>();
		if (hideRowAttribute?.Values.Any(v => ObjectUtils.AreEqual(Value, v)) == true)
			return false;

		return true;
	}
}
