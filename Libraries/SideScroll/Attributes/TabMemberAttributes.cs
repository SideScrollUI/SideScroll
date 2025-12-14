namespace SideScroll.Attributes;

/// <summary>
/// Attributes that control how SideScroll displays and formats data members in tabs and UI controls.
/// </summary>
/// <remarks>
/// <b>Display Control:</b> Use <see cref="NameAttribute"/>, <see cref="HiddenAttribute"/>, and visibility attributes 
/// to control what gets shown and how it appears.
/// <para>
/// <b>Data Binding:</b> Use <see cref="DataKeyAttribute"/> and <see cref="DataValueAttribute"/> for grid row matching 
/// and data repository integration.
/// </para>
/// <para>
/// <b>Layout:</b> Use sizing and alignment attributes to control column widths, text wrapping, and positioning.
/// </para>
/// <para>
/// <b>Formatting:</b> Use <see cref="FormattedAttribute"/> and <see cref="FormatterAttribute"/> to customize 
/// how values are displayed.
/// </para>
/// </remarks>
internal static class _DocTabMemberSentinel { }

/// <summary>
/// Specifies a custom display name for fields, properties, or methods.
/// </summary>
/// <param name="name">The display name to use instead of the member name.</param>
/// <remarks>
/// <b>Apply to:</b> Fields, properties, or methods.
/// <para>
/// Overrides the default member name in UI displays, column headers, and labels.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Person
/// {
///     [Name("Full Name")]
///     public string FullName { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class NameAttribute(string name) : Attribute
{
	/// <summary>
	/// The display name to use for this member.
	/// </summary>
	public string Name => name;
}

/// <summary>
/// Marks a field or property as a unique key for DataRepo and DataGrid row matching.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// DataRepos and DataGrids use this as a unique identifier when matching and updating items.
/// Essential for proper data binding and row identification.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Product
/// {
///     [DataKey]
///     public int Id { get; set; }
///     public string Name { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DataKeyAttribute : Attribute;

/// <summary>
/// Marks a field or property as the inner data value that this class represents for linking.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// Sets an inner value whose <see cref="DataKeyAttribute"/> will be used if one is not set on the referencing class.
/// If TabInstance.DataRepoInstance contains elements with DataValue, this value can be passed in links.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class OrderItem
/// {
///     [DataValue]
///     public Product Product { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DataValueAttribute : Attribute;

/// <summary>
/// Shows the field or property value as the nested tab instead of the parent class.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// When navigating to nested objects, displays the marked member's content directly 
/// rather than showing the parent object's properties.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Container
/// {
///     [InnerValue]
///     public List&lt;Item&gt; Items { get; set; } = new();
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InnerValueAttribute : Attribute;

/// <summary>
/// Displays all object members as additional rows in the current view.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// Instead of showing the object as a nested item, expands all its members 
/// inline as extra rows in the current display.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Order
/// {
///     [Inline]
///     public Address ShippingAddress { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InlineAttribute : Attribute;

/// <summary>
/// Hides the field or property from both column and row displays.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// Completely excludes the member from UI display while keeping it available for data operations.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class User
/// {
///     [Hidden]
///     public string InternalId { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HiddenAttribute : Attribute;

/// <summary>
/// Hides the property from column displays only.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Excludes the property from column views while still showing it in row-based displays.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Report
/// {
///     [HiddenColumn]
///     public string DetailedNotes { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class HiddenColumnAttribute : Attribute;

/// <summary>
/// Hides the field or property from row displays only.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// Excludes the member from row-based views while still showing it in column displays.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Summary
/// {
///     [HiddenRow]
///     public int Count { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HiddenRowAttribute : Attribute;

/// <summary>
/// Hides rows or columns when the member value matches any of the specified values.
/// </summary>
/// <param name="value">Primary value to match for hiding.</param>
/// <param name="additionalValues">Additional values that trigger hiding.</param>
/// <remarks>
/// <b>Apply to:</b> Fields, properties, classes, or structs.
/// <para>
/// When applied to types, affects all members of that type (individual members can override).
/// Useful for hiding empty, default, or unwanted values from display.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Status
/// {
///     [Hide(null, "", "Unknown")]
///     public string Description { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class HideAttribute(object? value, params object?[] additionalValues) : Attribute
{
	/// <summary>
	/// All values that trigger hiding when matched.
	/// </summary>
	public object?[] Values { get; } =
	[
		value,
		.. additionalValues
	];
}

/// <summary>
/// Hides rows when the member value matches any of the specified values.
/// </summary>
/// <param name="value">Primary value to match for hiding rows.</param>
/// <param name="additionalValues">Additional values that trigger row hiding.</param>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// Similar to <see cref="HideAttribute"/> but only affects row displays, 
/// leaving column displays unaffected.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Item
/// {
///     [HideRow(0, -1)]
///     public int Quantity { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HideRowAttribute(object? value, params object?[] additionalValues) : Attribute
{
	/// <summary>
	/// All values that trigger row hiding when matched.
	/// </summary>
	public object?[] Values { get; } =
	[
		value,
		.. additionalValues
	];
}

/// <summary>
/// Hides columns when the member value matches any of the specified values.
/// </summary>
/// <param name="value">Primary value to match for hiding columns.</param>
/// <param name="additionalValues">Additional values that trigger column hiding.</param>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Similar to <see cref="HideAttribute"/> but only affects column displays, 
/// leaving row displays unaffected.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Config
/// {
///     [HideColumn(false)]
///     public bool IsEnabled { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class HideColumnAttribute(object? value, params object?[] additionalValues) : Attribute
{
	/// <summary>
	/// All values that trigger column hiding when matched.
	/// </summary>
	public object?[] Values { get; } =
	[
		value,
		.. additionalValues
	];
}

/// <summary>
/// Shows the member only in DEBUG builds.
/// </summary>
/// <param name="value">Whether to show in debug builds (default: true).</param>
/// <remarks>
/// <b>Apply to:</b> Fields, properties, classes, or structs.
/// <para>
/// When applied to types, affects all members of that type (individual members can override).
/// Useful for development-only information that shouldn't appear in release builds.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class DiagnosticInfo
/// {
///     [DebugOnly]
///     public string InternalState { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class DebugOnlyAttribute(bool value = true) : Attribute
{
	/// <summary>
	/// Whether to show this member in debug builds.
	/// </summary>
	public bool Value => value;
}

/// <summary>
/// Applies special styling to DataGrid cells based on whether the value contains objects with data.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Automatically detects and styles DataGrid cells that contain objects with data
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Document
/// {
///     [StyleValue]
///     public string Content { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class StyleValueAttribute : Attribute;

/// <summary>
/// Applies automatic formatting to display values in a more readable form.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// Rounds and formats values for display (e.g., shows TimeSpan as "1.6 Days" instead of raw ticks).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Performance
/// {
///     [Formatted]
///     public TimeSpan Duration { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FormattedAttribute : Attribute;

/// <summary>
/// Specifies a custom formatter type for displaying string values.
/// </summary>
/// <param name="type">The formatter type to use for string conversion.</param>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// Provides complete control over how values are converted to display strings.
/// The formatter type should implement the appropriate formatting interface.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Currency
/// {
///     [Formatter(typeof(CurrencyFormatter))]
///     public decimal Amount { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FormatterAttribute(Type type) : Attribute
{
	/// <summary>
	/// The formatter type used for string conversion.
	/// </summary>
	public Type Type => type;
}

/// <summary>
/// Enables text wrapping.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Allows text to wrap within the display area.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Comment
/// {
///     [WordWrap]
///     public string Text { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class WordWrapAttribute : Attribute;

/// <summary>
/// Right-aligns controls within toolbars.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Only works on toolbar controls to position them on the right side of the toolbar.
/// Useful for actions or controls that should appear at the end of the toolbar.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class ToolbarActions
/// {
///     [RightAlign]
///     public string SearchBox { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class RightAlignAttribute : Attribute;

/// <summary>
/// Sets column width to auto-size based on content instead of percentage-based sizing.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Column width adjusts automatically to fit content rather than using fixed or percentage widths.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Product
/// {
///     [AutoSize]
///     public string Code { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class AutoSizeAttribute : Attribute;

/// <summary>
/// Sets the minimum desired width for the member's display.
/// </summary>
/// <param name="minWidth">Minimum width in pixels.</param>
/// <remarks>
/// <b>Apply to:</b> Fields, properties, or methods.
/// <para>
/// Users can still resize smaller if needed, but this sets the preferred minimum width.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Description
/// {
///     [MinWidth(200)]
///     public string Text { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class MinWidthAttribute(int minWidth) : Attribute
{
	/// <summary>
	/// The minimum width in pixels.
	/// </summary>
	public int MinWidth => minWidth;
}

/// <summary>
/// Sets the maximum desired width for the member's display.
/// </summary>
/// <param name="maxWidth">Maximum width in pixels.</param>
/// <remarks>
/// <b>Apply to:</b> Fields, properties, or methods.
/// <para>
/// Prevents the display from growing beyond the specified width.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Summary
/// {
///     [MaxWidth(300)]
///     public string Title { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class MaxWidthAttribute(int maxWidth) : Attribute
{
	/// <summary>
	/// The maximum width in pixels.
	/// </summary>
	public int MaxWidth => maxWidth;
}

/// <summary>
/// Sets the maximum desired height for the member's display.
/// </summary>
/// <param name="maxHeight">Maximum height in pixels.</param>
/// <remarks>
/// <b>Apply to:</b> Fields, properties, or methods.
/// <para>
/// Prevents the display from growing beyond the specified height.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class TextArea
/// {
///     [MaxHeight(150)]
///     public string Content { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class MaxHeightAttribute(int maxHeight) : Attribute
{
	/// <summary>
	/// The maximum height in pixels.
	/// </summary>
	public int MaxHeight => maxHeight;
}

/// <summary>
/// Automatically selects the item if it has a non-null value.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// When displaying lists or collections, automatically highlights/selects items 
/// with non-null values in the marked member.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class ListItem
/// {
///     [AutoSelect]
///     public bool IsDefault { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class AutoSelectAttribute : Attribute;

/// <summary>
/// Displays a method as a selectable item with an optional custom name.
/// </summary>
/// <param name="name">Optional display name for the method (uses method name if null).</param>
/// <remarks>
/// <b>Apply to:</b> Methods.
/// <para>
/// Makes methods appear as interactive items in the UI, allowing users to invoke them.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Actions
/// {
///     [Item("Refresh Data")]
///     public void RefreshData() { }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method)]
public class ItemAttribute(string? name = null) : Attribute
{
	/// <summary>
	/// The display name for the method item.
	/// </summary>
	public string? Name => name;
}

/// <summary>
/// Displays a method as a button column with optional visibility control.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Methods.
/// <para>
/// Creates a button column that invokes the method when clicked. 
/// Optionally controlled by a visibility property.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class RowActions
/// {
///     [ButtonColumn("Delete", "CanDelete")]
///     public void Delete() { }
///     
///     public bool CanDelete { get; set; } = true;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method)]
public class ButtonColumnAttribute : Attribute
{
	/// <summary>
	/// The display name for the button.
	/// </summary>
	public string? Name { get; init; }
	
	/// <summary>
	/// Property name that controls button visibility.
	/// </summary>
	public string? VisiblePropertyName { get; init; }

	/// <summary>
	/// Creates a button column with optional name.
	/// </summary>
	/// <param name="name">Button display name.</param>
	public ButtonColumnAttribute(string? name = null)
	{
		Name = name;
	}

	/// <summary>
	/// Creates a button column with name and visibility control.
	/// </summary>
	/// <param name="name">Button display name.</param>
	/// <param name="visiblePropertyName">Property that controls visibility.</param>
	public ButtonColumnAttribute(string name, string visiblePropertyName)
	{
		Name = name;
		VisiblePropertyName = visiblePropertyName;
	}
}

/// <summary>
/// Sets the column index for tab form layouts.
/// </summary>
/// <param name="index">The column index (should be in multiples of 2 for label + control pairs).</param>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Controls column positioning in tab forms. Use multiples of 2 since each property 
/// typically uses two columns (one for label, one for control).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class FormData
/// {
///     [ColumnIndex(0)]
///     public string FirstName { get; set; } = "";
///     
///     [ColumnIndex(2)]
///     public string LastName { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class ColumnIndexAttribute(int index) : Attribute
{
	/// <summary>
	/// The column index for form layout.
	/// </summary>
	public int Index => index;
}
