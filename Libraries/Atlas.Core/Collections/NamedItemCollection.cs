using System.Reflection;

namespace Atlas.Core;

public class NamedItemCollection<T1, T2>
{
	public static List<KeyValuePair<MemberInfo, T2>> Items => _items ??= GetItems();
	private static List<KeyValuePair<MemberInfo, T2>>? _items;

	public static List<T2> Values => _values ??= Items.Select(v => v.Value).ToList();
	private static List<T2>? _values;

	public static List<KeyValuePair<MemberInfo, T2>> GetItems()
	{
		Type collectionType = typeof(T1);
		Type elementType = typeof(T2);

		BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;

		// Add PropertyInfo's
		List<KeyValuePair<MemberInfo, T2>> keyValues = collectionType
			.GetProperties(bindingFlags)
			.Where(p => elementType.IsAssignableFrom(p.PropertyType))
			.OrderBy(x => x.MetadataToken)
			.Select(p => new KeyValuePair<MemberInfo, T2>(p, (T2)p.GetValue(null)!))
			.ToList();

		// Add FieldInfo's
		IEnumerable<KeyValuePair<MemberInfo, T2>> fieldValues = collectionType
			.GetFields(bindingFlags)
			.Where(f => elementType.IsAssignableFrom(f.FieldType))
			.OrderBy(x => x.MetadataToken)
			.Select(f => new KeyValuePair<MemberInfo, T2>(f, (T2)f.GetValue(null)!));

		keyValues.AddRange(fieldValues);

		return keyValues;
	}
}
