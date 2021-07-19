using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Atlas.Tabs
{
	// Generates an event when the SelectedItems change
	public interface ITabSelector
	{
		IList SelectedItems { get; }

		event EventHandler<EventArgs> OnSelectionChanged;
	}

	public interface ISelectedItem
	{
		object SelectedItem { get; set; }
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
		public object Object { get; set; }
		public bool Fill { get; set; }
	}

	public enum AutoSelectType
	{
		None,
		FirstSavedOrNew,
		AnyNewOrSaved, // Restores saved if multiple, otherwise first
		First,
	}

	public class TabModel
	{
		public string Id { get; set; } // todo: Unique key for bookmarks?
		public string Name { get; set; } = "<TabModel>";
		public string Notes { get; set; } // not used anymore
		public object Object { get; set; } // optional

		public AutoSelectType AutoSelect { get; set; } = AutoSelectType.FirstSavedOrNew;

		public bool Editing { get; set; }
		public bool Skippable { get; set; }

		public int MinDesiredWidth { get; set; } = 0;
		public int MaxDesiredWidth { get; set; } = 1500;

		public IList Actions { get; set; }
		public TaskInstanceCollection Tasks { get; set; } = new TaskInstanceCollection();

		public List<IList> ItemList { get; set; } = new List<IList>();

		public List<TabObject> Objects { get; set; } = new List<TabObject>();
		//public List<ITabControl> CustomTabControls { get; set; } = new List<ITabControl>(); // should everything be a custom control? tabControls?

		public IList Items
		{
			get => ItemList.First();
			set
			{
				ItemList.Clear();
				AddData(value);
			}
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

		public override string ToString() => Name;

		public TabModel()
		{
		}

		public TabModel(string name)
		{
			Name = name;
		}

		public static TabModel Create(string name, object obj)
		{
			if (TabUtils.ObjectHasLinks(obj) == false)
				return null;

			var tabModel = new TabModel(name);
			tabModel.AddData(obj);
			if (tabModel.ItemList.Count == 0)
				return null;

			return tabModel;
		}

		public TabObject AddObject(object obj, bool fill = false)
		{
			if (obj == null)
				obj = "(null)";

			if (obj is ChartSettings)
				MinDesiredWidth = 800;

			var tabObject = new TabObject()
			{
				Object = obj,
				Fill = fill,
			};

			Objects.Add(tabObject);

			return tabObject;
		}

		public void AddData(object obj)
		{
			Object = obj;
			if (obj == null)
				return;

			Type type = obj.GetType();
			if (type.Assembly.ManifestModule.ScopeName == "Lazy")
				type = type.BaseType; // Use original type for lazy loaded serializer wrapper classes, so properties appear in the same MetadataToken order

			if (obj is IItemCollection itemCollection)
			{
				CustomSettingsPath = CustomSettingsPath ?? itemCollection.CustomSettingsPath;
			}

			if (obj is IList iList)
			{
				Type elementType = type.GetElementTypeForAll();
				if (elementType != null)
				{
					List<PropertyInfo> visibleProperties = elementType.GetVisibleProperties();
					if (elementType == typeof(string))
					{
						// string properties don't display well
						AddEnumerable((IEnumerable)obj);
					}
					else if (type.GenericTypeArguments.Length > 0 && visibleProperties.Count > 0)
					{
						// list element type has properties (should check if they're visible properties?)
						ItemList.Add(iList);
					}
					/*else if (elementType.IsPrimitive)
					{
						// todo: create a List<elementType>
					}
					else*/
					else if (visibleProperties.Count == 0)
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
					UpdateSkippable(elementType);
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
			else if (TabUtils.ObjectHasLinks(obj))
			{
				// show as Name/Value columns for fields and properties
				AddObject(obj, type);
			}
		}

		private void UpdateSkippable(Type elementType)
		{
			// skip over single items that will take up lots of room (always show ListItems though)
			Skippable = false;
			/*if (ItemList is ItemCollection<> itemCollection)
			{
			}*/
			if (ItemList[0].Count == 1)
			{
				if (ItemList[0] is IItemCollection itemCollection && itemCollection.Skippable == false)
					return;

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
		}

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
		private void AddObject(object obj, Type type)
		{
			var itemCollection = ListMember.Create(obj);
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
			var visibleMethods = ListMethod.Create(Object);

			// Add any methods that return a Task object
			var methods = new ItemCollection<TaskCreator>();
			foreach (ListMethod listMethod in visibleMethods)
			{
				// todo: check parameter types, assuming Log param now
				/*if (methodInfo.IsPublic && methodInfo.ReturnType.IsAssignableFrom(typeof(Task)))
				{
					//methods.Add(new TaskMethod(methodInfo, Object));
				}*/

				methods.Add(new TaskDelegate(listMethod.Name, (TaskDelegate.CallAction)Delegate.CreateDelegate(typeof(TaskDelegate.CallAction), listMethod.MethodInfo)));
			}
			if (methods.Count > 0)
				Actions = methods;
		}

		public TabBookmark FindMatches(Filter filter, int depth)
		{
			var tabBookmark = new TabBookmark()
			{
				Name = Name,
				ViewSettings = new TabViewSettings(),
			};

			depth--;
			foreach (IList iList in ItemList)
			{
				List<PropertyInfo> visibleProperties = TabDataSettings.GetVisibleElementProperties(iList);

				var tabDataSettings = new TabDataSettings();
				tabBookmark.ViewSettings.TabDataSettings.Add(tabDataSettings);

				foreach (object obj in iList)
				{
					if (filter.Matches(obj, visibleProperties))
					{
						var selectedRow = new SelectedRow()
						{
							RowIndex = -1,
							Object = obj,
						};
						tabDataSettings.SelectedRows.Add(selectedRow);
						tabBookmark.SelectedObjects.Add(obj);
					}
					else if (depth >= 0)
					{
						TabModel tabModel = Create(obj.Formatted(), obj);
						if (tabModel != null)
						{
							TabBookmark childNode = tabModel.FindMatches(filter, depth);
							if (childNode.SelectedObjects.Count > 0)
							{
								childNode.TabModel = tabModel;
								var selectedRow = new SelectedRow()
								{
									RowIndex = -1,
									Object = obj,
								};
								tabDataSettings.SelectedRows.Add(selectedRow);
								tabBookmark.ChildBookmarks.Add(childNode.Name, childNode);
								tabBookmark.SelectedObjects.Add(obj);
							}
						}
					}
				}
			}

			return tabBookmark;
		}
	}
}
