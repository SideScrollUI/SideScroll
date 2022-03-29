using System;
using System.Collections.Generic;

namespace Atlas.Core;

// Use the specified name instead of the field/property name
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
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
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DataValueAttribute : Attribute
{
}

// Put Serialize here so others don't have to reference a serializer directly?
// Serialize: Shows the field/property instead of the parent class as the nested tab
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InnerValueAttribute : Attribute
{
}
// Don't show this field/property as a column or row
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HiddenAttribute : Attribute
{
}

// Don't show this property as a column
[AttributeUsage(AttributeTargets.Property)]
public class HiddenColumnAttribute : Attribute
{
}

// Don't show this field/property as a row
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HiddenRowAttribute : Attribute
{
}

// Don't show row or column if any value matches
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HideAttribute : Attribute
{
	public readonly List<object> Values;

	// passing a null param passes a null array :(
	public HideAttribute(object value, params object[] additonalValues)
	{
		// Combine both params into a single list
		Values = new List<object>(additonalValues)
		{
			value
		};
	}
}

// Don't show row if any value matches
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HideRowAttribute : Attribute
{
	public readonly List<object> Values;

	// passing a null param passes a null array :(
	public HideRowAttribute(object value, params object[] additonalValues)
	{
		// Combine both params into a single list
		Values = new List<object>(additonalValues)
		{
			value
		};
	}
}

// Don't show row if any value matches
[AttributeUsage(AttributeTargets.Property)]
public class HideColumnAttribute : Attribute
{
	public readonly List<object> Values;

	// passing a null param passes a null array :(
	public HideColumnAttribute(object value, params object[] additonalValues)
	{
		// Combine both params into a single list
		Values = new List<object>(additonalValues)
		{
			value
		};
	}
}

// Don't show unless #if DEBUG set
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DebugOnlyAttribute : Attribute
{
	public readonly bool Value;

	public DebugOnlyAttribute(bool value = true)
	{
		Value = value;
	}
}

// Style a Column to use the same color as the header
[AttributeUsage(AttributeTargets.Property)]
public class StyleLabelAttribute : Attribute
{
}

// Style value based on whether it contains links or not
[AttributeUsage(AttributeTargets.Property)]
public class StyleValueAttribute : Attribute
{
}

// Round value when displaying (i.e. show TimeSpan as short value like "1.6 Days")
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FormattedAttribute : Attribute
{
}

// Displayed string formatter
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FormatterAttribute : Attribute
{
	public readonly Type Type;

	public FormatterAttribute(Type type)
	{
		Type = type;
	}
}

// Adds spaces between words in a string
/*[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class WordSpacedAttribute : Attribute
{
}*/

// Wrap the text, Accepts return by default
[AttributeUsage(AttributeTargets.Property)]
public class WordWrapAttribute : Attribute
{
}

// Right align contents in parent control
[AttributeUsage(AttributeTargets.Property)]
public class RightAlignAttribute : Attribute
{
}

// Column Width should AutoSize instead of */Percent based
[AttributeUsage(AttributeTargets.Property)]
public class AutoSizeAttribute : Attribute
{
}

// MinDesiredWidthAttribute? Actually allows user to use smaller values (a good thing)
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class MinWidthAttribute : Attribute
{
	public readonly int MinWidth;

	public MinWidthAttribute(int minWidth)
	{
		MinWidth = minWidth;
	}
}

// MaxDesiredWidthAttribute?
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

// AutoSelect the item if non-null, rename? add priority?
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class AutoSelectAttribute : Attribute
{
}

// Show method as an Item
[AttributeUsage(AttributeTargets.Method)]
public class ItemAttribute : Attribute
{
	public readonly string Name;

	public ItemAttribute(string name = null)
	{
		Name = name;
	}
}

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
