using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Atlas.Core;
using Atlas.Extensions;

namespace Atlas.Tabs
{
	public interface IListEditable
	{
		bool Editable { get; }
	}

	public class ListProperty : ListMember, IListEditable
	{
		public PropertyInfo propertyInfo;
		
		[HiddenColumn]
		public override bool Editable { get { return propertyInfo.CanWrite; } }

		[Editing]
		[InnerValue]
		public override object Value
		{
			get
			{
				try
				{
					return propertyInfo.GetValue(obj);
				}
				catch (Exception)
				{
					return null;
				}
			}
			set
			{
				if (propertyInfo.CanWrite)
				{
					Type type = propertyInfo.PropertyType;
					if (value != null)
					{
						type = type.GetNonNullableType();
					}
					propertyInfo.SetValue(obj, Convert.ChangeType(value, type));

					INotifyPropertyChanged notifyPropertyChanged = obj as INotifyPropertyChanged;
					if (notifyPropertyChanged != null)
					{
						//notifyPropertyChanged.PropertyChanged?.Invoke(obj, new PropertyChangedEventArgs(propertyName));
					}
				}
			}
		}

		public ListProperty(object obj, PropertyInfo propertyInfo) : 
			base(obj, propertyInfo)
		{
			this.propertyInfo = propertyInfo;
			
			Name = propertyInfo.Name;
			Name = Name.AddSpacesBetweenWords();
			NameAttribute attribute = propertyInfo.GetCustomAttribute<NameAttribute>();
			if (attribute != null)
				Name = attribute.Name;
		}

		public override string ToString()
		{
			return Name;
		}

		public static ItemCollection<ListProperty> Create(object obj)
		{
			PropertyInfo[] properties = obj.GetType().GetProperties().OrderBy(x => x.MetadataToken).ToArray();
			ItemCollection<ListProperty> listProperties = new ItemCollection<ListProperty>();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.GetCustomAttribute(typeof(HiddenRowAttribute)) != null)
					continue;
				if (!propertyInfo.DeclaringType.IsNotPublic)
					listProperties.Add(new ListProperty(obj, propertyInfo));
			}
			return listProperties;
		}
	}
}

/*
What do we do about child objects?
	can't edit child objects

	DataBinding

This class only works alone, no fields
Just expand these properties only
*/