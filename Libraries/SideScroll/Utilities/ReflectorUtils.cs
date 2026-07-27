using System.Reflection;

namespace SideScroll.Utilities;

/// <summary>
/// Provides reflection utilities for navigating object property paths
/// </summary>
/// <remarks>
/// Based on: https://stackoverflow.com/questions/366332/best-way-to-get-sub-properties-using-getproperty
/// </remarks>
public static class ReflectorUtils
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
				bool hasBracket = brackStart >= 0 || brackEnd >= 0;
				if (hasBracket &&
					(brackStart <= 0 || brackEnd != propertyName.Length - 1 || brackEnd <= brackStart + 1))
				{
					return null;
				}
				string subPropertyName = brackStart > 0 ? propertyName[..brackStart] : propertyName;

				var properties = currentType.GetProperties()
					.Where(x => x.Name == subPropertyName)
					.ToList();
				PropertyInfo? property = properties.FirstOrDefault(x => x.DeclaringType == currentType)
					?? properties.FirstOrDefault();
				if (property == null)
					return null;

				obj = property.GetValue(obj, null);

				if (brackStart > 0)
				{
					if (obj == null)
						return null;

					string index = propertyName.Substring(brackStart + 1, brackEnd - brackStart - 1);
					bool indexed = false;
					foreach (Type iType in obj.GetType().GetInterfaces())
					{
						if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
						{
							indexed = true;
							try
							{
								obj = typeof(ReflectorUtils).GetMethod(nameof(GetDictionaryElement))!
									.MakeGenericMethod(iType.GetGenericArguments())
									.Invoke(null, [obj, index]);
							}
							catch (TargetInvocationException e) when (
								e.InnerException is KeyNotFoundException or InvalidCastException or
									FormatException or OverflowException)
							{
								return null;
							}
							break;
						}
						if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IList<>))
						{
							indexed = true;
							try
							{
								obj = typeof(ReflectorUtils).GetMethod(nameof(GetListElement))!
									.MakeGenericMethod(iType.GetGenericArguments())
									.Invoke(null, [obj, index]);
							}
							catch (TargetInvocationException e) when (
								e.InnerException is ArgumentOutOfRangeException or
									IndexOutOfRangeException or FormatException or OverflowException)
							{
								return null;
							}
							break;
						}
					}
					if (!indexed)
						return null;
				}

				currentType = obj?.GetType(); //property.PropertyType;
			}
			else return null;
		}
		return obj;
	}

	/// <summary>
	/// Gets an element from a dictionary by converting the index to the appropriate key type
	/// </summary>
	public static TValue GetDictionaryElement<TKey, TValue>(IDictionary<TKey, TValue> dict, object index)
	{
		TKey key = (TKey)Convert.ChangeType(index, typeof(TKey), null);
		return dict[key];
	}

	/// <summary>
	/// Gets an element from a list by index
	/// </summary>
	public static T GetListElement<T>(IList<T> list, object index)
	{
		return list[Convert.ToInt32(index)];
	}
}
