namespace SideScroll.Attributes;

// Use the specified name instead of the field/property name
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class NameAttribute(string name) : Attribute
{
	public readonly string Name = name;
}

// DataGrids use this as a unique key when matching rows
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DataKeyAttribute : Attribute;

// [DataValue] sets an inner value whose [DataKey] will be used if one is not set on the referencing class
// If the TabInstance.DataRepoInstance is set with elements that implement [DataValue], this value can also be passed in a bookmark 
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DataValueAttribute : Attribute;

// Put Serialize here so others don't have to reference a serializer directly?
// Serialize: Shows the field/property instead of the parent class as the nested tab
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InnerValueAttribute : Attribute;

// Show all object's members as extra rows
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InlineAttribute : Attribute;

// Don't show this field/property as a column or row
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HiddenAttribute : Attribute;

// Don't show this property as a column
[AttributeUsage(AttributeTargets.Property)]
public class HiddenColumnAttribute : Attribute;

// Don't show this field/property as a row
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HiddenRowAttribute : Attribute;

// Don't show row or column if any value matches
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
public class HideAttribute(object? value, params object?[] additonalValues) : Attribute
{
	// passing a null param passes a null array :(
	// Combine both params into a single list
	public readonly List<object?> Values = new(additonalValues)
	{
		value
	};
}

// Don't show row if any value matches
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HideRowAttribute(object? value, params object?[] additonalValues) : Attribute
{
	// passing a null param passes a null array :(
	// Combine both params into a single list
	public readonly List<object?> Values = new(additonalValues)
	{
		value
	};
}

// Don't show row if any value matches
[AttributeUsage(AttributeTargets.Property)]
public class HideColumnAttribute(object? value, params object?[] additonalValues) : Attribute
{
	// passing a null param passes a null array :(
	// Combine both params into a single list
	public readonly List<object?> Values = new(additonalValues)
	{
		value
	};
}

// Don't show unless #if DEBUG set
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DebugOnlyAttribute(bool value = true) : Attribute
{
	public readonly bool Value = value;
}

// Style value based on whether it contains links or not
[AttributeUsage(AttributeTargets.Property)]
public class StyleValueAttribute : Attribute;

// Round value when displaying (i.e. show TimeSpan as short value like "1.6 Days")
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FormattedAttribute : Attribute;

// Displayed string formatter
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FormatterAttribute(Type type) : Attribute
{
	public readonly Type Type = type;
}

// Adds spaces between words in a string
/*[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class WordSpacedAttribute : Attribute
{
}*/

// Wrap the text, Accepts return by default
[AttributeUsage(AttributeTargets.Property)]
public class WordWrapAttribute : Attribute;

// Right align contents in parent control
[AttributeUsage(AttributeTargets.Property)]
public class RightAlignAttribute : Attribute;

// Column Width should AutoSize instead of */Percent based
[AttributeUsage(AttributeTargets.Property)]
public class AutoSizeAttribute : Attribute;

// MinDesiredWidthAttribute? Actually allows user to use smaller values (a good thing)
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class MinWidthAttribute(int minWidth) : Attribute
{
	public readonly int MinWidth = minWidth;
}

// MaxDesiredWidthAttribute?
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class MaxWidthAttribute(int maxWidth) : Attribute
{
	public readonly int MaxWidth = maxWidth;
}

// MaxDesiredHeightAttribute?
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class MaxHeightAttribute(int maxHeight) : Attribute
{
	public readonly int MaxHeight = maxHeight;
}

// AutoSelect the item if non-null, rename? add priority?
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class AutoSelectAttribute : Attribute;

// Show method as an Item
[AttributeUsage(AttributeTargets.Method)]
public class ItemAttribute(string? name = null) : Attribute
{
	public readonly string? Name = name;
}

// Don't show this field/property as a column
[AttributeUsage(AttributeTargets.Method)]
public class ButtonColumnAttribute : Attribute
{
	public readonly string? Name;
	public readonly string? VisiblePropertyName;

	public ButtonColumnAttribute(string? name = null)
	{
		Name = name;
	}

	public ButtonColumnAttribute(string name, string visiblePropertyName)
	{
		Name = name;
		VisiblePropertyName = visiblePropertyName;
	}
}

// -> Allows setting the column for param controls
[AttributeUsage(AttributeTargets.Property)]
public class ColumnIndexAttribute(int index) : Attribute
{
	public readonly int Index = index;
}
