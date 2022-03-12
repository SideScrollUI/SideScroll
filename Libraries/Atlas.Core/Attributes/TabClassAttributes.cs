using System;

namespace Atlas.Core;

// Params data
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ParamsAttribute : Attribute
{
}

// Shows all property names and [Item] methods as single column ListItem's
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ListItemAttribute : Attribute
{
	public readonly bool IncludeBaseTypes;

	public ListItemAttribute(bool includeBaseTypes = false)
	{
		IncludeBaseTypes = includeBaseTypes;
	}
}

// ToString() on items in an array instead of showing item properties
// Allow on property/field as a ToString() alternative?
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ToStringAttribute : Attribute
{
}

// Tab is rootable for bookmarks, also serializes tab
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TabRootAttribute : Attribute
{
}

// Allow Tab to be auto-collapsed if there's only a single item
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SkippableAttribute : Attribute
{
	public readonly bool Value;

	public SkippableAttribute(bool value = true)
	{
		Value = value;
	}
}

// [Summary("Text to describe object")], [Description] conflicts with NUnit's, use [TabDescription]?
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SummaryAttribute : Attribute
{
	public readonly string Summary;

	public SummaryAttribute(string summary)
	{
		Summary = summary;
	}
}
