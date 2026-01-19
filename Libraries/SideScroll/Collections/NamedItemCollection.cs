using System.Reflection;

namespace SideScroll.Collections;

/// <summary>
/// Provides static reflection-based access to public static properties and fields of a specified type
/// </summary>
/// <typeparam name="TCollection">The type containing the static members to collect</typeparam>
/// <typeparam name="TValue">The type of values to collect</typeparam>
/// <example>
/// <code>
/// // Example usage:
/// public class Icons : NamedItemCollection&lt;Icons, string&gt;
/// {
///     public static string Home = "home.png";
///     public static string Settings = "settings.png";
/// }
/// 
/// // Access all values:
/// var icons = Icons.Values;
/// </code>
/// </example>
public class NamedItemCollection<TCollection, TValue>
{
	public static List<KeyValuePair<MemberInfo, TValue>> Items => _items ??= GetItems();
	private static List<KeyValuePair<MemberInfo, TValue>>? _items;

	public static List<TValue> Values => _values ??= Items.Select(v => v.Value).ToList();
	private static List<TValue>? _values;

	/// <summary>
	/// Retrieves all public static properties and fields from the collection type that match the value type
	/// </summary>
	public static List<KeyValuePair<MemberInfo, TValue>> GetItems()
	{
		Type collectionType = typeof(TCollection);
		Type elementType = typeof(TValue);

		const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;

		// Add PropertyInfo's
		List<KeyValuePair<MemberInfo, TValue>> keyValues = collectionType
			.GetProperties(bindingFlags)
			.Where(p => elementType.IsAssignableFrom(p.PropertyType))
			.OrderBy(p => p.Module.Name)
			.ThenBy(p => p.MetadataToken)
			.Select(p => new KeyValuePair<MemberInfo, TValue>(p, (TValue)p.GetValue(null)!))
			.ToList();

		// Add FieldInfo's
		IEnumerable<KeyValuePair<MemberInfo, TValue>> fieldValues = collectionType
			.GetFields(bindingFlags)
			.Where(f => elementType.IsAssignableFrom(f.FieldType))
			.OrderBy(f => f.Module.Name)
			.ThenBy(f => f.MetadataToken)
			.Select(f => new KeyValuePair<MemberInfo, TValue>(f, (TValue)f.GetValue(null)!));

		keyValues.AddRange(fieldValues);

		return keyValues;
	}
}
