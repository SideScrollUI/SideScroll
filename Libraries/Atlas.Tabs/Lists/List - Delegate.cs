using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Atlas.Core;
using Atlas.Extensions;

namespace Atlas.Tabs
{
	public class ListDelegate : ListMember, IPropertyEditable, ILoadAsync
	{
		public delegate Task<object> LoadObjectAsync(Call call);

		private LoadObjectAsync loadAction;
		private MethodInfo methodInfo;

		private bool CacheEnabled { get; set; }
		private bool valueCached;
		private object valueObject = null;

		[Editing, InnerValue, WordWrap]
		public override object Value
		{
			get
			{
				try
				{
					if (CacheEnabled)
					{
						if (!valueCached)
						{
							valueCached = true;
							valueObject = GetValue();
						}
						return valueObject;
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
				valueObject = value;
				valueCached = true;
			}
		}

		public ListDelegate(LoadObjectAsync loadAction, bool cached = true) :
			base(loadAction.Target, loadAction.Method)
		{
			this.loadAction = loadAction;
			this.CacheEnabled = cached;
			this.methodInfo = loadAction.Method;

			Name = methodInfo.Name;
			Name = Name.WordSpaced();
			NameAttribute attribute = methodInfo.GetCustomAttribute<NameAttribute>();
			if (attribute != null)
				Name = attribute.Name;
		}

		public override string ToString()
		{
			return Name;
		}

		public async Task<object> LoadAsync(Call call)
		{
			return await loadAction.Invoke(call);
		}

		private object GetValue()
		{
			//return Task.Run(() => callAction.Invoke(call)).GetAwaiter().GetResult();
			return Task.Run(() => loadAction.Invoke(new Call())).GetAwaiter().GetResult();
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

					ListMethodObject listMethod = new ListMethodObject(obj, methodInfo);

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
