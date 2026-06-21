using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Collections.Concurrent;
using System.Reflection;

namespace SideScroll.Tabs.Lists;

/// <summary>
/// Per-type cache of filtered, sorted <see cref="MemberInfo"/> arrays used by
/// <see cref="ListProperty.Create"/>, <see cref="ListMethod.Create"/>,
/// <see cref="ListField.Create"/>, and <see cref="ListMember.Create"/>.
/// </summary>
/// <remarks>
/// <para>
/// Although the CLR internally caches <c>GetCustomAttribute</c> results after the first
/// per-member call, the LINQ filtering and sorting chains (e.g. <c>.Where(p => p.IsRowVisible())
/// .OrderBy(p => p.Module.Name).ThenBy(p => p.MetadataToken)</c>) are re-evaluated on
/// every call to the <c>Create</c> methods, and <see cref="ListMember.Create"/> additionally
/// rebuilds a <see cref="System.Collections.Generic.SortedDictionary{TKey,TValue}"/> and
/// formats a <c>MetadataToken</c> sort-key string per member.
/// </para>
/// <para>
/// This cache eliminates all of that repeated work by computing the arrays once per
/// <c>(Type, includeBaseTypes, includeStatic)</c> combination and reusing them for every
/// subsequent instance of that type.
/// </para>
/// </remarks>
internal static class ReflectionCache
{
	private readonly record struct TypeKey(Type Type, bool IncludeBaseTypes, bool IncludeStatic);

	private static readonly ConcurrentDictionary<TypeKey, PropertyInfo[]> Properties = new();
	private static readonly ConcurrentDictionary<TypeKey, MethodInfo[]> Methods = new();
	private static readonly ConcurrentDictionary<TypeKey, FieldInfo[]> Fields = new();

	/// <summary>
	/// Merged, sorted <c>(sortKey, MemberInfo)</c> array for use by <see cref="ListMember.Create"/>.
	/// Combines properties and methods in <c>MetadataToken</c> order (the same order that the
	/// original <see cref="System.Collections.Generic.SortedDictionary{TKey,TValue}"/> produced),
	/// with duplicate-name hiding applied so only the most-derived member for each name is kept.
	/// </summary>
	private static readonly ConcurrentDictionary<TypeKey, (string SortKey, MemberInfo Member)[]> MergedMembers = new();

	/// <summary>
	/// Per-<see cref="PropertyInfo"/> display name cache.
	/// Computed once from <c>[Name]</c> / <c>WordSpaced()</c> / <c>[DebugOnly]</c> prefix —
	/// all purely structural, never dependent on instance values.
	/// </summary>
	private static readonly ConcurrentDictionary<PropertyInfo, string> PropertyDisplayNames = new();

	/// <summary>Per-<see cref="FieldInfo"/> display name cache.</summary>
	private static readonly ConcurrentDictionary<FieldInfo, string> FieldDisplayNames = new();

	/// <summary>Per-<see cref="MethodInfo"/> display name cache.</summary>
	private static readonly ConcurrentDictionary<MethodInfo, string> MethodDisplayNames = new();

	/// <summary>
	/// <c>true</c> if the property has any <c>[Hide]</c>, <c>[HideRow]</c>, or class-level
	/// <c>[Hide]</c> attribute — meaning <see cref="ListProperty.IsRowVisible"/> must evaluate
	/// the actual value.  <c>false</c> means <c>IsRowVisible()</c> is unconditionally <c>true</c>
	/// and the call can be skipped entirely.
	/// </summary>
	private static readonly ConcurrentDictionary<PropertyInfo, bool> PropertyHideChecks = new();

	/// <summary>Same short-circuit flag for <see cref="FieldInfo"/>.</summary>
	private static readonly ConcurrentDictionary<FieldInfo, bool> FieldHideChecks = new();

	// ── Public accessors ──────────────────────────────────────────────────

	/// <summary>
	/// Returns the cached, sorted, structurally-filtered <see cref="PropertyInfo"/> array for
	/// <paramref name="type"/>.  Instance-level visibility (e.g. <c>[Hide]</c> with values) is
	/// not applied here; callers must check <see cref="ListProperty.IsRowVisible"/> per instance.
	/// </summary>
	public static PropertyInfo[] GetProperties(Type type, bool includeBaseTypes, bool includeStatic)
		=> Properties.GetOrAdd(new(type, includeBaseTypes, includeStatic), k =>
			ComputeProperties(k.Type, k.IncludeBaseTypes, k.IncludeStatic));

	/// <summary>
	/// Returns the cached, sorted, structurally-filtered <see cref="MethodInfo"/> array
	/// (only methods with <c>[Item]</c> that pass <see cref="ListMethod.IsVisible"/>).
	/// </summary>
	public static MethodInfo[] GetMethods(Type type, bool includeBaseTypes, bool includeStatic)
		=> Methods.GetOrAdd(new(type, includeBaseTypes, includeStatic), k =>
			ComputeMethods(k.Type, k.IncludeBaseTypes, k.IncludeStatic));

	/// <summary>
	/// Returns the cached, sorted, structurally-filtered <see cref="FieldInfo"/> array.
	/// Instance-level visibility (e.g. <c>[Hide]</c> with values) is not applied here.
	/// </summary>
	public static FieldInfo[] GetFields(Type type, bool includeBaseTypes, bool includeStatic)
		=> Fields.GetOrAdd(new(type, includeBaseTypes, includeStatic), k =>
			ComputeFields(k.Type, k.IncludeBaseTypes, k.IncludeStatic));

	/// <summary>
	/// Returns the cached interleaved <c>(sortKey, MemberInfo)</c> array that combines
	/// visible properties and <c>[Item]</c> methods in <c>MetadataToken</c> order.
	/// Used by <see cref="ListMember.Create"/> to bypass per-call
	/// <see cref="System.Collections.Generic.SortedDictionary{TKey,TValue}"/> construction.
	/// </summary>
	public static (string SortKey, MemberInfo Member)[] GetMergedMethodMembers(Type type, bool includeBaseTypes, bool includeStatic)
		=> MergedMembers.GetOrAdd(new(type, includeBaseTypes, includeStatic), k =>
			ComputeMergedMembers(k.Type, k.IncludeBaseTypes, k.IncludeStatic));

	/// <summary>
	/// Returns the cached display name for a property — computed once from <c>[Name]</c>,
	/// <c>PropertyInfo.Name.WordSpaced()</c>, and <c>[DebugOnly]</c> prefix.
	/// Eliminates repeated <see cref="Extensions.StringExtensions.WordSpaced"/> tokenization
	/// and <c>GetCustomAttribute</c> calls in <see cref="ListProperty"/> constructors.
	/// </summary>
	public static string GetPropertyDisplayName(PropertyInfo propertyInfo)
		=> PropertyDisplayNames.GetOrAdd(propertyInfo, ComputePropertyDisplayName);

	/// <summary>
	/// Returns the cached display name for a field — computed once from <c>[Name]</c>,
	/// <c>FieldInfo.Name.WordSpaced()</c>, and <c>[DebugOnly]</c> prefix.
	/// </summary>
	public static string GetFieldDisplayName(FieldInfo fieldInfo)
		=> FieldDisplayNames.GetOrAdd(fieldInfo, ComputeFieldDisplayName);

	/// <summary>
	/// Returns the cached display name for a method — computed once from <c>[Name]</c>,
	/// <c>[Item]</c>, and <c>MethodInfo.Name.TrimEnd("Async").WordSpaced()</c>.
	/// </summary>
	public static string GetMethodDisplayName(MethodInfo methodInfo)
		=> MethodDisplayNames.GetOrAdd(methodInfo, ComputeMethodDisplayName);

	/// <summary>
	/// Returns <c>true</c> if the property has any <c>[Hide]</c>, <c>[HideRow]</c>, or
	/// class-level <c>[Hide]</c> attribute, meaning <see cref="ListProperty.IsRowVisible"/>
	/// needs to evaluate the actual property value.
	/// Returns <c>false</c> when <c>IsRowVisible()</c> is unconditionally <c>true</c>
	/// — callers can skip the call entirely for the common case.
	/// </summary>
	public static bool PropertyHasValueDependentHide(PropertyInfo propertyInfo)
		=> PropertyHideChecks.GetOrAdd(propertyInfo, p =>
			p.GetCustomAttribute<HideAttribute>() != null ||
			p.DeclaringType!.GetCustomAttribute<HideAttribute>() != null ||
			p.GetCustomAttribute<HideRowAttribute>() != null);

	/// <summary>Same short-circuit helper for <see cref="FieldInfo"/>.</summary>
	public static bool FieldHasValueDependentHide(FieldInfo fieldInfo)
		=> FieldHideChecks.GetOrAdd(fieldInfo, f =>
			f.GetCustomAttribute<HideAttribute>() != null ||
			f.DeclaringType?.GetCustomAttribute<HideAttribute>() != null ||
			f.GetCustomAttribute<HideRowAttribute>() != null);

	// ── Compute helpers (run once per type key) ───────────────────────────

	private static PropertyInfo[] ComputeProperties(Type type, bool includeBaseTypes, bool includeStatic)
		=> type.GetProperties()
			.Where(p => p.IsRowVisible())
			.Where(p => p.GetGetMethod(false)?.GetParameters().Length == 0)
			.Where(p => includeBaseTypes || p.DeclaringType == type)
			.Where(p => includeStatic || !p.GetAccessors(nonPublic: true)[0].IsStatic)
			.OrderBy(p => p.Module.Name)
			.ThenBy(p => p.MetadataToken)
			.ToArray();

	private static MethodInfo[] ComputeMethods(Type type, bool includeBaseTypes, bool includeStatic)
		=> type.GetMethods()
			.Where(ListMethod.IsVisible)
			.Where(m => includeBaseTypes || m.DeclaringType == type)
			.Where(m => includeStatic || !m.IsStatic)
			.OrderBy(m => m.Module.Name)
			.ThenBy(m => m.MetadataToken)
			.ToArray();

	private static FieldInfo[] ComputeFields(Type type, bool includeBaseTypes, bool includeStatic)
		=> type.GetFields()
			.Where(f => f.IsRowVisible())
			.Where(f => includeBaseTypes || f.DeclaringType == type)
			.Where(f => includeStatic || !f.IsStatic)
			.OrderBy(f => f.Module.Name)
			.ThenBy(f => f.MetadataToken)
			.ToArray();

	/// <summary>
	/// Builds the merged property + method list in <c>MetadataToken</c> sort order, applying
	/// the same duplicate-name handling as <see cref="ListProperty.Create"/> and
	/// <see cref="ListMethod.Create"/>: if a name appears more than once (e.g. a <c>new</c>
	/// property hiding a base-type property), the last — i.e. most-derived — entry wins.
	/// </summary>
	private static (string SortKey, MemberInfo Member)[] ComputeMergedMembers(Type type, bool includeBaseTypes, bool includeStatic)
	{
		PropertyInfo[] properties = GetProperties(type, includeBaseTypes, includeStatic);
		MethodInfo[] methods = GetMethods(type, includeBaseTypes, includeStatic);

		// Use a SortedDictionary so the final array is already in sort-key order.
		var merged = new SortedDictionary<string, MemberInfo>(StringComparer.Ordinal);

		// Properties — sorted ascending by MetadataToken, so a derived 'new' property
		// has a higher MetadataToken than the base property it hides.
		// Track name → current sortKey so we can remove the earlier base entry.
		var seenPropertyNames = new Dictionary<string, string>(properties.Length);
		foreach (PropertyInfo propertyInfo in properties)
		{
			MethodInfo getMethod = propertyInfo.GetGetMethod(false)!;
			string sortKey = $"{getMethod.Module.Name}:{getMethod.MetadataToken:D10}";

			if (seenPropertyNames.TryGetValue(propertyInfo.Name, out string? prevKey))
			{
				merged.Remove(prevKey);
			}

			seenPropertyNames[propertyInfo.Name] = sortKey;
			merged[sortKey] = propertyInfo;
		}

		// Methods — same duplicate-name handling.
		var seenMethodNames = new Dictionary<string, string>(methods.Length);
		foreach (MethodInfo methodInfo in methods)
		{
			string sortKey = $"{methodInfo.Module.Name}:{methodInfo.MetadataToken:D10}";

			if (seenMethodNames.TryGetValue(methodInfo.Name, out string? prevKey))
			{
				merged.Remove(prevKey);
			}

			seenMethodNames[methodInfo.Name] = sortKey;
			merged[sortKey] = methodInfo;
		}

		return merged.Select(kvp => (kvp.Key, kvp.Value)).ToArray();
	}

	private static string ComputePropertyDisplayName(PropertyInfo propertyInfo)
	{
		var nameAttr = propertyInfo.GetCustomAttribute<NameAttribute>();
		string name = nameAttr?.Name ?? propertyInfo.Name.WordSpaced();

		if (propertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null ||
			propertyInfo.PropertyType.GetCustomAttribute<DebugOnlyAttribute>() != null)
		{
			name = "* " + name;
		}

		return name;
	}

	private static string ComputeFieldDisplayName(FieldInfo fieldInfo)
	{
		var nameAttr = fieldInfo.GetCustomAttribute<NameAttribute>();
		string name = nameAttr?.Name ?? fieldInfo.Name.WordSpaced();

		// Note: original uses && (both field AND field type must have [DebugOnly])
		if (fieldInfo.GetCustomAttribute<DebugOnlyAttribute>() != null &&
			fieldInfo.FieldType.GetCustomAttribute<DebugOnlyAttribute>() != null)
		{
			name = "* " + name;
		}

		return name;
	}

	private static string ComputeMethodDisplayName(MethodInfo methodInfo)
	{
		string name = methodInfo.Name.TrimEnd("Async").WordSpaced();

		var nameAttr = methodInfo.GetCustomAttribute<NameAttribute>();
		if (nameAttr != null)
		{
			name = nameAttr.Name;
		}

		var itemAttr = methodInfo.GetCustomAttribute<ItemAttribute>();
		if (itemAttr?.Name != null)
		{
			name = itemAttr.Name;
		}

		return name;
	}
}
