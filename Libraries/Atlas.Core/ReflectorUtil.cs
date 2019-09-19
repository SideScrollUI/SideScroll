using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Atlas.Core
{
	// https://stackoverflow.com/questions/366332/best-way-to-get-sub-properties-using-getproperty
	public static class ReflectorUtil
	{
		public static object FollowPropertyPath(object value, string path)
		{
			if (value == null) throw new ArgumentNullException("value");
			if (path == null) throw new ArgumentNullException("path");

			Type currentType = value.GetType();

			object obj = value;
			foreach (string propertyName in path.Split('.'))
			{
				if (currentType != null)
				{
					PropertyInfo property = null;
					int brackStart = propertyName.IndexOf("[");
					int brackEnd = propertyName.IndexOf("]");

					property = currentType.GetProperty(brackStart > 0 ? propertyName.Substring(0, brackStart) : propertyName);
					obj = property.GetValue(obj, null);

					if (brackStart > 0)
					{
						string index = propertyName.Substring(brackStart + 1, brackEnd - brackStart - 1);
						foreach (Type iType in obj.GetType().GetInterfaces())
						{
							if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
							{
								obj = typeof(ReflectorUtil).GetMethod("GetDictionaryElement")
													 .MakeGenericMethod(iType.GetGenericArguments())
													 .Invoke(null, new object[] { obj, index });
								break;
							}
							if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IList<>))
							{
								obj = typeof(ReflectorUtil).GetMethod("GetListElement")
													 .MakeGenericMethod(iType.GetGenericArguments())
													 .Invoke(null, new object[] { obj, index });
								break;
							}
						}
					}

					currentType = obj != null ? obj.GetType() : null; //property.PropertyType;
				}
				else return null;
			}
			return obj;
		}

		public static TValue GetDictionaryElement<TKey, TValue>(IDictionary<TKey, TValue> dict, object index)
		{
			TKey key = (TKey)Convert.ChangeType(index, typeof(TKey), null);
			return dict[key];
		}

		public static T GetListElement<T>(IList<T> list, object index)
		{
			return list[Convert.ToInt32(index)];
		}

	}
}
