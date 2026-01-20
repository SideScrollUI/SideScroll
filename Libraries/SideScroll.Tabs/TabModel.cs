using SideScroll.Attributes;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Lists;
using SideScroll.Tasks;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace SideScroll.Tabs;

public class TabObject
{
	public object? Object { get; set; }

	public bool Fill { get; set; } // Stretch to Fill all vertical space
	public bool EnableScrolling { get; set; }

	public override string? ToString() => Object?.ToString();
}

public class ObjectUpdatedEventArgs(object obj) : EventArgs
{
	public object Object => obj;

	public override string? ToString() => Object?.ToString();
}

public class TabFormObject : TabObject
{
	public event EventHandler<ObjectUpdatedEventArgs>? ObjectChanged;
	public event EventHandler<EventArgs>? OnFocus;

	public void Update(object? sender, object obj)
	{
		Object = obj;
		ObjectChanged?.Invoke(sender, new ObjectUpdatedEventArgs(Object!));
	}

	public void Focus(object? sender)
	{
		OnFocus?.Invoke(sender, EventArgs.Empty);
	}
}

public enum AutoSelectType
{
	Any, // 0 to many
	NonEmpty, // 1 - many, or default selection
	None, // Deselect all
}

public class TabModel
{
	// public string? Id { get; set; } // todo: Unique key for bookmarks?
	public string Name { get; set; } = "<TabModel>";
	public object? Object { get; set; } // optional

	// Selects Saved or Default, and then any New
	public AutoSelectType AutoSelectSaved { get; set; } = AutoSelectType.Any;
	public bool AutoSelectDefault { get; set; } = true;
	public bool AutoSelectNew { get; set; } = true;
	public bool ReloadOnThemeChange { get; set; }

	public bool Editing { get; set; }
	public bool Skippable { get; set; } // Will collapse collections that have a single item
	public bool ShowTasks { get; set; } // Will add the Tasks to show logs when an error occurs
	public bool ShowSearch { get; set; } // Show search filter above DataGrids

	public int MinDesiredWidth { get; set; }
	public int MaxDesiredWidth { get; set; } = 1500;

	public SearchFilter? SearchFilter { get; set; } // DataGrid filtering will also update this filter
	public int MaxSearchDepth { get; set; } = 0;

	public IList? Actions { get; set; }
	public TaskInstanceCollection Tasks { get; set; } = [];

	public List<IList> ItemList { get; set; } = [];

	public List<TabObject> Objects { get; set; } = [];
	//public List<ITabControl> CustomTabControls { get; set; } = []; // should everything be a custom control? tabControls?

	public IList? Items
	{
		get => ItemList.FirstOrDefault();
		set
		{
			ItemList.Clear();
			AddData(value);
		}
	}

	public object? DefaultSelectedItem { get; set; }

	// used for saving/loading TabViewSettings
	public string? CustomSettingsPath { get; set; } // Must be set before LoadUI()
	public string ObjectTypePath
	{
		get
		{
			if (Object == null)
				return "(null)";

			Type objType = Object.GetType();
			return objType.GetAssemblyQualifiedShortName();
		}
	}

	public override string ToString() => Name;

	public TabModel() { }

	public TabModel(string name)
	{
		Name = name;
	}

	public static TabModel? Create(string name, object obj)
	{
		if (TabUtils.ObjectHasLinks(obj) == false && obj is not Enum)
			return null;

		var tabModel = new TabModel(name);
		tabModel.AddData(obj);
		if (tabModel.ItemList.Count == 0 && tabModel.Objects.Count == 0)
			return null;

		return tabModel;
	}

	public TabObject AddObject(object? obj, bool fill = false, bool enableScrolling = false)
	{
		obj ??= "(null)";

		if (obj is ChartView)
		{
			ReloadOnThemeChange = true;
			MinDesiredWidth = 800;
		}

		TabObject tabObject = new()
		{
			Object = obj,
			Fill = fill,
			EnableScrolling = enableScrolling,
		};

		Objects.Add(tabObject);

		return tabObject;
	}

	public TabFormObject AddForm(object obj, bool fill = false, bool enableScrolling = false)
	{
		TabFormObject tabObject = new()
		{
			Object = obj,
			Fill = fill,
			EnableScrolling = enableScrolling,
		};

		Objects.Add(tabObject);

		return tabObject;
	}

	public void AddData(object? obj)
	{
		Object = obj;
		if (obj == null)
			return;

		Type type = obj.GetType();
		if (type.Assembly.ManifestModule.ScopeName == "Lazy" && type.BaseType != null)
		{
			type = type.BaseType; // Use original type for lazy loaded serializer wrapper classes, so properties appear in the same MetadataToken order
		}

		if (obj is IItemCollection itemCollection)
		{
			CustomSettingsPath ??= itemCollection.CustomSettingsPath;
		}

		var listItemAttribute = type.GetCustomAttribute<ListItemAttribute>();
		if (listItemAttribute != null)
		{
			ItemList.Add(IListItem.Create(obj, listItemAttribute.IncludeBaseTypes));
			return;
		}

		if (obj is IList iList)
		{
			if (AddList(iList, type))
				return;
		}

		// Can only set one set of columns for Items, so we have to choose
		// Should we always show fields and properties?
		if (obj is IDictionary dictionary)
		{
			// Show as Key/Value columns, change to keys only?
			AddDictionary(dictionary);
		}
		else if (obj is IEnumerable enumerable)
		{
			// show inner type as list (but only one column using a ToString for the label)
			//AddObject(type);
			AddEnumerable(enumerable);
		}
		else if (obj is DataTable dataTable)
		{
			ItemList.Add(dataTable.DefaultView);
		}
		else if (obj is ChartView)
		{
			AddObject(obj);
		}
		else if (obj is Enum enumValue)
		{
			ItemList.Add(ListEnumValue.Create(enumValue));
		}
		else if (TabUtils.ObjectHasLinks(obj))
		{
			// show as Name/Value columns for fields and properties
			AddObjectMembers(obj);
		}
	}

	private bool AddList(IList list, Type listType)
	{
		Type? elementType = listType.GetElementTypeForAll();
		if (elementType == null)
			return false;

		List<PropertyInfo> visibleProperties = elementType.GetVisibleProperties();
		if (elementType == typeof(string))
		{
			// string properties don't display well
			AddEnumerable(list);
		}
		else if (listType.GenericTypeArguments.Length > 0 && visibleProperties.Count > 0)
		{
			// list element type has properties (should check if they're visible properties?)
			ItemList.Add(list);
		}
		else if (list is byte[] byteArray)
		{
			ItemList.Add(ListByte.Create(byteArray));
		}
		/*else if (elementType.IsPrimitive)
		{
			// todo: create a List<elementType>
		}
		else*/
		else if (visibleProperties.Count == 0)
		{
			ItemList.Add(ListToString.Create(list));
		}
		else
		{
			// this doesn't work if there's a null value in the array
			// should we databind columns to Value property in Nullable?
			Type? underlyingType = Nullable.GetUnderlyingType(elementType);
			if (underlyingType != null)
			{
				elementType = underlyingType;
			}

			Type genericType = typeof(ItemCollection<>).MakeGenericType(elementType);
			IList iNewList = (IList)Activator.CreateInstance(genericType)!;
			foreach (object child in list)
			{
				iNewList.Add(child);
			}
			ItemList.Add(iNewList);
		}

		UpdateSkippable(elementType);
		return true;
	}

	private void AddDictionary(IDictionary dictionary)
	{
		List<DictionaryEntry> sortedList = []; // can't sort ItemCollection
		try
		{
			foreach (DictionaryEntry item in dictionary)
			{
				sortedList.Add(item);
			}
		}
		catch (Exception e)
		{
			Debug.WriteLine("Failed to add Dictionary",
				new Tag("Exception", e),
				new Tag("Dictionary", dictionary));
		}

		if (Object is IComparable)
		{
			sortedList = sortedList
				.OrderBy(x => x.Key)
				.ToList();
		}

		ItemList.Add(new ItemCollection<DictionaryEntry>(sortedList));
	}

	public static Type[]? GetInterfaceGenericArguments(Type type, Type genericType)
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

	private void AddEnumerable(IEnumerable enumerable)
	{
		Type type = enumerable.GetType();
		if (type.GenericTypeArguments.Length == 0 || type.GenericTypeArguments[0] == typeof(string))
		{
			ItemList.Add(ListToString.Create(enumerable));
			return;
		}

		Type elementType = GetElementType(type);
		Type genericType = typeof(ItemCollection<>).MakeGenericType(elementType);
		IList iList = (IList)Activator.CreateInstance(genericType)!;
		foreach (var item in enumerable)
		{
			iList.Add(item);
		}
		ItemList.Add(iList);
	}

	// merge with GetElementTypeForAll?
	private static Type GetElementType(Type type)
	{
		Type elementType;
		if (type.IsAssignableToGenericType(typeof(IEnumerable<>)))
		{
			// generic interface might be in interface, not primary class
			Type[] types = GetInterfaceGenericArguments(type, typeof(IEnumerable<>))!;
			elementType = types[0];
		}
		else
		{
			elementType = type.GenericTypeArguments[0];
		}

		return elementType;
	}

	// Adds the fields and properties as one list, and methods as another list (disabled right now)
	private void AddObjectMembers(object obj)
	{
		var itemCollection = ListMember.Create(obj);
		ItemList.Add(itemCollection);

		//AddMethods(obj);
	}

	private void UpdateSkippable(Type elementType)
	{
		// skip over single items that will take up lots of room (always show ListItems though)
		Skippable = false;

		if (ItemList[0].Count == 1 && ItemList[0][0] is object firstItem)
		{
			if (ItemList[0] is IItemCollection itemCollection && !itemCollection.Skippable)
				return;

			var skippableAttribute = firstItem.GetType().GetCustomAttribute<SkippableAttribute>();
			if (skippableAttribute != null)
			{
				Skippable = skippableAttribute.Value;
			}
			else if (firstItem is not ITab && TabDataColumns.GetVisibleProperties(elementType).Count > 1)
			{
				Skippable = true;
			}
		}
	}

	public void Clear()
	{
		Objects.Clear();
		ItemList.Clear();
		Actions = null;
	}

	// todo: split Actions out of ListMethod since those return values?
	/*private void AddMethods(object obj)
	{
		var visibleMethods = ListMethod.Create(obj);

		// Add any methods that return a Task object
		var methods = new ItemCollection<TaskCreator>();
		foreach (ListMethod listMethod in visibleMethods)
		{
			var taskDelegate = new TaskDelegate(Name, (TaskDelegate.CallAction)Delegate.CreateDelegate(typeof(TaskDelegate.CallAction), MethodInfo));
			methods.Add(taskDelegate);
		}
		if (methods.Count > 0)
			Actions = methods;
	}*/

	public TabBookmark FindMatches(Filter filter, int depth)
	{
		TabBookmark tabBookmark = new();

		depth = Math.Min(depth, MaxSearchDepth);
		depth--;
		foreach (IList iList in ItemList)
		{
			List<PropertyInfo> visibleProperties = TabDataColumns.GetVisibleElementProperties(iList);

			TabDataBookmark tabDataBookmark = new();
			tabBookmark.TabDatas.Add(tabDataBookmark);

			foreach (object obj in iList)
			{
				if (filter.Matches(obj, visibleProperties))
				{
					SelectedRow selectedRow = new()
					{
						Object = obj,
					};
					tabDataBookmark.SelectedRows.Add(new(selectedRow));
				}
				else if (depth >= 0)
				{
					TabModel? tabModel = Create(obj.Formatted() ?? "", obj);
					if (tabModel != null)
					{
						TabBookmark childNode = tabModel.FindMatches(filter, depth);
						if (childNode.SelectedRows.Count > 0)
						{
							childNode.TabModel = tabModel;
							SelectedRow selectedRow = new()
							{
								Object = obj,
							};
							tabDataBookmark.SelectedRows.Add(new(selectedRow, childNode));
						}
					}
				}
			}
		}

		return tabBookmark;
	}
}
