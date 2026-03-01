using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Extensions;
using System.Reflection;

namespace SideScroll.Tabs.Lists;

/// <summary>
/// Represents a method member as a list item with lazy invocation and optional caching
/// </summary>
public class ListMethod : ListMember
{
	/// <summary>
	/// Gets the method info for this method
	/// </summary>
	public MethodInfo MethodInfo { get; }

	/// <summary>
	/// Gets or sets whether the method result should be cached
	/// </summary>
	public bool IsCacheable { get; set; }

	private bool _valueCached;
	private object? _valueObject;

	/// <summary>
	/// Gets or sets the method result value, with optional caching
	/// </summary>
	[EditColumn, InnerValue, WordWrap]
	public override object? Value
	{
		get
		{
			try
			{
				if (IsCacheable)
				{
					if (!_valueCached)
					{
						_valueObject = GetValue();
						_valueCached = true;
					}
					return _valueObject;
				}
				return GetValue();
			}
			catch (Exception)
			{
				return null;
			}
		}
		set
		{
			_valueObject = value;
			_valueCached = true;
		}
	}

	public override string? ToString() => Name;

	/// <summary>
	/// Initializes a new ListMethod for the specified method
	/// </summary>
	public ListMethod(object obj, MethodInfo methodInfo, bool isCacheable = true) :
		base(obj, methodInfo)
	{
		MethodInfo = methodInfo;
		IsCacheable = isCacheable;

		UpdateName();
	}

	private void UpdateName()
	{
		Name = MethodInfo.Name.TrimEnd("Async").WordSpaced();

		NameAttribute? attribute = MethodInfo.GetCustomAttribute<NameAttribute>();
		if (attribute != null)
		{
			Name = attribute.Name;
		}

		ItemAttribute? itemAttribute = MethodInfo.GetCustomAttribute<ItemAttribute>();
		if (itemAttribute != null && itemAttribute.Name != null)
		{
			Name = itemAttribute.Name;
		}
	}

	/*public async Task<object> LoadAsync(Call call)
	{
		Task task = (Task)MethodInfo.Invoke(Object, [call]);
		await task.ConfigureAwait(false);
		return ((dynamic)task).Result;
	}*/

	private object? GetValue()
	{
		object[] parameters = [];
		ParameterInfo[] parameterInfos = MethodInfo.GetParameters();
		if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(Call))
		{
			parameters = [new Call()];
		}

		var result = Task.Run(() => MethodInfo.Invoke(Object, parameters)).GetAwaiter().GetResult();

		if (result is Task)
		{
			return ((dynamic)result).Result;
		}

		return result;
	}

	/// <summary>
	/// Creates a collection of list methods from an object using reflection, filtering to methods marked with [Item]
	/// </summary>
	/// <param name="obj">The object to extract methods from</param>
	/// <param name="includeBaseTypes">Whether to include methods from base types</param>
	/// <param name="includeStatic">Whether to include static methods</param>
	public new static ItemCollection<ListMethod> Create(object obj, bool includeBaseTypes, bool includeStatic = true)
	{
		// this doesn't work for virtual methods (or any method modifier?)
		var methodInfos = obj.GetType().GetMethods()
			.Where(IsVisible)
			.Where(m => includeBaseTypes || m.DeclaringType == obj.GetType())
			.Where(m => includeStatic || !m.IsStatic)
			.OrderBy(m => m.Module.Name)
			.ThenBy(m => m.MetadataToken);

		var listMethods = new ItemCollection<ListMethod>();
		var propertyToIndex = new Dictionary<string, int>();
		foreach (MethodInfo methodInfo in methodInfos)
		{
			var listMethod = new ListMethod(obj, methodInfo);

			if (propertyToIndex.TryGetValue(methodInfo.Name, out int index))
			{
				// Replace base method with derived
				listMethods.RemoveAt(index);
				listMethods.Insert(index, listMethod);
			}
			else
			{
				propertyToIndex[methodInfo.Name] = listMethods.Count;
				listMethods.Add(listMethod);
			}
		}
		return listMethods;
	}

	/// <summary>
	/// Determines whether a method should be visible in lists (must have [Item] attribute and meet visibility criteria)
	/// </summary>
	public static bool IsVisible(MethodInfo methodInfo)
	{
		if (methodInfo.DeclaringType!.IsNotPublic ||
			methodInfo.ReturnType == null ||
			methodInfo.GetCustomAttribute<HiddenAttribute>() != null || // [Hidden]
			methodInfo.GetCustomAttribute<HiddenRowAttribute>() != null || // [HiddenRow]
			methodInfo.GetCustomAttribute<ItemAttribute>() == null // These are treated as Data Members
			)
			return false;

#if !DEBUG
			if (methodInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		ParameterInfo[] parameterInfos = methodInfo.GetParameters();
		if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType != typeof(Call))
			return false;

		return true;
	}
}
