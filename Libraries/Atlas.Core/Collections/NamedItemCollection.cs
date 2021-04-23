using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Atlas.Core
{
	public class NamedItemCollection<T1, T2>
	{
		public static List<T2> All => _all = _all ?? GetAll();
		private static List<T2> _all;

		private static List<T2> GetAll()
		{
			Type collectionType = typeof(T1);
			Type elementType = typeof(T2);

			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;

			List<T2> propertyValues = collectionType.GetProperties(bindingFlags).Where(p => elementType.IsAssignableFrom(p.PropertyType)).OrderBy(x => x.MetadataToken).Select(p => (T2)p.GetValue(null)).ToList();

			IEnumerable<T2> fieldValues = collectionType.GetFields(bindingFlags).Where(f => elementType.IsAssignableFrom(f.FieldType)).OrderBy(x => x.MetadataToken).Select(f => (T2)f.GetValue(null));

			propertyValues.AddRange(fieldValues);

			return propertyValues;
		}
	}
}
