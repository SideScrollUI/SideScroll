using SideScroll.Attributes;
using System.Reflection;

namespace SideScroll.Extensions;

/// <summary>
/// Extension methods for MemberInfo, FieldInfo, and PropertyInfo to determine visibility in data displays
/// </summary>
public static class MemberExtensions
{
	/// <summary>
	/// Determines whether a field should be visible as a row in data displays (excludes constants, debug-only fields, and [Hidden]/[HiddenRow] fields)
	/// </summary>
	public static bool IsRowVisible(this FieldInfo fieldInfo)
	{
		// IsLiteral alone, a const is never also readonly so !IsInitOnly was always true
		if (fieldInfo.IsLiteral)
			return false;

#if !DEBUG
			if (fieldInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return fieldInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			fieldInfo.GetCustomAttribute<HiddenRowAttribute>() == null; // [HiddenRow]
	}

	/// <summary>
	/// Determines whether a property should be visible as a row in data displays (excludes non-public types, debug-only properties, and [Hidden]/[HiddenRow] properties)
	/// </summary>
	public static bool IsRowVisible(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.DeclaringType!.IsNotPublic || propertyInfo.GetMethod?.IsPublic != true)
			return false;

#if !DEBUG
			if (propertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return propertyInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			propertyInfo.GetCustomAttribute<HiddenRowAttribute>() == null; // [HiddenRow]
	}

	/// <summary>
	/// Determines whether a property should be visible as a column in DataGrids (excludes non-public types, debug-only properties, and [Hidden]/[HiddenColumn] properties)
	/// </summary>
	public static bool IsColumnVisible(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.DeclaringType!.IsNotPublic || propertyInfo.GetMethod?.IsPublic != true)
			return false;

#if !DEBUG
			if (propertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return propertyInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			propertyInfo.GetCustomAttribute<HiddenColumnAttribute>() == null; // [HiddenRow]
	}

	/// <summary>
	/// Returns whether the member is declared further down the hierarchy than the one it hides
	/// </summary>
	private static bool IsMoreDerivedThan(this MemberInfo memberInfo, MemberInfo other)
	{
		return other.DeclaringType!.IsAssignableFrom(memberInfo.DeclaringType);
	}

	/// <summary>
	/// Returns the member a subclass declares over the ones it hides, matching what the compiler resolves
	/// </summary>
	/// <remarks>Reflection returns the members in no guaranteed order, so it can't choose between them</remarks>
	public static T GetMostDerived<T>(this IReadOnlyList<T> members) where T : MemberInfo
	{
		T mostDerived = members[0];
		foreach (T member in members)
		{
			if (member.IsMoreDerivedThan(mostDerived))
			{
				mostDerived = member;
			}
		}
		return mostDerived;
	}

	/// <summary>
	/// Returns the members with each hidden declaration replaced by the one hiding it
	/// </summary>
	/// <remarks>
	/// <para>
	/// Reflection resolves a member a subclass redeclares with the same signature, but returns both
	/// declarations when it can't: always for a field, and for a property whose type changed. The
	/// base declaration holds a value nothing refers to, so leaving both in makes whichever the
	/// caller happens to reach for the winner.
	/// </para>
	/// <para>
	/// Each name keeps the position it first appeared at, so this only removes members and never
	/// reorders the rest.
	/// </para>
	/// </remarks>
	public static List<T> RemoveHidden<T>(this IReadOnlyList<T> members) where T : MemberInfo
	{
		var indexes = new Dictionary<string, int>(members.Count);
		List<T> remaining = new(members.Count);

		foreach (T member in members)
		{
			if (indexes.TryGetValue(member.Name, out int index))
			{
				if (member.IsMoreDerivedThan(remaining[index]))
				{
					remaining[index] = member;
				}
			}
			else
			{
				indexes[member.Name] = remaining.Count;
				remaining.Add(member);
			}
		}
		return remaining;
	}
}
