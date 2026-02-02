using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using System.Collections;

namespace SideScroll.Tabs;

/// <summary>
/// Overrides default ObjectHasLinks()
/// </summary>
public interface IHasLinks
{
	/// <summary>
	/// Indicates whether this object contains linkable content
	/// Objects that have links will show in a different color and show sub Tabs
	/// </summary>
	bool HasLinks { get; }
}

/// <summary>
/// Utility methods for working with tab objects and links
/// </summary>
public static class TabUtils
{
	/// <summary>
	/// Types that should be ignored when checking for links when ignoreEmpty is true
	/// </summary>
	public static List<Type> IgnoreHighlightTypes { get; set; } = [];

	/// <summary>
	/// Determines whether an object contains linkable content or nested objects.
	/// Objects that have links will show in a different color and show sub Tabs.
	/// Returns false for primitive types (int, bool, etc.), enums, strings, decimals, DateTime, and TimeSpan.
	/// Returns true for complex types like classes, collections, and other objects that can be navigated into.
	/// </summary>
	/// <param name="obj">The object to check for linkable content</param>
	/// <param name="ignoreEmpty">If true, empty collections and types in IgnoreHighlightTypes will return false</param>
	public static bool ObjectHasLinks(object? obj, bool ignoreEmpty = false)
	{
		if (obj == null)
			return false;

		if (obj is IHasLinks hasLinks)
			return hasLinks.HasLinks;

		object? value = obj.GetInnerValue();
		if (value == null)
			return false;

		if (value is IListItem listItem)
			value = listItem.Value;

		Type type = value!.GetType();
		if (type.IsPrimitive ||
			type.IsEnum ||
			type == typeof(string) ||
			type == typeof(decimal) ||
			type == typeof(DateTime) ||
			type == typeof(TimeSpan))
		{
			return false;
		}

		if (ignoreEmpty)
		{
			if (value is ICollection collection)
			{
				if (collection.Count == 0)
					return false;
			}

			foreach (Type ignoreType in IgnoreHighlightTypes)
			{
				if (ignoreType.IsAssignableFrom(type))
					return false;
			}
		}

		return true;
	}
}
