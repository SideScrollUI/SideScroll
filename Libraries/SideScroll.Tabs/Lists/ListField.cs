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

		// Display name is purely structural — look up the per-FieldInfo cache.
		Name = ReflectionCache.GetFieldDisplayName(fieldInfo);
	}

	/// <summary>
	/// Creates a collection of list fields from an object using reflection
	/// </summary>
	/// <param name="obj">The object to extract fields from</param>
	/// <param name="includeBaseTypes">Whether to include fields from base types</param>
	/// <param name="includeStatic">Whether to include static fields</param>
	public new static ItemCollection<ListField> Create(object obj, bool includeBaseTypes = true, bool includeStatic = true)
	{
		// Use cached, structurally-filtered, sorted FieldInfo[] to avoid repeated LINQ evaluation.
		FieldInfo[] fieldInfos = ReflectionCache.GetFields(obj.GetType(), includeBaseTypes, includeStatic);

		ItemCollection<ListField> listFields = [];
		// Replace any overridden/new field & properties
		var fieldToIndex = new Dictionary<string, int>(fieldInfos.Length);
		foreach (FieldInfo fieldInfo in fieldInfos)
		{
			var listField = new ListField(obj, fieldInfo);
			// IsRowVisible() is unconditionally true when the field has no [Hide]/[HideRow] attributes.
			if (ReflectionCache.FieldHasValueDependentHide(fieldInfo) && !listField.IsRowVisible())
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
