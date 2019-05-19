using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Atlas.Core;
using Atlas.Extensions;

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

	public class TabObject
	{
		public object obj;
		public bool fill;
	}

	public class TabModel
	{
		public string Id { get; set; } // todo: Unique key for bookmarks?
		public string Name { get; set; } = "<TabModel>";
		public string Notes { get; set; }
		public object Object { get; set; } // optional
		public bool AutoLoad { get; set; } = true;

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
				//this.Object = value;
				AddData(value);
			}
		}
		public bool Editing { get; set; } = false;
		public bool Skippable { get; set; } = false;


		public TabModel()
		{
		}

		public TabModel(string name)
		{
			this.Name = name;
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
			this.Object = obj;
			if (obj == null)
				return;

			Type type = obj.GetType();
			if (type.Assembly.ManifestModule.ScopeName == "Lazy")
				type = type.BaseType; // Use original type for lazy loaded serializer wrapper classes, so properties appear in the same MetadataToken order

			if (obj is IList)
			{
				IList iList = (IList)obj;

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
					Skippable = (ItemList[0].Count == 1 && TabDataSettings.GetVisibleProperties(elementType).Count > 1);
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
			List<DictionaryEntry> sortedList = new List<DictionaryEntry>(); // can't sort ItemCollection
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

			Type genericType = typeof(ItemCollection<>).MakeGenericType(elementType);
			IList iList = (IList)Activator.CreateInstance(genericType);
			foreach (var item in iEnumerable)
			{
				iList.Add(item);
			}
			ItemList.Add(iList);
		}

		// Adds the fields and properties as one list, and methods as another list (disabled right now)
		private void AddObject(Type type)
		{
			FieldInfo[] fieldInfos = type.GetFields().OrderBy(x => x.MetadataToken).ToArray();
			PropertyInfo[] propertyInfos = type.GetProperties().OrderBy(x => x.MetadataToken).ToArray();

			ItemCollection<ListMember> itemCollection = new ItemCollection<ListMember>();

			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				if (fieldInfo.GetCustomAttribute(typeof(HiddenRowAttribute)) != null)
					continue;
				ListField listField = new ListField(Object, fieldInfo);
				itemCollection.Add(listField);
			}

			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (!propertyInfo.DeclaringType.IsNotPublic)
				{
					if (propertyInfo.GetCustomAttribute(typeof(HiddenRowAttribute)) != null)
						continue;
					ListProperty listProperty = new ListProperty(Object, propertyInfo);
					itemCollection.Add(listProperty);
				}
			}
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

		private void AddMethods(Type type)
		{
			MethodInfo[] methodInfos = type.GetMethods().OrderBy(x => x.MetadataToken).ToArray();

			// Add any methods that return a Task object
			ItemCollection<TaskCreator> methods = new ItemCollection<TaskCreator>();
			foreach (MethodInfo methodInfo in methodInfos)
			{
				// todo: check parameter types, assuming Log param now
				/*if (methodInfo.IsPublic && methodInfo.ReturnType.IsAssignableFrom(typeof(Task)))
				{
					//methods.Add(new TaskMethod(methodInfo, Object));
				}*/
				if (methodInfo.IsPublic && methodInfo.ReturnType == null)
				{
					ParameterInfo[] parameterInfos = methodInfo.GetParameters();
					if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(Call))
					{
						methods.Add(new TaskDelegate(methodInfo.Name, (TaskDelegate.CallAction)Delegate.CreateDelegate(typeof(TaskDelegate.CallAction), methodInfo)));
					}
				}
			}
			if (methods.Count > 0)
				Actions = methods;
		}

		public TabBookmark FindMatches(Filter filter, int depth)
		{
			TabBookmark tabBookmark = new TabBookmark();
			tabBookmark.Name = this.Name;
			tabBookmark.tabViewSettings = new TabViewSettings();

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
						SelectedRow selectedRow = new SelectedRow();
						selectedRow.rowIndex = -1;
						selectedRow.obj = obj;
						tabDataSettings.SelectedRows.Add(selectedRow);
						tabBookmark.selected.Add(obj);
					}
					else if (depth >= 0)
					{
						TabModel tabModel = Create(obj.ObjectToString(), obj);
						if (tabModel != null)
						{
							TabBookmark childNode = tabModel.FindMatches(filter, depth);
							if (childNode.selected.Count > 0)
							{
								childNode.tabModel = tabModel;
								SelectedRow selectedRow = new SelectedRow();
								selectedRow.rowIndex = -1;
								selectedRow.obj = obj;
								tabDataSettings.SelectedRows.Add(selectedRow);
								tabBookmark.tabChildBookmarks.Add(childNode.Name, childNode);
								tabBookmark.selected.Add(obj);
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
		public static bool ObjectHasChildren(object obj)
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
			return true;
		}
	}
}

/*

*/