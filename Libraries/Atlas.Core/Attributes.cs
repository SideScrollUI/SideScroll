using System;

namespace Atlas.Core
{
	// ->Tabs: Use the specified name instead of the field/property name
	[AttributeUsage(AttributeTargets.All)]
	public class NameAttribute : Attribute
	{
		public readonly string Name;

		public NameAttribute(string name)
		{
			this.Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.All)]
	public class PasswordCharAttribute : Attribute
	{
		public readonly char Character;

		public PasswordCharAttribute(char c)
		{
			this.Character = c;
		}
	}

	// [Example("0123456789abcdef")]
	[AttributeUsage(AttributeTargets.All)]
	public class ExampleAttribute : Attribute
	{
		public readonly string Text;

		public ExampleAttribute(string text)
		{
			this.Text = text;
		}
	}

	// 
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class StyleValueAttribute : Attribute
	{
	}

	// Put Serialize here so others don't have to reference a serializer directly?
	// Serialize: Shows the field/property instead of the parent class as the nested tab
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class InnerValueAttribute : Attribute
	{
	}

	// Params data
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ParamsAttribute : Attribute
	{
	}

	/*[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class ReadOnlyAttribute : Attribute
	{
	}*/

	// ->Serialize: When Cloning an object, anything marked with [Static] won't be deep copied
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
	public class StaticAttribute : Attribute
	{
	}

	// ->Serialize: Can't use [NonSerialized] since that's only for fields :(
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class UnserializedAttribute : Attribute
	{
	}

	// ->Tabs: Don't show this field/property as a column
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class HiddenColumnAttribute : Attribute
	{
	}

	// ->Tabs: Don't show this field/property as a column
	[AttributeUsage(AttributeTargets.Method)]
	public class ButtonColumnAttribute : Attribute
	{
		public readonly string Name;

		public ButtonColumnAttribute(string name = null)
		{
			this.Name = name;
		}
	}

	// ->Tabs: Don't show this field/property as a row
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class HiddenRowAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class BindListAttribute : Attribute
	{
		public readonly string Name;

		public BindListAttribute(string name)
		{
			this.Name = name;
		}
	}

	// ->Tabs: Allows property to be edited in GUI
	[AttributeUsage(AttributeTargets.Property)]
	public class EditingAttribute : Attribute
	{
	}

	// ->Tabs: Flag as the ToString() property/field?
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class AttributeToString : Attribute
	{
	}

	// ->Tabs: 
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class AttributeSelectable : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class InheritAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class DictionaryEntryAttribute : Attribute
	{
		public readonly string key;
		public readonly string value;

		public DictionaryEntryAttribute(string key, string value)
		{
			this.key = key;
			this.value = value;
		}
	}

	[AttributeUsage(AttributeTargets.All)]
	public class UnitAttribute : Attribute
	{
		public readonly string Name;

		public UnitAttribute(string name)
		{
			this.Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.All)]
	public class XAxisAttribute : Attribute
	{
	}
}
