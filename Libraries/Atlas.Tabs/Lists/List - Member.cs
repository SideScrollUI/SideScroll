using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Atlas.Tabs
{
	public abstract class ListMember : INotifyPropertyChanged
	{
		public const int MaxStringLength = 1000;

		public event PropertyChangedEventHandler PropertyChanged;
		public MemberInfo memberInfo;
		public object obj;
		public string Name { get; set; }

		[HiddenColumn]
		public virtual bool Editable { get { return true; } }
		public bool autoLoad = true;

		//[HiddenColumn]
		[StyleValue]
		[InnerValue]
		public abstract object Value { get; set; }

		[HiddenColumn]
		[Name("Value")]
		[StyleValue]
		[Editing]
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
						return value.ObjectToString();
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

		private void ListProperty_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
