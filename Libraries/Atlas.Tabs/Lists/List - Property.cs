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
	public interface IPropertyEditable
	{
		bool Editable { get; }
	}
	public interface IMaxDesiredWidth
	{
		int? MaxDesiredWidth { get; }
	}

	public class ListProperty : ListMember, IPropertyEditable, IMaxDesiredWidth
	{
		public PropertyInfo propertyInfo;
		private bool cached;
		private bool valueCached;
		private object valueObject = null;

		[HiddenColumn]
		public override bool Editable // rename to IsReadOnly?
		{
			get
			{
				bool propertyReadOnly = (propertyInfo.GetCustomAttribute<ReadOnlyAttribute>() != null);
				return propertyInfo.CanWrite && !propertyReadOnly;
			}
		}

		[HiddenColumn]
		public int? MaxDesiredWidth
		{
			get
			{
				var maxWidthAttribute = propertyInfo.GetCustomAttribute<ColumnMaxWidthAttribute>();
				if (maxWidthAttribute != null)
					return maxWidthAttribute.MaxWidth;
				return null;
			}
		}

		[Editing, InnerValue, WordWrap]
		public override object Value
		{
			get
			{
				try
				{
					if (cached)
					{
						if (!valueCached)
						{
							valueCached = true;
							valueObject = propertyInfo.GetValue(obj);
						}
						return valueObject;
					}
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

		public ListProperty(object obj, PropertyInfo propertyInfo, bool cached = true) : 
			base(obj, propertyInfo)
		{
			this.propertyInfo = propertyInfo;
			this.cached = cached;
			var accessors = propertyInfo.GetAccessors(true);
			autoLoad = !accessors[0].IsStatic;

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
			// this doesn't work for virtual methods (or any method modifier?)
			PropertyInfo[] propertyInfos = obj.GetType().GetProperties().OrderBy(x => x.MetadataToken).ToArray();
			var listProperties = new ItemCollection<ListProperty>();
			var propertyToIndex = new Dictionary<string, int>();
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (!propertyInfo.DeclaringType.IsNotPublic)
				{
					if (propertyInfo.GetCustomAttribute<HiddenRowAttribute>() != null)
						continue;
					if (propertyInfo.DeclaringType.IsNotPublic)
						continue;

					ListProperty listProperty = new ListProperty(obj, propertyInfo);

					int index;
					if (propertyToIndex.TryGetValue(propertyInfo.Name, out index))
					{
						listProperties.RemoveAt(index);
						listProperties.Insert(index, listProperty);
					}
					else
					{
						propertyToIndex[propertyInfo.Name] = listProperties.Count;
						listProperties.Add(listProperty);
					}
				}
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