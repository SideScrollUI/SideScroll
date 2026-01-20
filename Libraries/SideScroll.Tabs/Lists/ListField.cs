using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Utilities;
using System.Reflection;

namespace SideScroll.Tabs.Lists;

public class ListField : ListMember, IPropertyIsEditable
{
	[HiddenColumn]
	public FieldInfo FieldInfo { get; }

	[HiddenColumn]
	public override bool IsEditable => true;

	[Hidden]
	public bool IsFormatted => FieldInfo.GetCustomAttribute<FormattedAttribute>() != null;

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

	[Hidden]
	public bool IsFieldVisible => FieldInfo.IsRowVisible();

	public override string? ToString() => Name;

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
