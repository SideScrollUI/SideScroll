using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Reflection;

namespace SideScroll.Tabs.Lists;

/// <summary>
/// Represents an async delegate member that can be lazily loaded and cached
/// </summary>
public class ListDelegate : ListMember, IPropertyIsEditable, ILoadAsync
{
	/// <summary>
	/// Delegate type for loading objects asynchronously
	/// </summary>
	public delegate Task<object?> LoadObjectAsync(Call call);

	/// <summary>
	/// Gets the async load action delegate
	/// </summary>
	public LoadObjectAsync LoadAction { get; }

	/// <summary>
	/// Gets the method info for the load action
	/// </summary>
	public MethodInfo MethodInfo => LoadAction.Method;

	/// <summary>
	/// Gets or sets whether the loaded value should be cached
	/// </summary>
	public bool IsCacheable { get; set; }

	private bool _valueCached;
	private object? _valueObject;

	/// <summary>
	/// Gets or sets the loaded value, with optional caching
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
	/// Initializes a new ListDelegate with the specified async load action
	/// </summary>
	public ListDelegate(LoadObjectAsync loadAction, bool isCacheable = true) :
		base(loadAction.Target!, loadAction.Method)
	{
		LoadAction = loadAction;
		IsCacheable = isCacheable;

		Name = MethodInfo.Name.WordSpaced();

		NameAttribute? attribute = MethodInfo.GetCustomAttribute<NameAttribute>();
		if (attribute != null)
		{
			Name = attribute.Name;
		}
	}

	/// <summary>
	/// Loads the value asynchronously using the load action
	/// </summary>
	public async Task<object?> LoadAsync(Call call)
	{
		return await LoadAction.Invoke(call);
	}

	private object? GetValue()
	{
		return Task.Run(() => LoadAction.Invoke(new Call())).GetAwaiter().GetResult();
	}

	/*public static ItemCollection<ListMethodObject> Create(object obj)
	{
		// this doesn't work for virtual methods (or any method modifier?)
		MethodInfo[] methodInfos = obj.GetType().GetMethods().OrderBy(x => x.MetadataToken).ToArray();
		var listMethods = new ItemCollection<ListMethodObject>();
		var propertyToIndex = new Dictionary<string, int>();
		foreach (MethodInfo methodInfo in methodInfos)
		{
			if (!methodInfo.DeclaringType.IsNotPublic)
			{
				if (methodInfo.GetCustomAttribute<HiddenRowAttribute>() != null)
					continue;

				if (methodInfo.DeclaringType.IsNotPublic)
					continue;

				var listMethod = new ListMethodObject(obj, methodInfo);

				int index;
				if (propertyToIndex.TryGetValue(methodInfo.Name, out index))
				{
					listMethods.RemoveAt(index);
					listMethods.Insert(index, listMethod);
				}
				else
				{
					propertyToIndex[methodInfo.Name] = listMethods.Count;
					listMethods.Add(listMethod);
				}
			}
		}
		return listMethods;
	}*/
}
