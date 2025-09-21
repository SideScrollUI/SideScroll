namespace SideScroll.Attributes;

/// <summary>
/// Attributes that control class and struct behavior in SideScroll tab displays and navigation.
/// </summary>
/// <remarks>
/// <b>Display Control:</b> Use <see cref="ListItemAttribute"/> and <see cref="ToStringAttribute"/> 
/// to control how types are displayed in lists and collections.
/// <para>
/// <b>Tab Behavior:</b> Use <see cref="TabRootAttribute"/> to enable bookmarking and serialization, 
/// and <see cref="SkippableAttribute"/> to allow automatic tab collapsing.
/// </para>
/// </remarks>
internal static class _DocTabClassSentinel { }

/// <summary>
/// Displays all property names and [Item] methods as single-column list items.
/// </summary>
/// <param name="includeBaseTypes">Whether to include properties and methods from base types (default: false).</param>
/// <remarks>
/// <b>Apply to:</b> Classes or structs.
/// <para>
/// Creates a simplified list view showing each property name and decorated method as individual items 
/// in a single column layout. Useful for creating compact, scannable displays of object contents.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [ListItem(includeBaseTypes: true)]
/// public class MenuItem
/// {
///     public string Name { get; set; } = "";
///     public string Description { get; set; } = "";
///     
///     [Item]
///     public void Execute() { }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ListItemAttribute(bool includeBaseTypes = false) : Attribute
{
	/// <summary>
	/// Whether to include properties and methods from base types.
	/// </summary>
	public bool IncludeBaseTypes => includeBaseTypes;
}

/// <summary>
/// Shows individual collection members as a comma delimited list of ToString() results
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Classes or structs.
/// <para>
/// When applied to ICollection types, shows a comma delimited list of all the items ToString() results
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [ToString]
/// public class ProductList : List&lt;Product&gt;
/// {
///     public string Category { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ToStringAttribute : Attribute;

/// <summary>
/// Enables the tab to be bookmarked and serialized as a root-level tab.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Classes or structs.
/// <para>
/// Makes the tab eligible for direct bookmarking and enables tab serialization. 
/// Root tabs can be saved, restored, and shared as independent navigation points.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [TabRoot]
/// public class Dashboard
/// {
///     public string Title { get; set; } = "";
///     public List&lt;Widget&gt; Widgets { get; set; } = new();
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TabRootAttribute : Attribute;

/// <summary>
/// Allows the tab to be automatically collapsed when it contains only a single item.
/// </summary>
/// <param name="value">Whether to allow auto-collapsing (default: true).</param>
/// <remarks>
/// <b>Apply to:</b> Classes or structs.
/// <para>
/// When enabled, tabs containing only one item will be automatically collapsed to reduce 
/// UI clutter and provide a more streamlined navigation experience.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Skippable(true)]
/// public class SingleItemContainer
/// {
///     public ImportantData Data { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SkippableAttribute(bool value = true) : Attribute
{
	/// <summary>
	/// Whether to allow automatic tab collapsing.
	/// </summary>
	public bool Value => value;
}
