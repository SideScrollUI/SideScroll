using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Atlas.Tabs
{
	public class ListDelegate : ListMember, IPropertyEditable, ILoadAsync
	{
		public delegate Task<object> LoadObjectAsync(Call call);

		public LoadObjectAsync LoadAction;
		public MethodInfo MethodInfo;

		public bool CacheEnabled { get; set; }

		private bool _valueCached;
		private object _valueObject = null;

		[Editing, InnerValue, WordWrap]
		public override object Value
		{
			get
			{
				try
				{
					if (CacheEnabled)
					{
						if (!_valueCached)
						{
							_valueCached = true;
							_valueObject = GetValue();
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

		public override string ToString() => Name;

		public ListDelegate(LoadObjectAsync loadAction, bool cached = true) :
			base(loadAction.Target, loadAction.Method)
		{
			LoadAction = loadAction;
			CacheEnabled = cached;
			MethodInfo = loadAction.Method;

			Name = MethodInfo.Name;
			Name = Name.WordSpaced();
			NameAttribute attribute = MethodInfo.GetCustomAttribute<NameAttribute>();
			if (attribute != null)
				Name = attribute.Name;
		}

		public async Task<object> LoadAsync(Call call)
		{
			return await LoadAction.Invoke(call);
		}

		private object GetValue()
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
}
