﻿using Atlas.Core;
using Atlas.Extensions;
using System;
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
		[Name("Name")]
		object Key { get; }

		[InnerValue, StyleValue]
		object Value { get; set; }
	}

	public abstract class ListMember : IListPair, IListItem, INotifyPropertyChanged, IListAutoSelect
	{
		public const int MaxStringLength = 1000;

		public event PropertyChangedEventHandler PropertyChanged;
		public MemberInfo memberInfo;
		public object obj;
		public string Name { get; set; }

		[HiddenColumn]
		public object Key => Name;

		[HiddenColumn, HiddenRow]
		public int Order { get; set; } = 0;

		[HiddenColumn]
		public virtual bool Editable => true;
		public bool autoLoad = true;

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
					else if (value is string)
					{
						string text = (string)value;
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

		public ListMember(object obj, MemberInfo memberInfo)
		{
			this.obj = obj;
			this.memberInfo = memberInfo;
			
			if (obj is INotifyPropertyChanged)
				(obj as INotifyPropertyChanged).PropertyChanged += ListProperty_PropertyChanged;
		}

		public override string ToString()
		{
			return Name;
		}

		private void ListProperty_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}

		public static ItemCollection<ListMember> Sort(ItemCollection<ListMember> items)
		{
			var sorted = new ItemCollection<ListMember>(items.OrderByDescending(i => TabModel.ObjectHasChildren(i, true)).ToList());
			return sorted;
		}
	}
}
