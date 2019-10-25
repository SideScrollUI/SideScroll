using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Atlas.Core;
using Atlas.Extensions;

namespace Atlas.Tabs
{
	public class ListField : ListMember, IPropertyEditable, IMaxDesiredWidth
	{
		public FieldInfo fieldInfo;
		
		[HiddenColumn]
		public override bool Editable { get { return true; } }

		[HiddenColumn]
		public int? MaxDesiredWidth
		{
			get
			{
				var maxWidthAttribute = fieldInfo.GetCustomAttribute<ColumnMaxWidthAttribute>();
				if (maxWidthAttribute != null)
					return maxWidthAttribute.MaxWidth;
				return null;
			}
		}

		[Editing, InnerValue]
		public override object Value
		{
			get
			{
				try
				{
					return fieldInfo.GetValue(obj);
				}
				catch (Exception)
				{
					return null;
				}
			}
			set
			{
				fieldInfo.SetValue(obj, Convert.ChangeType(value, fieldInfo.FieldType));
			}
		}

		public ListField(object obj, FieldInfo fieldInfo) : 
			base(obj, fieldInfo)
		{
			this.fieldInfo = fieldInfo;
			autoLoad = !fieldInfo.IsStatic;

			Name = fieldInfo.Name;
			Name = Name.AddSpacesBetweenWords();
			NameAttribute attribute = fieldInfo.GetCustomAttribute<NameAttribute>();
			if (attribute != null)
				Name = attribute.Name;
		}

		public override string ToString()
		{
			return Name;
		}

		public static ItemCollection<ListField> Create(object obj)
		{
			FieldInfo[] fieldInfos = obj.GetType().GetFields().OrderBy(x => x.MetadataToken).ToArray();
			ItemCollection<ListField> listFields = new ItemCollection<ListField>();
			// replace any overriden/new field & properties
			var fieldToIndex = new Dictionary<string, int>();
			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				if (fieldInfo.GetCustomAttribute<HiddenRowAttribute>() != null)
					continue;
				ListField listField = new ListField(obj, fieldInfo);
				int index;
				if (fieldToIndex.TryGetValue(fieldInfo.Name, out index))
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
	}
}

/*
*/