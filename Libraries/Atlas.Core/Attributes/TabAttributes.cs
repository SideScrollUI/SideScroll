using System;
using System.Collections.Generic;

namespace Atlas.Core
{
	// Use the specified name instead of the field/property name
	[AttributeUsage(AttributeTargets.All)]
	public class NameAttribute : Attribute
	{
		public readonly string Name;

		public NameAttribute(string name)
		{
			Name = name;
		}
	}

	// DataGrids use this as a unique key when matching rows
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class DataKeyAttribute : Attribute
	{
	}

	// DataGrids use this as a unique key when matching rows
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
	public class DataValueAttribute : Attribute
	{
	}

	// If set, this method will appear as an Action (rename to [Action]?)
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

	// Style a Column to use the same color as the header
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class StyleLabelAttribute : Attribute
	{
	}

	// Style value based on whether it contains links or not
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class StyleValueAttribute : Attribute
	{
	}

	// 
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class WordSpacedAttribute : Attribute
	{
	}

	// 
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class WordWrapAttribute : Attribute
	{
	}

	// Round value when displaying (i.e. show TimeSpan as short value like "1.6 Days")
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class FormattedAttribute : Attribute
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

	// Don't show this field/property as a column
	[AttributeUsage(AttributeTargets.Method)]
	public class ButtonColumnAttribute : Attribute
	{
		public readonly string Name;
		public readonly string VisiblePropertyName;

		public ButtonColumnAttribute(string name = null)
		{
			Name = name;
		}

		public ButtonColumnAttribute(string name, string visiblePropertyName)
		{
			Name = name;
			VisiblePropertyName = visiblePropertyName;
		}
	}

	// Column Width should AutoSize instead of */Percent based
	[AttributeUsage(AttributeTargets.Property)]
	public class AutoSizeAttribute : Attribute
	{
	}

	// AutoSelect the item if non-null, rename? add priority?
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class AutoSelectAttribute : Attribute
	{
	}

	// Don't show this field/property as a column or row
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class HiddenAttribute : Attribute
	{
	}

	// Don't show this field/property as a column
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class HiddenColumnAttribute : Attribute
	{
	}

	// Don't show this field/property as a row
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class HiddenRowAttribute : Attribute
	{
	}

	// Don't show row if value is null
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class HideNullAttribute : Attribute
	{
	}

	// Don't show row if value matches
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class HideAttribute : Attribute
	{
		public readonly List<object> Values;

		// passing a null param passes a null array :(
		public HideAttribute(object value, params object[] additonalValues)
		{
			// Combine both params into a single list
			Values = new List<object>(additonalValues);
			Values.Add(value);
		}
	}

	// Allow Tab to be collapsed
	[AttributeUsage(AttributeTargets.Class)]
	public class SkippableAttribute : Attribute
	{
		public readonly bool Value;

		public SkippableAttribute(bool value = true)
		{
			Value = value;
		}
	}

	// Don't show unless #if DEBUG set
	[AttributeUsage(AttributeTargets.Property)]
	public class DebugOnlyAttribute : Attribute
	{
		public readonly bool Value;

		public DebugOnlyAttribute(bool value = true)
		{
			Value = value;
		}
	}

	// Tab is rootable for bookmarks, also serializes tab
	[AttributeUsage(AttributeTargets.Class)]
	public class TabRootAttribute : Attribute
	{
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

	// Allows property to be edited in UI
	[AttributeUsage(AttributeTargets.Property)]
	public class EditingAttribute : Attribute
	{
	}

	// Flag as the ToString() property/field? MaxDesiredWidthAttribute?
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class MinWidthAttribute : Attribute
	{
		public readonly int MinWidth;

		public MinWidthAttribute(int minWidth)
		{
			MinWidth = minWidth;
		}
	}

	// Flag as the ToString() property/field? MaxDesiredWidthAttribute?
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class MaxWidthAttribute : Attribute
	{
		public readonly int MaxWidth;

		public MaxWidthAttribute(int maxWidth)
		{
			MaxWidth = maxWidth;
		}
	}

	// MaxDesiredHeightAttribute?
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class MaxHeightAttribute : Attribute
	{
		public readonly int MaxHeight;

		public MaxHeightAttribute(int maxHeight)
		{
			MaxHeight = maxHeight;
		}
	}

	// Flag as the ToString() property/field?
	// ToString() on items in array
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
	public class ToStringAttribute : Attribute
	{
	}

	// 
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class AttributeSelectable : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class InheritAttribute : Attribute
	{
	}

	// Show method as an Item
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class ItemAttribute : Attribute
	{
		public readonly string Name;

		public ItemAttribute(string name = null)
		{
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class DictionaryEntryAttribute : Attribute
	{
		public readonly string Key;
		public readonly string Value;

		public DictionaryEntryAttribute(string key, string value)
		{
			Key = key;
			Value = value;
		}
	}

	// ->Toolbar: Show a separator before this item
	[AttributeUsage(AttributeTargets.Property)]
	public class SeparatorAttribute : Attribute
	{
	}
}
