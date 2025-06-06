using System.Reflection;

namespace SideScroll.Utilities;

// https://stackoverflow.com/questions/366332/best-way-to-get-sub-properties-using-getproperty
public static class ReflectorUtil
{
	public static object? FollowPropertyPath(object value, string path)
	{
		ArgumentNullException.ThrowIfNull(value);
		ArgumentNullException.ThrowIfNull(path);

		Type? currentType = value.GetType();

		object? obj = value;
		foreach (string propertyName in path.Split('.'))
		{
			if (currentType != null)
			{
				int brackStart = propertyName.IndexOf('[');
				int brackEnd = propertyName.IndexOf(']');
				string subPropertyName = brackStart > 0 ? propertyName[..brackStart] : propertyName;

				var properties = currentType.GetProperties()
					.Where(x => x.Name == subPropertyName)
					.ToList();
				PropertyInfo property = properties.FirstOrDefault(x => x.DeclaringType == currentType) ?? properties.First();
				obj = property.GetValue(obj, null);

				if (brackStart > 0)
				{
					string index = propertyName.Substring(brackStart + 1, brackEnd - brackStart - 1);
					foreach (Type iType in obj!.GetType().GetInterfaces())
					{
						if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
						{
							obj = typeof(ReflectorUtil).GetMethod("GetDictionaryElement")!
								.MakeGenericMethod(iType.GetGenericArguments())
								.Invoke(null, [obj, index]);
							break;
						}
						if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IList<>))
						{
							obj = typeof(ReflectorUtil).GetMethod("GetListElement")!
								.MakeGenericMethod(iType.GetGenericArguments())
								.Invoke(null, [obj, index]);
							break;
						}
					}
				}

				currentType = obj?.GetType(); //property.PropertyType;
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
