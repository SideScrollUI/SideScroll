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
		public MethodInfo MethodInfo;
		private bool CacheEnabled { get; set; }
		private bool valueCached;
		private object valueObject = null;

		[HiddenColumn]
		public int? MaxDesiredWidth
		{
			get
			{
				var maxWidthAttribute = MethodInfo.GetCustomAttribute<MaxWidthAttribute>();
				return maxWidthAttribute?.MaxWidth;
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

		public override string ToString() => Name;

		public ListMethod(object obj, MethodInfo methodInfo, bool cached = true) : 
			base(obj, methodInfo)
		{
			MethodInfo = methodInfo;
			CacheEnabled = cached;

			Name = methodInfo.Name;
			Name = Name.WordSpaced();
			NameAttribute attribute = methodInfo.GetCustomAttribute<NameAttribute>();
			if (attribute != null)
				Name = attribute.Name;

			ItemAttribute itemAttribute = methodInfo.GetCustomAttribute<ItemAttribute>();
			if (itemAttribute != null && itemAttribute.Name != null)
				Name = itemAttribute.Name;
		}

		public async Task<object> LoadAsync(Call call)
		{
			Task task = (Task)MethodInfo.Invoke(Object, new object[] { call });
			await task.ConfigureAwait(false);
			return (object)((dynamic)task).Result;
		}

		private object GetValue()
		{
			//return Task.Run(() => callAction.Invoke(call)).GetAwaiter().GetResult();
			var result = Task.Run(() => MethodInfo.Invoke(Object, new object[] { new Call() })).GetAwaiter().GetResult();
			if (result is Task task)
				return (object)((dynamic)result).Result;

			return result;
		}

		public static new ItemCollection<ListMethod> Create(object obj)
		{
			// this doesn't work for virtual methods (or any method modifier?)
			MethodInfo[] methodInfos = obj.GetType().GetMethods().OrderBy(x => x.MetadataToken).ToArray();
			var listMethods = new ItemCollection<ListMethod>();
			var propertyToIndex = new Dictionary<string, int>();
			foreach (MethodInfo methodInfo in methodInfos)
			{
				if (methodInfo.DeclaringType.IsNotPublic)
					continue;

				if (methodInfo.ReturnType == null)
					continue;

				if (methodInfo.GetCustomAttribute<HiddenAttribute>() != null)
					continue;

				if (methodInfo.GetCustomAttribute<HiddenRowAttribute>() != null)
					continue;

				ParameterInfo[] parameterInfos = methodInfo.GetParameters();
				if (parameterInfos.Length != 1 || parameterInfos[0].ParameterType != typeof(Call))
					continue;

				if (methodInfo.GetCustomAttribute<ItemAttribute>() == null)
					continue;

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
	}
}
