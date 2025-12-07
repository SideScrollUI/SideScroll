using System.Reflection;

namespace SideScroll.Utilities;

/// <summary>
/// Provides reflection utilities for navigating object property paths
/// </summary>
/// <remarks>
/// Based on: https://stackoverflow.com/questions/366332/best-way-to-get-sub-properties-using-getproperty
/// </remarks>
public static class ReflectorUtil
{
	/// <summary>
	/// Follows a property path on an object to retrieve a nested value
	/// </summary>
	/// <param name="value">The object to start navigation from</param>
	/// <param name="path">The property path to follow, using dot notation (e.g., "Property.SubProperty[0]")</param>
	/// <returns>The value at the end of the property path, or null if not found</returns>
	/// <remarks>
	/// Supports:
	/// <list type="bullet">
	/// <item><description>Nested properties using dot notation (e.g., "Address.Street")</description></item>
	/// <item><description>Dictionary indexing using brackets (e.g., "Items[key]")</description></item>
	/// <item><description>List indexing using brackets (e.g., "Items[0]")</description></item>
	/// </list>
	/// </remarks>
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
}
