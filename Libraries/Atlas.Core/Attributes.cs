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
			Name = name;
		}
	}

	// ->Tabs: Use the specified name instead of the field/property name
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class KeyAttribute : Attribute
	{
	}

	// ->Tabs: Use the specified name instead of the field/property name
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class DataKeyAttribute : Attribute
	{
	}

	// ->Tabs: Use the specified name instead of the field/property name
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
	public class DataValueAttribute : Attribute
	{
	}

	// ->Tabs: Use the specified name instead of the field/property name
	[AttributeUsage(AttributeTargets.Method)]
	public class VisibleAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.All)]
	public class PasswordCharAttribute : Attribute
	{
		public readonly char Character;

		public PasswordCharAttribute(char c)
		{
			Character = c;
		}
	}

	// [Example("0123456789abcdef")]
	[AttributeUsage(AttributeTargets.All)]
	public class ExampleAttribute : Attribute
	{
		public readonly string Text;

		public ExampleAttribute(string text)
		{
			Text = text;
		}
	}

	// [Description] conflicts with NUnit's, use [TabDescription]?
	/*[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class DescriptionAttribute : Attribute
	{
	}*/

	// [Summary("Text to describe object")], [Description] conflicts with NUnit's, use [TabDescription]?
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
	public class SummaryAttribute : Attribute
	{
		public readonly string Summary;

		public SummaryAttribute(string summary)
		{
			Summary = summary;
		}
	}

	// 
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class StyleValueAttribute : Attribute
	{
	}

	// 
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class WordWrapAttribute : Attribute
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
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
	public class UnserializedAttribute : Attribute
	{
	}

	// ->Serialize: Can't use [NonSerialized] since that's only for fields :(
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
	public class SecureAttribute : Attribute
	{
	}

	// ->Tabs: Don't show this field/property as a column
	[AttributeUsage(AttributeTargets.Method)]
	public class ButtonColumnAttribute : Attribute
	{
		public readonly string Name;

		public ButtonColumnAttribute(string name = null)
		{
			Name = name;
		}
	}

	// ->Tabs: Column Width should AutoSize instead of */Percent based
	[AttributeUsage(AttributeTargets.Property)]
	public class AutoSizeAttribute : Attribute
	{
	}

	// ->Tabs: Don't show this field/property as a column
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class HiddenColumnAttribute : Attribute
	{
	}

	// ->Tabs: Don't show this field/property as a row
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class HiddenRowAttribute : Attribute
	{
	}

	// ->Tabs: Allow Tab to be collapsed
	[AttributeUsage(AttributeTargets.Class)]
	public class SkippableAttribute : Attribute
	{
		public readonly bool Value;

		public SkippableAttribute(bool value = true)
		{
			Value = value;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class BindListAttribute : Attribute
	{
		public readonly string Name;

		public BindListAttribute(string name)
		{
			Name = name;
		}
	}

	// ->Tabs: Allows property to be edited in UI
	[AttributeUsage(AttributeTargets.Property)]
	public class EditingAttribute : Attribute
	{
	}

	// ->Tabs: Flag as the ToString() property/field? MaxDesiredWidthAttribute?
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class MinWidthAttribute : Attribute
	{
		public readonly int MinWidth;

		public MinWidthAttribute(int minWidth)
		{
			MinWidth = minWidth;
		}
	}

	// ->Tabs: Flag as the ToString() property/field? MaxDesiredWidthAttribute?
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class MaxWidthAttribute : Attribute
	{
		public readonly int MaxWidth;

		public MaxWidthAttribute(int maxWidth)
		{
			MaxWidth = maxWidth;
		}
	}

	// ->Tabs: Flag as the ToString() property/field?
	// ->Tabs: ToString() on items in array
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
	public class ToStringAttribute : Attribute
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

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class UnitAttribute : Attribute
	{
		public readonly string Name;

		public UnitAttribute(string name)
		{
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class XAxisAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class YAxisAttribute : Attribute
	{
	}

	// ->Toolbar: Show a separator before this item
	[AttributeUsage(AttributeTargets.Property)]
	public class SeparatorAttribute : Attribute
	{
	}
}
