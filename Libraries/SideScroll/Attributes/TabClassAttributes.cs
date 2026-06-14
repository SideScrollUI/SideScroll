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
/// Enables searching child items when filtering. By default, only top-level items are searched.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Classes, structs, or properties.
/// <para>
/// When applied to an item type or a property, the filter will also search nested child items.
/// The search depth is controlled by <c>TabModel.MaxSearchDepth</c> on the starting tab.
/// Without this attribute, filtering only searches the immediate items in the list.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Searchable]
/// public class Order
/// {
///     public string Name { get; set; } = "";
///     public List&lt;OrderItem&gt; Items { get; set; } = new();
/// }
/// 
/// public class Invoice
/// {
///     public string Number { get; set; } = "";
/// 
///     [Searchable]
///     public Address BillingAddress { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property)]
public class SearchableAttribute : Attribute;

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

/// <summary>
/// Signals whether a type may be fully explored during schema / headless traversal.
/// </summary>
/// <param name="value">Whether the type is fully explored (default: true).</param>
/// <remarks>
/// <b>Apply to:</b> Classes or structs (used as item-list element types).
/// <para>
/// Overrides the headless viewer's element-type allowlist for this type:
/// <c>[Explorable]</c> opts a type in to full exploration (items and children) even when it is
/// not in the allowlist, and <c>[Explorable(false)]</c> opts it out (only sampled) even when it
/// is. Without the attribute, allowlist membership decides. The visible property columns are
/// always shown regardless.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Explorable]
/// public class MenuItem
/// {
///     public string Name { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ExplorableAttribute(bool value = true) : Attribute
{
	/// <summary>
	/// Whether the type may be fully explored during schema / headless traversal.
	/// </summary>
	public bool Value => value;
}
