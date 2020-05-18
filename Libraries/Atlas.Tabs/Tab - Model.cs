﻿using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Atlas.Tabs
{
	public interface ITabSelector //: ITabCreator
	{
		IList SelectedItems { get; }
		//object CreateControl(object value, out string label); // remove? moved to ITabCreator

		event EventHandler<EventArgs> OnSelectionChanged;
	}

	public interface ITabCreator
	{
		object CreateControl(object value, out string label);
	}

	public interface ITabCreatorAsync
	{
		Task<ITab> CreateAsync(Call call);
	}

	public class TabObject
	{
		public object obj;
		public bool fill;
	}

	public class TabModel
	{
		public static List<Type> IgnoreHighlightTypes { get; set; } = new List<Type>();
		
		public enum AutoSelectType
		{
			None,
			FirstSavedOrNew,
			AnyNewOrSaved,
		}
		public string Id { get; set; } // todo: Unique key for bookmarks?
		public string Name { get; set; } = "<TabModel>";
		public string Notes { get; set; }
		public object Object { get; set; } // optional
		public bool AutoLoad { get; set; } = true;
		public AutoSelectType AutoSelect { get; set; } = AutoSelectType.FirstSavedOrNew;

		public BookmarkCollection Bookmarks { get; set; }
		public IList Actions { get; set; }
		public TaskInstanceCollection Tasks { get; set; } = new TaskInstanceCollection();

		public List<IList> ItemList { get; set; } = new List<IList>();

		public List<TabObject> Objects { get; set; } = new List<TabObject>();
		//public List<ITabControl> CustomTabControls { get; set; } = new List<ITabControl>(); // should everything be a custom control? tabControls?

		public void AddObject(object obj, bool fill = false)
		{
			if (obj == null)
				throw new Exception("Object is null");
			Objects.Add(new TabObject() { obj = obj, fill = fill });
		}

		public IList Items
		{
			set
			{
				ItemList.Clear();
				//ItemList.Add(value);
				//bject = value;
				AddData(value);
			}
		}
		public bool Editing { get; set; } = false;
		public bool Skippable { get; set; } = false;
		public int MinDesiredWidth { get; set; } = 0;
		public int MaxDesiredWidth { get; set; } = 1500;


		public TabModel()
		{
		}

		public TabModel(string name)
		{
			Name = name;
		}

		public static TabModel Create(string name, object obj)
		{
			if (ObjectHasChildren(obj) == false)
				return null;
			TabModel tabModel = new TabModel(name);
			tabModel.AddData(obj);
			if (tabModel.ItemList.Count == 0)
				return null;
			return tabModel;
		}

		public override string ToString()
		{
			return Name;
		}

		public void AddData(object obj)
		{
			Object = obj;
			if (obj == null)
				return;

			Type type = obj.GetType();
			if (type.Assembly.ManifestModule.ScopeName == "Lazy")
				type = type.BaseType; // Use original type for lazy loaded serializer wrapper classes, so properties appear in the same MetadataToken order

			if (obj is IList iList)
			{
				Type elementType = type.GetElementTypeForAll();
				if (elementType != null)
				{
					if (elementType == typeof(string))
					{
						// string properties don't display well
						AddEnumerable((IEnumerable)obj);
					}
					else if (type.GenericTypeArguments.Length > 0 && elementType.GetProperties().Length > 0)
					{
						// list element type has properties (should check if they're visible properties?)
						ItemList.Add(iList);
					}
					/*else if (elementType.IsPrimitive)
					{
						// todo: create a List<elementType>
					}
					else*/
					else if (elementType.GetProperties().Length == 0)
					{
						ItemList.Add(ListToString.Create(iList));
					}
					else
					{
						// this doesn't work if there's a null value in the array
						// should we databind columns to Value property in Nullable?
						Type underlyingType = Nullable.GetUnderlyingType(elementType);
						if (underlyingType != null)
							elementType = underlyingType;

						Type genericType = typeof(ItemCollection<>).MakeGenericType(elementType);
						IList iNewList = (IList)Activator.CreateInstance(genericType);
						foreach (object child in iList)
							iNewList.Add(child);
						ItemList.Add(iNewList);
					}
					// skip over single items that will take up lots of room (always show ListItems though)
					Skippable = false;
					if (ItemList[0].Count == 1)
					{
						var firstItem = ItemList[0][0];
						var skippableAttribute = firstItem.GetType().GetCustomAttribute<SkippableAttribute>();
						if (skippableAttribute != null)
						{
							Skippable = skippableAttribute.Value;
						}
						else if (!(firstItem is ITab) && TabDataSettings.GetVisibleProperties(elementType).Count > 1)
						{
							Skippable = true;
						}

						//Skippable = (skippableAttribute != null) || (!(firstItem is ITab) && TabDataSettings.GetVisibleProperties(elementType).Count > 1);
					}
					return;
				}
			}

			// Can only set one set of columns for Items, so we have to choose
			// Should we always show fields and properties?
			if (typeof(IDictionary).IsAssignableFrom(type))
			{
				// Show as Key/Value columns, change to keys only?
				AddDictionary(type);
			}
			else if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				// show inner type as list (but only one column using a ToString for the label)
				//AddObject(type);
				AddEnumerable((IEnumerable)obj);
			}
			else if (type.IsEnum)
			{
				var values = Enum.GetValues(type);
				AddEnumerable(values);
			}
			else
			{
				if (!ObjectHasChildren(obj))
					return;
				// show as Name/Value columns for fields and properties
				AddObject(type);
			}
		}

		/*public bool Skippable
		{
			get
			{
				if (ItemList.Count == 1 && ItemList[0].Count == 1 && TabDataSettings.GetVisibleProperties(elementType).Count > 1) ;
				return false;
			}
		}*/

		private void AddDictionary(Type type)
		{
			var sortedList = new List<DictionaryEntry>(); // can't sort ItemCollection
			try
			{
				foreach (DictionaryEntry item in (IDictionary)Object)
				{
					sortedList.Add(item);
				}
			}
			catch (Exception)
			{

			}
			if (Object is IComparable)
				sortedList = sortedList.OrderBy(x => x.Key).ToList();
			ItemList.Add(new ItemCollection<DictionaryEntry>(sortedList));
		}

		public Type[] GetInterfaceGenericArguments(Type type, Type genericType)
		{
			foreach (Type iType in type.GetInterfaces())
			{
				if (iType.IsGenericType && iType.GetGenericTypeDefinition() == genericType)
				{
					return iType.GetGenericArguments();
				}
			}
			return null;
		}

		private void AddEnumerable(IEnumerable iEnumerable)
		{
			Type type = iEnumerable.GetType();
			if (type.GenericTypeArguments.Length == 0 || type.GenericTypeArguments[0] == typeof(string))
			{
				ItemList.Add(ListToString.Create(iEnumerable));
				return;
			}
			Type elementType = GetElementType(type);

			Type genericType = typeof(ItemCollection<>).MakeGenericType(elementType);
			IList iList = (IList)Activator.CreateInstance(genericType);
			foreach (var item in iEnumerable)
			{
				iList.Add(item);
			}
			ItemList.Add(iList);
		}

		// merge with GetElementTypeForAll?
		private Type GetElementType(Type type)
		{
			Type elementType;
			if (type.IsAssignableToGenericType(typeof(IEnumerable<>)))
			{
				// generic interface might be in interface, not primary class
				Type[] types = GetInterfaceGenericArguments(type, typeof(IEnumerable<>));
				elementType = types[0];
			}
			else
			{
				elementType = type.GenericTypeArguments[0];
			}

			return elementType;
		}

		// Adds the fields and properties as one list, and methods as another list (disabled right now)
		private void AddObject(Type type)
		{
			var itemCollection = new ItemCollection<ListMember>();

			var listFields = ListField.Create(Object);
			itemCollection.AddRange(listFields);

			var listProperties = ListProperty.Create(Object);
			itemCollection.AddRange(listProperties);

			AddMethodProperties(type, itemCollection);

			//itemCollection = new ItemCollection<ListMember>(itemCollection.OrderBy(x => x.memberInfo.MetadataToken).ToList());
			ItemList.Add(itemCollection);

			AddMethods(type);
		}

		public void Clear()
		{
			Objects.Clear();
			ItemList.Clear();
			Actions = null;
		}

		private List<MethodInfo> GetVisibleMethods(Type type)
		{
			MethodInfo[] methodInfos = type.GetMethods().OrderBy(x => x.MetadataToken).ToArray();
			var visibleMethods = new List<MethodInfo>();

			foreach (MethodInfo methodInfo in methodInfos)
			{
				if (methodInfo.IsPublic && methodInfo.ReturnType != null && methodInfo.GetType().GetCustomAttribute<VisibleAttribute>() != null)
					visibleMethods.Add(methodInfo);
			}
			return visibleMethods;
		}

		private void AddMethodProperties(Type type, ItemCollection<ListMember> itemCollection)
		{
			List<MethodInfo> visibleMethods = GetVisibleMethods(type);

			// Add any methods that return a Task object
			foreach (MethodInfo methodInfo in visibleMethods)
			{
				ParameterInfo[] parameterInfos = methodInfo.GetParameters();
				if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(Call))
				{
					//itemCollection.Add(new ListMethod2((ListMethod2.LoadObjectAsync)Delegate.CreateDelegate(typeof(ListMethod2.LoadObjectAsync), methodInfo)));

					itemCollection.Add(new ListMethod(Object, methodInfo));
				}
			}
		}

		private void AddMethods(Type type)
		{
			List<MethodInfo> visibleMethods = GetVisibleMethods(type);

			// Add any methods that return a Task object
			ItemCollection<TaskCreator> methods = new ItemCollection<TaskCreator>();
			foreach (MethodInfo methodInfo in visibleMethods)
			{
				// todo: check parameter types, assuming Log param now
				/*if (methodInfo.IsPublic && methodInfo.ReturnType.IsAssignableFrom(typeof(Task)))
				{
					//methods.Add(new TaskMethod(methodInfo, Object));
				}*/

				ParameterInfo[] parameterInfos = methodInfo.GetParameters();
				if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(Call))
				{
					methods.Add(new TaskDelegate(methodInfo.Name, (TaskDelegate.CallAction)Delegate.CreateDelegate(typeof(TaskDelegate.CallAction), methodInfo)));
				}
			}
			if (methods.Count > 0)
				Actions = methods;
		}

		public TabBookmark FindMatches(Filter filter, int depth)
		{
			TabBookmark tabBookmark = new TabBookmark()
			{
				Name = Name,
				tabViewSettings = new TabViewSettings(),
			};

			depth--;
			foreach (IList iList in ItemList)
			{
				Type listType = iList.GetType();
				Type elementType = listType.GetGenericArguments()[0]; // dictionaries?
				List<PropertyInfo> visibleProperties = TabDataSettings.GetVisibleProperties(elementType);

				TabDataSettings tabDataSettings = new TabDataSettings();
				tabBookmark.tabViewSettings.TabDataSettings.Add(tabDataSettings);

				foreach (object obj in iList)
				{
					if (filter.Matches(obj, visibleProperties))
					{
						SelectedRow selectedRow = new SelectedRow()
						{
							rowIndex = -1,
							obj = obj,
						};
						tabDataSettings.SelectedRows.Add(selectedRow);
						tabBookmark.selectedObjects.Add(obj);
					}
					else if (depth >= 0)
					{
						TabModel tabModel = Create(obj.Formatted(), obj);
						if (tabModel != null)
						{
							TabBookmark childNode = tabModel.FindMatches(filter, depth);
							if (childNode.selectedObjects.Count > 0)
							{
								childNode.tabModel = tabModel;
								SelectedRow selectedRow = new SelectedRow()
								{
									rowIndex = -1,
									obj = obj,
								};
								tabDataSettings.SelectedRows.Add(selectedRow);
								tabBookmark.tabChildBookmarks.Add(childNode.Name, childNode);
								tabBookmark.selectedObjects.Add(obj);
							}
						}
					}
				}
			}

			return tabBookmark;
		}

		// used for saving/loading TabViewSettings
		public string CustomSettingsPath { get; set; }
		public string ObjectTypePath
		{
			get
			{
				if (Object == null)
					return "(null)";
				
				Type objType = Object.GetType();
				return objType.FullName; // need to hash this or escape it
			}
		}

		// Might want to move this elsewhere or refactor
		public static bool ObjectHasChildren(object obj, bool ignoreEmpty = false)
		{
			if (obj == null)
				return false;
			object value = obj.GetInnerValue();
			if (value == null)
				return false;
			if (value is ListItem listItem)
				value = listItem.Value;
			if (value is ListMember listMember)
				value = listMember.Value;
			Type type = value.GetType();
			if (type.IsPrimitive ||
				type.IsEnum ||
				type.Equals(typeof(string)) ||
				type.Equals(typeof(decimal)) ||
				type.Equals(typeof(DateTime)) ||
				type.Equals(typeof(TimeSpan))) //  || type.IsEnum 
			{
				return false;
			}

			if (ignoreEmpty)
			{
				if (value is ICollection collection)
				{
					if (collection.Count == 0)
						return false;
					Type elementType = collection.GetType().GetElementTypeForAll();
					if (elementType != null && elementType.IsPrimitive)
						return false;
				}

				foreach (Type ignoreType in IgnoreHighlightTypes)
				{
					if (ignoreType.IsAssignableFrom(type))
						return false;
				}
			}

			return true;
		}
	}
}
