using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

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
		public PropertyInfo PropertyInfo;
		public bool Cached;

		private bool _valueCached;
		private object _valueObject = null;

		[HiddenColumn]
		public override bool Editable // rename to IsReadOnly?
		{
			get
			{
				bool propertyReadOnly = (PropertyInfo.GetCustomAttribute<ReadOnlyAttribute>() != null);
				return PropertyInfo.CanWrite && !propertyReadOnly;
			}
		}

		[HiddenColumn]
		public int? MaxDesiredWidth
		{
			get
			{
				var maxWidthAttribute = PropertyInfo.GetCustomAttribute<MaxWidthAttribute>();
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
					if (Cached)
					{
						if (!_valueCached)
						{
							_valueCached = true;
							_valueObject = PropertyInfo.GetValue(Object);
						}
						return _valueObject;
					}
					return PropertyInfo.GetValue(Object);
				}
				catch (Exception)
				{
					return null;
				}
			}
			set
			{
				if (PropertyInfo.CanWrite)
				{
					Type type = PropertyInfo.PropertyType;
					if (value != null)
					{
						type = type.GetNonNullableType();
					}
					PropertyInfo.SetValue(Object, Convert.ChangeType(value, type));

					if (Object is INotifyPropertyChanged notifyPropertyChanged)
					{
						//notifyPropertyChanged.PropertyChanged?.Invoke(obj, new PropertyChangedEventArgs(propertyName));
					}
				}
			}
		}

		[Hidden]
		public Type UnderlyingType => PropertyInfo.PropertyType.GetNonNullableType();

		public override string ToString() => Name;

		public ListProperty(object obj, PropertyInfo propertyInfo, bool cached = true) : 
			base(obj, propertyInfo)
		{
			PropertyInfo = propertyInfo;
			Cached = cached;
			var accessors = propertyInfo.GetAccessors(true);
			AutoLoad = !accessors[0].IsStatic;

			Name = propertyInfo.Name;
			if (PropertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				Name = "*" + Name;
			Name = Name.WordSpaced();
			NameAttribute attribute = propertyInfo.GetCustomAttribute<NameAttribute>();
			if (attribute != null)
				Name = attribute.Name;
		}

		public static new ItemCollection<ListProperty> Create(object obj)
		{
			// this doesn't work for virtual methods (or any method modifier?)
			PropertyInfo[] propertyInfos = obj.GetType().GetProperties().OrderBy(x => x.MetadataToken).ToArray();
			var listProperties = new ItemCollection<ListProperty>();
			var propertyToIndex = new Dictionary<string, int>();
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (!propertyInfo.DeclaringType.IsNotPublic)
				{
					if (propertyInfo.GetCustomAttribute<HiddenAttribute>() != null)
						continue;

					if (propertyInfo.GetCustomAttribute<HiddenRowAttribute>() != null)
						continue;

					if (propertyInfo.DeclaringType.IsNotPublic)
						continue;

					var listProperty = new ListProperty(obj, propertyInfo);

					// move this to later?
					if (propertyInfo.GetCustomAttribute<HideNullAttribute>() != null)
					{
						if (listProperty.Value == null)
							continue;
					}

					if (propertyToIndex.TryGetValue(propertyInfo.Name, out int index))
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

		// This can be slow due to lazy property loading
		public static ItemCollection<ListProperty> Sort(ItemCollection<ListProperty> listProperties)
		{
			var autoSorted = new ItemCollection<ListProperty>(listProperties.OrderByDescending(i => i.PropertyInfo.GetCustomAttribute<AutoSelectAttribute>() != null).ToList());
			var linkSorted = new ItemCollection<ListProperty>(autoSorted.OrderByDescending(i => TabModel.ObjectHasLinks(i, true)).ToList());
			return linkSorted;
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