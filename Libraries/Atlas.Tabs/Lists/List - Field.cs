using System;
using System.Linq;
using System.Reflection;
using Atlas.Core;
using Atlas.Extensions;

namespace Atlas.Tabs
{
	public class ListField : ListMember, IPropertyEditable
	{
		public FieldInfo fieldInfo;
		
		[HiddenColumn]
		public override bool Editable { get { return true; } }

		[Editing]
		[InnerValue]
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
			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				listFields.Add(new ListField(obj, fieldInfo));
			}
			return listFields;
		}
	}
}

/*
*/