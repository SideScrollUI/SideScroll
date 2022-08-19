using Atlas.Core;
using Atlas.Extensions;
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
	public override object? Value
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

	[Hidden]
	public bool IsFieldVisible => FieldInfo.IsRowVisible();

	public override string? ToString() => Name;

	public ListField(object obj, FieldInfo fieldInfo) :
		base(obj, fieldInfo)
	{
		FieldInfo = fieldInfo;
		AutoLoad = !fieldInfo.IsStatic;

		NameAttribute? nameAttribute = fieldInfo.GetCustomAttribute<NameAttribute>();

		Name = nameAttribute?.Name ?? fieldInfo.Name.WordSpaced();

		if (FieldInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
			Name = "* " + Name;
	}

	public static new ItemCollection<ListField> Create(object obj, bool includeBaseTypes = true)
	{
		var fieldInfos = obj.GetType().GetFields()
			.Where(f => f.IsRowVisible())
			.Where(f => includeBaseTypes || f.DeclaringType == obj.GetType())
			.OrderBy(f => f.MetadataToken);

		var listFields = new ItemCollection<ListField>();
		// replace any overriden/new field & properties
		var fieldToIndex = new Dictionary<string, int>();
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
		if (hideAttribute?.Values != null)
		{
			if (hideAttribute.Values.Any(v => ObjectUtils.AreEqual(Value, v)))
				return false;
		}

		var classHideAttribute = FieldInfo.DeclaringType!.GetCustomAttribute<HideAttribute>();
		if (classHideAttribute?.Values != null)
		{
			if (classHideAttribute.Values.Any(v => ObjectUtils.AreEqual(Value, v)))
				return false;
		}

		var hideRowAttribute = FieldInfo.GetCustomAttribute<HideRowAttribute>();
		if (hideRowAttribute?.Values != null)
		{
			if (hideRowAttribute.Values.Any(v => ObjectUtils.AreEqual(Value, v)))
				return false;
		}
		return true;
	}
}
