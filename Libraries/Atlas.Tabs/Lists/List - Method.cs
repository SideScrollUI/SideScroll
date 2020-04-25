using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Atlas.Tabs
{
	public class ListMethod : ListMember, IPropertyEditable, IMaxDesiredWidth, ILoadAsync
	{
		public MethodInfo methodInfo;
		private bool CacheEnabled { get; set; }
		private bool valueCached;
		private object valueObject = null;

		[HiddenColumn]
		public int? MaxDesiredWidth
		{
			get
			{
				var maxWidthAttribute = methodInfo.GetCustomAttribute<MaxWidthAttribute>();
				if (maxWidthAttribute != null)
					return maxWidthAttribute.MaxWidth;
				return null;
			}
		}

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

		public ListMethod(object obj, MethodInfo methodInfo, bool cached = true) : 
			base(obj, methodInfo)
		{
			this.methodInfo = methodInfo;
			this.CacheEnabled = cached;

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
			var task = (Task<object>)methodInfo.Invoke(obj, new object[] { call });
			return await task;
		}

		private object GetValue()
		{
			//return Task.Run(() => callAction.Invoke(call)).GetAwaiter().GetResult();
			return Task.Run(() => methodInfo.Invoke(obj, new object[] { new Call() })).GetAwaiter().GetResult();
		}

		public static ItemCollection<ListMethod> Create(object obj)
		{
			// this doesn't work for virtual methods (or any method modifier?)
			MethodInfo[] methodInfos = obj.GetType().GetMethods().OrderBy(x => x.MetadataToken).ToArray();
			var listMethods = new ItemCollection<ListMethod>();
			var propertyToIndex = new Dictionary<string, int>();
			foreach (MethodInfo methodInfo in methodInfos)
			{
				if (!methodInfo.DeclaringType.IsNotPublic)
				{
					if (methodInfo.GetCustomAttribute<HiddenRowAttribute>() != null)
						continue;
					if (methodInfo.DeclaringType.IsNotPublic)
						continue;

					ListMethod listMethod = new ListMethod(obj, methodInfo);

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
		}
	}
}
