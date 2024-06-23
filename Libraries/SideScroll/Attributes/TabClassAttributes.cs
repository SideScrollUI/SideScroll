namespace SideScroll.Attributes;

// Params data
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ParamsAttribute : Attribute;

// Shows all property names and [Item] methods as single column ListItem's
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ListItemAttribute(bool includeBaseTypes = false) : Attribute
{
	public readonly bool IncludeBaseTypes = includeBaseTypes;
}

// ICollection's that specify this will show individual members in Formatted()
// Instead of calling ICollection's ToString()
// Allow on property/field as a ToString() alternative?
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ToStringAttribute : Attribute;

// Tab is rootable for bookmarks, also serializes tab
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TabRootAttribute : Attribute;

// Allow Tab to be auto-collapsed if there's only a single item
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SkippableAttribute(bool value = true) : Attribute
{
	public readonly bool Value = value;
}

// [Summary("Text to describe object")], [Description] conflicts with NUnit's, use [TabDescription]?
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SummaryAttribute(string summary) : Attribute
{
	public readonly string Summary = summary;
}
