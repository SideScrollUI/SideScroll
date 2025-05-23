using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Extensions;
using System.Reflection;

namespace SideScroll.Tabs.Lists;

public class ListMethod : ListMember
{
	public MethodInfo MethodInfo { get; }
	private bool CacheEnabled { get; set; }

	private bool _valueCached;
	private object? _valueObject;

	[Editing, InnerValue, WordWrap]
	public override object? Value
	{
		get
		{
			try
			{
				if (CacheEnabled)
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

	public ListMethod(object obj, MethodInfo methodInfo, bool cached = true) :
		base(obj, methodInfo)
	{
		MethodInfo = methodInfo;
		CacheEnabled = cached;

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
		Task task = (Task)MethodInfo.Invoke(Object, new object[] { call });
		await task.ConfigureAwait(false);
		return (object)((dynamic)task).Result;
	}*/

	private object? GetValue()
	{
		var parameters = Array.Empty<object>();
		ParameterInfo[] parameterInfos = MethodInfo.GetParameters();
		if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(Call))
		{
			parameters = [new Call()];
		}

		var result = Task.Run(() => MethodInfo.Invoke(Object, parameters)).GetAwaiter().GetResult();

		if (result is Task)
		{
			return (object)((dynamic)result).Result;
		}

		return result;
	}

	public new static ItemCollection<ListMethod> Create(object obj, bool includeBaseTypes, bool includeStatic = true)
	{
		// this doesn't work for virtual methods (or any method modifier?)
		var methodInfos = obj.GetType().GetMethods()
			.Where(m => IsVisible(m))
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
