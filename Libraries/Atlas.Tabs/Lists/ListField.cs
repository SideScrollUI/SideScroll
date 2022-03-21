using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Atlas.Tabs;

public class ListField : ListMember, IPropertyEditable
{
	public readonly FieldInfo FieldInfo;

	[HiddenColumn]
	public override bool Editable => true;

	[Hidden]
	public bool IsFormatted => (FieldInfo.GetCustomAttribute<FormattedAttribute>() != null);

	[Editing, InnerValue]
	public override object Value
	{
		get
		{
			try
			{
				var value = FieldInfo.GetValue(Object);

				if (IsFormatted)
					value = value.Formatted();

				return value;
			}
			catch (Exception)
			{
				return null;
			}
		}
		set
		{
			FieldInfo.SetValue(Object, Convert.ChangeType(value, FieldInfo.FieldType));
		}
	}

	public override string ToString() => Name;

	public ListField(object obj, FieldInfo fieldInfo) :
		base(obj, fieldInfo)
	{
		FieldInfo = fieldInfo;
		AutoLoad = !fieldInfo.IsStatic;

		NameAttribute attribute = fieldInfo.GetCustomAttribute<NameAttribute>();

		Name = attribute?.Name ?? fieldInfo.Name.WordSpaced();

		if (FieldInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
			Name = "* " + Name;
	}

	public static new ItemCollection<ListField> Create(object obj, bool includeBaseTypes = true)
	{
		var fieldInfos = obj.GetType().GetFields()
			.Where(f => IsVisible(f))
			.Where(f => includeBaseTypes || f.DeclaringType == obj.GetType())
			.OrderBy(f => f.MetadataToken);

		var listFields = new ItemCollection<ListField>();
		// replace any overriden/new field & properties
		var fieldToIndex = new Dictionary<string, int>();
		foreach (FieldInfo fieldInfo in fieldInfos)
		{
			var listField = new ListField(obj, fieldInfo);
			if (!listField.IsObjectVisible())
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


	public static bool IsVisible(FieldInfo fieldInfo)
	{
		if (fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
			return false;

#if !DEBUG
			if (fieldInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return fieldInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			fieldInfo.GetCustomAttribute<HiddenRowAttribute>() == null; // [HiddenRow]
	}

	public bool IsObjectVisible()
	{
		if (FieldInfo.GetCustomAttribute<HideNullAttribute>() != null)
		{
			if (Value == null)
				return false;
		}

		var hideAttribute = FieldInfo.GetCustomAttribute<HideAttribute>();
		if (hideAttribute?.Values != null)
		{
			return !hideAttribute.Values.Any(v => ObjectUtils.AreEqual(Value, v));
		}
		return true;
	}
}
