﻿using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Atlas.Tabs
{
	public interface IListAutoSelect
	{
		int Order { get; }
	}

	public interface IListPair
	{
		[Name("Name"), StyleLabel]
		object Key { get; }

		[InnerValue, StyleValue]
		object Value { get; set; }
	}

	public interface IMaxDesiredWidth
	{
		int? MaxDesiredWidth { get; }
	}

	public interface IMaxDesiredHeight
	{
		int? MaxDesiredHeight { get; }
	}

	public abstract class ListMember : IListPair, IListItem, INotifyPropertyChanged, IListAutoSelect, IMaxDesiredWidth, IMaxDesiredHeight
	{
		public const int MaxStringLength = 1000;
		private const int DefaultMaxDesiredHeight = 500;

		public event PropertyChangedEventHandler PropertyChanged;

		public MemberInfo MemberInfo;

		public object Object;

		[StyleLabel]
		public string Name { get; set; }

		[HiddenColumn]
		public object Key => Name;

		[Hidden]
		public int Order { get; set; } = 0;

		[HiddenColumn]
		public virtual bool Editable => true;

		public bool AutoLoad = true;

		[HiddenColumn]
		public int? MaxDesiredWidth => GetCustomAttribute<MaxWidthAttribute>()?.MaxWidth;

		[HiddenColumn]
		public int? MaxDesiredHeight => GetCustomAttribute<MaxHeightAttribute>()?.MaxHeight ?? DefaultMaxDesiredHeight;

		//[HiddenColumn]
		[StyleValue, InnerValue, WordWrap]
		public abstract object Value { get; set; }

		[HiddenColumn, Name("Value"), StyleValue, Editing]
		public object ValueText
		{
			get
			{
				try
				{
					object value = Value;
					if (value == null)
					{
						return null;
					}
					else if (value is string text)
					{
						if (text.Length > MaxStringLength)
							return text.Substring(0, MaxStringLength);
					}
					else if (!value.GetType().IsPrimitive)
					{
						return value.Formatted();
					}
					return value;
				}
				catch (Exception)
				{
					return null;
				}
			}
			set
			{
				// hide this for Avalonia
				//if (memberInfo.GetType().Has)
				Value = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueText)));
			}
		}

		public override string ToString() => Name;

		public ListMember(object obj, MemberInfo memberInfo)
		{
			Object = obj;
			MemberInfo = memberInfo;
			
			if (obj is INotifyPropertyChanged notifyPropertyChanged)
				notifyPropertyChanged.PropertyChanged += ListProperty_PropertyChanged;
		}

		private void ListProperty_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}


		public T GetCustomAttribute<T>() where T : Attribute
		{
			return MemberInfo.GetCustomAttribute<T>();
		}

		public static ItemCollection<ListMember> Sort(ItemCollection<ListMember> items)
		{
			var autoSorted = new ItemCollection<ListMember>(items.OrderByDescending(i => i.MemberInfo.GetCustomAttribute<AutoSelectAttribute>() != null).ToList());

			var linkSorted = new ItemCollection<ListMember>(autoSorted.OrderByDescending(i => TabUtils.ObjectHasLinks(i, true)).ToList());

			return linkSorted;
		}

		public static ItemCollection<ListMember> Create(object obj)
		{
			var listMembers = new ItemCollection<ListMember>();

			// Add in correct order?
			// ListItem.Create() has correct order by using property get, but doesn't work with fields
			//var members = obj.GetType().GetMembers();

			var listProperties = ListProperty.Create(obj);
			listMembers.AddRange(listProperties);

			var listFields = ListField.Create(obj);
			listMembers.AddRange(listFields);

			var listMethods = ListMethod.Create(obj); // order doesn't match up with properties and fields
			listMembers.AddRange(listMethods);

			//listMembers = itemCollection.OrderBy(x => x.MemberInfo.MetadataToken).ToList();

			return listMembers;
		}
	}
}
