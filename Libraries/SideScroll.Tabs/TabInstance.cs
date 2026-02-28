using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Logs;
using SideScroll.Serialize;
using SideScroll.Serialize.Atlas.Schema;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Settings;
using SideScroll.Tasks;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace SideScroll.Tabs;

/// <summary>
/// Interface for tabs that load data asynchronously
/// </summary>
public interface ITabAsync
{
	/// <summary>
	/// Loads the tab data asynchronously into the model
	/// This method gets called before Load() and LoadUI()
	/// </summary>
	Task LoadAsync(Call call, TabModel model);
}

/// <summary>
/// Abstract base class for tab instances that load asynchronously
/// </summary>
public abstract class TabInstanceAsync : TabInstance, ITabAsync
{
	/// <summary>
	/// Loads the tab data asynchronously into the model
	/// This method gets called before Load() and LoadUI()
	/// </summary>
	public abstract Task LoadAsync(Call call, TabModel model);
}

/// <summary>
/// Tab instance that wraps an ILoadAsync method for asynchronous data loading
/// </summary>
public class TabInstanceLoadAsync(ILoadAsync loadAsync) : TabInstance, ITabAsync
{
	/// <summary>
	/// The async load method to execute
	/// </summary>
	public ILoadAsync LoadMethod => loadAsync;

	/// <summary>
	/// Executes the async load method and adds the result to the model
	/// </summary>
	public async Task LoadAsync(Call call, TabModel model)
	{
		Task task = LoadMethod.LoadAsync(call);
		await task.ConfigureAwait(false);
		object result = ((dynamic)task).Result;
		model.AddData(result);
	}
}

/// <summary>
/// Tab instance that creates a tab asynchronously using ITabCreatorAsync
/// </summary>
public class TabCreatorAsync(ITabCreatorAsync creatorAsync) : TabInstance, ITabAsync
{
	/// <summary>
	/// The tab creator that will generate the tab asynchronously
	/// </summary>
	public ITabCreatorAsync CreatorAsync => creatorAsync;

	private TabInstance? _innerChildInstance;

	/// <summary>
	/// Creates the tab asynchronously and loads it into the model
	/// </summary>
	public async Task LoadAsync(Call call, TabModel model)
	{
		ITab? tab = await CreatorAsync.CreateAsync(call);
		if (tab == null)
			return;

		_innerChildInstance = CreateChildTab(tab);
		if (_innerChildInstance is ITabAsync tabAsync)
		{
			await tabAsync.LoadAsync(call, model);
		}
	}

	/// <summary>
	/// Delegates LoadUI to the inner child instance
	/// </summary>
	public override void LoadUI(Call call, TabModel model)
	{
		_innerChildInstance!.LoadUI(call, model);
	}
}

/// <summary>
/// An instance of a TabModel, created by TabView. Manages tab lifecycle, data loading, bookmarks, and child tabs
/// </summary>
public class TabInstance : IDisposable
{
	/// <summary>
	/// The bookmark name used for saving the current tab state
	/// </summary>
	public const string CurrentBookmarkName = "Current";

	/// <summary>
	/// Maximum number of items to preload to avoid freezing the UI
	/// </summary>
	public static int MaxPreloadItems { get; set; } = 50;

	/// <summary>
	/// The project this tab instance belongs to
	/// </summary>
	public Project Project { get; set; }

	/// <summary>
	/// The ITab that created this instance
	/// </summary>
	public ITab? iTab { get; set; }

	/// <summary>
	/// Task instance for managing async operations in this tab
	/// </summary>
	public TaskInstance TaskInstance { get; } = new();

	/// <summary>
	/// The data model for this tab containing items, objects, and settings
	/// </summary>
	public TabModel Model { get; private set; } = new();

	/// <summary>
	/// Display label for the tab
	/// </summary>
	public string Label
	{
		get => Model.Name;
		set => Model.Name = value;
	}

	/// <summary>
	/// Optional loading message to display while the tab is being initialized
	/// </summary>
	public string? LoadingMessage { get; init; }

	/// <summary>
	/// Shortcut to access project data repositories
	/// </summary>
	public ProjectDataRepos Data => Project.Data;

	/// <summary>
	/// View settings including width, selection, and filter information
	/// </summary>
	public TabViewSettings TabViewSettings { get; set; } = new();

	/// <summary>
	/// Current bookmark state for this tab
	/// </summary>
	public TabBookmark? TabBookmark { get; set; }

	/// <summary>
	/// The bookmark that was loaded when the tab was created
	/// </summary>
	public TabBookmark? TabBookmarkLoaded { get; set; }

	/// <summary>
	/// The parent selection that points to this tab
	/// </summary>
	public SelectedRow? SelectedRow { get; set; }

	/// <summary>
	/// Nesting depth of this tab in the hierarchy (root = 1)
	/// </summary>
	public int Depth => 1 + (ParentTabInstance?.Depth ?? 0);

	/// <summary>
	/// Parent tab instance if this is a child tab
	/// </summary>
	public TabInstance? ParentTabInstance { get; set; }

	/// <summary>
	/// Dictionary of child tab instances created from selected items
	/// </summary>
	public Dictionary<object, TabInstance> ChildTabInstances { get; set; } = [];

	/// <summary>
	/// UI synchronization context for posting actions to the UI thread
	/// </summary>
	public SynchronizationContext UiContext { get; set; }

	/// <summary>
	/// Bookmark node used for filtering search results
	/// </summary>
	public TabBookmark? FilterBookmarkNode { get; set; }

	/// <summary>
	/// Event arguments for single item selection
	/// </summary>
	public class ItemSelectedEventArgs(object obj) : EventArgs
	{
		/// <summary>
		/// The selected object
		/// </summary>
		public object Object => obj;

		public override string? ToString() => Object?.ToString();
	}

	/// <summary>
	/// Event arguments for multiple items selection
	/// </summary>
	public class ItemsSelectedEventArgs(IList list) : EventArgs
	{
		/// <summary>
		/// The list of selected items
		/// </summary>
		public IList List => list;
	}

	/// <summary>
	/// Event arguments for clipboard copy operations
	/// </summary>
	public class CopyToClipboardEventArgs(string text) : EventArgs
	{
		/// <summary>
		/// The text to copy to clipboard
		/// </summary>
		public string Text => text;
	}

	/// <summary>
	/// Event raised when the tab needs to refresh its controls and settings
	/// </summary>
	public event EventHandler<EventArgs>? OnRefresh;

	/// <summary>
	/// Event raised when the tab needs to reload its data
	/// </summary>
	public event EventHandler<EventArgs>? OnReload;

	/// <summary>
	/// Event raised when the tab model has changed
	/// </summary>
	public event EventHandler<EventArgs>? OnModelChanged;

	/// <summary>
	/// Event raised when a bookmark should be loaded
	/// </summary>
	public event EventHandler<EventArgs>? OnLoadBookmark;

	/// <summary>
	/// Event raised when the selection should be cleared
	/// </summary>
	public event EventHandler<EventArgs>? OnClearSelection;

	/// <summary>
	/// Event raised when items should be selected
	/// </summary>
	public event EventHandler<ItemsSelectedEventArgs>? OnSelectItems;

	/// <summary>
	/// Event raised when the selection has changed
	/// </summary>
	public event EventHandler<ItemSelectedEventArgs>? OnSelectionChanged;

	/// <summary>
	/// Event raised when an item has been modified
	/// </summary>
	public event EventHandler<EventArgs>? OnModified;

	/// <summary>
	/// Event raised when the tab should resize
	/// </summary>
	public event EventHandler<EventArgs>? OnResize;

	/// <summary>
	/// Event raised when validation is requested
	/// </summary>
	public event EventHandler<EventArgs>? OnValidate;

	/// <summary>
	/// Event raised when text should be copied to clipboard
	/// </summary>
	public event EventHandler<CopyToClipboardEventArgs>? OnCopyToClipboard;

	/// <summary>
	/// Default action to execute when Enter key is pressed
	/// </summary>
	public Action? DefaultAction { get; set; }

	// Relative paths for where all the TabSettings get stored, primarily used for loading future defaults
	// paths get hashed later to avoid having to encode and super long names breaking path limits
	private string? CustomPath => (Model.CustomSettingsPath != null) ? "Custom/" + GetType().GetAssemblyQualifiedShortName() + "/" + Model.CustomSettingsPath : null;
	private string TabPath => "Tab/" + GetType().GetAssemblyQualifiedShortName() + "/" + Model.ObjectTypePath;
	//private string TabPath => "Tab/" + GetType().GetAssemblyQualifiedShortName() + "/" + Model.ObjectTypePath + "/" + Label;
	// deprecate?
	private string TypeLabelPath => "TypePath/" + Model.ObjectTypePath + "/" + Label;
	private string TypePath => "Type/" + Model.ObjectTypePath;

	/// <summary>
	/// Whether the tab has finished loading its data and UI
	/// </summary>
	public bool IsLoaded { get; private set; }

	/// <summary>
	/// Whether Load has been called. Used by the view to track loading state
	/// </summary>
	public bool LoadCalled { get; set; }

	/// <summary>
	/// Whether the model is static and shouldn't be cleared on reload
	/// </summary>
	public bool StaticModel { get; set; }

	/// <summary>
	/// Whether to show all tasks or only tasks with errors
	/// </summary>
	public bool ShowTasks { get; set; }

	/// <summary>
	/// Whether this is a root tab instance
	/// </summary>
	public bool IsRoot { get; set; }

	/// <summary>
	/// The currently selected items in the tab
	/// </summary>
	public IList? SelectedItems { get; set; }

	/// <summary>
	/// Data repository instance for bookmarks to use for saving/loading values
	/// </summary>
	protected IDataRepoInstance? DataRepoInstance { get; set; }

	/// <summary>
	/// Gets the root tab instance by traversing up the parent chain
	/// </summary>
	public TabInstance RootInstance => ParentTabInstance?.RootInstance ?? this;

	private bool _settingLoaded;

	private bool _disposed;

	public override string ToString() => Label;

	/// <summary>
	/// Initializes a new tab instance with a new project
	/// </summary>
	public TabInstance()
	{
		Project = new();

		InitializeContext();
	}

	/// <summary>
	/// Initializes a new tab instance with a specific project and static model
	/// </summary>
	public TabInstance(Project project, TabModel model)
	{
		Project = project;
		Model = model;
		StaticModel = true;

		InitializeContext();
	}

	/// <summary>
	/// Creates a child tab instance from an ITab
	/// </summary>
	public TabInstance CreateChildTab(ITab tab)
	{
		TabInstance tabInstance = tab.Create();

		if (tabInstance.Project.LinkType == null)
		{
			tabInstance.Project = Project;
		}
		tabInstance.iTab = tab;
		tabInstance.ParentTabInstance = this;
		//tabInstance.taskInstance = taskInstance.AddSubTask(taskInstance.call); // too slow?
		return tabInstance;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			// Unsubscribe from events
			OnRefresh = null;
			OnReload = null;
			OnModelChanged = null;
			OnLoadBookmark = null;
			OnClearSelection = null;
			OnSelectItems = null;
			OnSelectionChanged = null;
			OnModified = null;
			OnResize = null;
			OnValidate = null;
			OnCopyToClipboard = null;

			// Dispose child instances
			foreach (TabInstance childInstance in ChildTabInstances.Values)
			{
				childInstance.Dispose();
			}
			ChildTabInstances.Clear();

			// Clear model
			if (!StaticModel)
			{
				Model.Clear();
			}

			// Cancel tasks
			foreach (TaskInstance taskInstance in Model.Tasks)
			{
				taskInstance.Cancel();
			}
			TaskInstance.Cancel();
		}

		_disposed = true;
	}

	public virtual void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	[MemberNotNull(nameof(UiContext))]
	private void InitializeContext()
	{
		UiContext ??= SynchronizationContext.Current ?? new();
	}

	private static void ActionCallback(object? state)
	{
		Action action = (Action)state!;
		action.Invoke();
	}

	/// <summary>
	/// Posts an action to the UI thread
	/// </summary>
	public void Post(Action action)
	{
		UiContext.Post(ActionCallback, action);
	}

	/// <summary>
	/// Posts a callback to the UI thread with optional parameter
	/// </summary>
	public void Post(SendOrPostCallback callback, object? param = null)
	{
		UiContext.Post(callback, param);
	}

	/// <summary>
	/// Posts an action to the UI thread with call timing
	/// </summary>
	public void Post(Call call, Action action)
	{
		using var callTimer = call.Timer(action.ToString());

		UiContext.Post(ActionCallback, action);
	}

	/// <summary>
	/// Posts a parameterized action to the UI thread
	/// </summary>
	public void Post(CallActionParams callAction, params object[] objects)
	{
		Post(null, callAction, objects);
	}

	/// <summary>
	/// Posts a parameterized action to the UI thread with optional call context
	/// </summary>
	public void Post(Call? call, CallActionParams callAction, params object[] objects)
	{
		var taskDelegate = new TaskDelegateParams(call, callAction.Method.Name, callAction, false, null, objects);
		UiContext.Post(CallActionParamsCallback, taskDelegate);
	}

	private void CallActionParamsCallback(object? state)
	{
		var taskDelegate = (TaskDelegateParams)state!;
		StartTask(taskDelegate, false);
	}

	/// <summary>
	/// Creates a task instance from a task creator
	/// </summary>
	public TaskInstance CreateTask(TaskCreator taskCreator, bool showTask, Call? call = null)
	{
		call ??= new Call(taskCreator.Label);
		TaskInstance taskInstance = taskCreator.Create(call);
		AddTask(taskInstance, showTask);
		return taskInstance;
	}

	/// <summary>
	/// Creates and starts a task from a task creator
	/// </summary>
	public TaskInstance StartTask(TaskCreator taskCreator, bool showTask, Call? call = null)
	{
		TaskInstance taskInstance = CreateTask(taskCreator, showTask, call);
		taskInstance.Start();
		return taskInstance;
	}

	/// <summary>
	/// Creates and starts a task from a synchronous action
	/// </summary>
	public TaskInstance StartTask(CallAction callAction, bool useTask, bool showTask)
	{
		var taskDelegate = new TaskDelegate(callAction, useTask);
		return StartTask(taskDelegate, showTask);
	}

	/// <summary>
	/// Creates and starts a task from an asynchronous action
	/// </summary>
	public TaskInstance StartAsync(CallActionAsync callAction, Call? call = null, bool showTask = false)
	{
		var taskDelegate = new TaskDelegateAsync(callAction, true);
		return StartTask(taskDelegate, showTask, call);
	}

	/// <summary>
	/// Creates and starts a task from a parameterized action
	/// </summary>
	public TaskInstance StartTask(CallActionParams callAction, bool useTask, bool showTask, params object[] objects)
	{
		var taskDelegate = new TaskDelegateParams(null, callAction.Method.Name.WordSpaced(), callAction, useTask, null, objects);
		return StartTask(taskDelegate, showTask);
	}

	/// <summary>
	/// Adds a task to the tab's task collection
	/// </summary>
	public void AddTask(TaskInstance taskInstance, bool showTask)
	{
		taskInstance.ShowTask = showTask || ShowTasks;
		if (taskInstance.ShowTask)
		{
			Model.Tasks.Add(taskInstance);
		}
	}

	/// <summary>
	/// Gets the list of properties for this tab instance
	/// </summary>
	protected ItemCollection<ListProperty> GetListProperties()
	{
		return ListProperty.Create(this, false);
	}

	/// <summary>
	/// Gets the list of members for this tab instance
	/// </summary>
	protected ItemCollection<ListMember> GetListMembers()
	{
		return ListMember.Create(this, false);
	}

	/// <summary>
	/// Gets the list items for this tab instance
	/// </summary>
	protected ItemCollection<IListItem> GetListItems()
	{
		return IListItem.Create(this, false);
	}

	private MethodInfo? GetDerivedLoadMethod(string name, int paramCount)
	{
		try
		{
			Type type = GetType(); // gets derived type
			return type.GetMethods()
				.FirstOrDefault(m =>
					m.Name == name &&
					m.DeclaringType != typeof(TabInstance) &&
					m.GetParameters().Length == paramCount);
		}
		catch (Exception)
		{
		}
		return null;
	}

	private bool HasLoadMethod => GetDerivedLoadMethod(nameof(Load), 2) != null;
	private bool HasLoadUIMethod => GetDerivedLoadMethod(nameof(LoadUI), 2) != null;

	/// <summary>
	/// Override this method to load the tab's data into the model. Called on a background thread
	/// </summary>
	public virtual void Load(Call call, TabModel model)
	{
	}

	/// <summary>
	/// Override this method to add UI-specific objects to the model. Called on the UI thread
	/// </summary>
	public virtual void LoadUI(Call call, TabModel model)
	{
	}

	/// <summary>
	/// Reinitializes the tab by reloading data and UI
	/// </summary>
	public void Reinitialize(bool force)
	{
		if (!force && IsLoaded)
			return;

		IsLoaded = true;
		LoadCalled = false; // allow TabView to reload

		StartAsync(ReinitializeAsync, TaskInstance.Call);
	}

	public async Task ReinitializeAsync(Call call)
	{
		_settingLoaded = false;
		TabModel model = Model;
		if (!StaticModel)
		{
			model = await LoadModelAsync(call);
		}

		try
		{
			Preload(model);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}

		var subTask = call.AddSubTask("Loading");
		Post(() => LoadModelUI(subTask.Call, model)); // Some controls need to be created on the UI context
	}

	private async Task<TabModel> LoadModelAsync(Call call)
	{
		var model = new TabModel(Model.Name)
		{
			Tasks = new(Model.Tasks),
		};

		if (this is ITabAsync tabAsync)
		{
			try
			{
				await tabAsync.LoadAsync(call, model);
			}
			catch (Exception e)
			{
				model.AddData(e);
				//model.Tasks.Add(call.TaskInstance);
			}
		}

		if (HasLoadMethod)
		{
			try
			{
				Load(call, model);
			}
			catch (Exception e)
			{
				call.Log.Add(e);
				model.AddData(e);
			}

			// Posted Log messages won't have taken affect here yet
			// Task.OnFinished hasn't always been called by this point
			if ((model.ShowTasks || call.Log.Level >= LogLevel.Error)
				&& !model.Tasks.Contains(call.TaskInstance!))
			{
				model.Tasks.Add(call.TaskInstance!);
			}
		}
		return model;
	}

	// Preload properties in a background thread so the UI isn't blocked
	// Todo: Make an async version of this for Task<T> Member(Call call)
	private void Preload(TabModel model)
	{
		for (int i = 0; i < model.ItemList.Count; i++)
		{
			IList iList = model.ItemList[i];
			Type listType = iList.GetType();
			Type? elementType = listType.GetElementTypeForAll();
			if (elementType == null) continue;

			var tabDataSettings = TabViewSettings.GetData(i);
			TabDataColumns dataColumns = new(tabDataSettings.ColumnNameOrder);
			List<TabPropertyColumn> propertyColumns = dataColumns.GetPropertyColumns(elementType);
			int itemCount = 0;
			foreach (object obj in iList)
			{
				if (obj != null)
				{
					foreach (var propertyColumn in propertyColumns)
					{
						if (propertyColumn.PropertyInfo.DeclaringType?.IsAbstract == true)
							continue;

						propertyColumn.PropertyInfo.GetValue(obj);
					}
				}
				itemCount++;
				if (itemCount > MaxPreloadItems)
					break;
			}

			if (iList is ItemCollection<ListProperty> propertyList)
			{
				model.ItemList[i] = ListProperty.Sort(propertyList);
			}

			if (iList is ItemCollection<ListMember> memberList)
			{
				model.ItemList[i] = ListMember.Sort(memberList);
			}
		}
	}

	private void LoadModelUI(Call call, TabModel model)
	{
		// Set the model before calling LoadUI() in case the Settings are needed
		// Load() initializes the tabModel.Object & CustomSettingsPath which gets used for the settings path
		Model = model;

		if (HasLoadUIMethod)
		{
			try
			{
				LoadUI(call, model);
			}
			catch (Exception e)
			{
				call.Log.Add(e);
				model.AddData(e);
			}
		}

		// Set the context to the UI for items that support it
		foreach (IList iList in model.ItemList)
		{
			if (iList is IContext context)
			{
				context.InitializeContext(true);
			}
		}

		try
		{
			LoadSettings(false);
			OnModelChanged?.Invoke(this, EventArgs.Empty);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}

		IsLoaded = true;
	}

	/// <summary>
	/// Triggers the model changed event to refresh the UI
	/// </summary>
	public void ReloadModel()
	{
		if (OnModelChanged != null)
		{
			var onModelChanged = OnModelChanged; // create temporary copy since this gets delayed
			UiContext.Send(_ => onModelChanged(this, EventArgs.Empty), null);
		}
	}

	/// <summary>
	/// Reloads the tab data by calling Load() and then refreshing
	/// </summary>
	public void Reload(bool reloadBookmark = false)
	{
		IsLoaded = false;
		LoadCalled = false;

		if (reloadBookmark)
		{
			TabBookmark = TabBookmarkLoaded;
		}

		if (OnReload != null)
		{
			if (this is ITabAsync tabAsync)
			{
				OnReload.Invoke(tabAsync, EventArgs.Empty);
			}
			else
			{
				UiContext.Send(_ => OnReload(this, EventArgs.Empty), null);
			}
		}
	}

	/// <summary>
	/// Reloads controls and settings without reloading data
	/// </summary>
	public void Refresh()
	{
		IsLoaded = false;

		if (OnRefresh != null)
		{
			var onRefresh = OnRefresh; // create temporary copy since this gets delayed
			UiContext.Send(_ => onRefresh(this, EventArgs.Empty), null);
		}
	}

	/// <summary>
	/// Triggers the resize event for the tab
	/// </summary>
	public void Resize()
	{
		if (OnResize != null)
		{
			var onResize = OnResize; // create temporary copy since this gets delayed
			UiContext.Send(_ => onResize(this, EventArgs.Empty), null);
		}
	}

	/// <summary>
	/// Whether the tab can be skipped/collapsed in the UI when it contains a single item
	/// </summary>
	public bool Skippable
	{
		get
		{
			//if (TabViewSettings.SelectionType == SelectionType.User && TabViewSettings.SelectedRows.Count == 0) // Need to split apart user selected rows?
			if (TabViewSettings.SelectionType != SelectionType.None && TabViewSettings.SelectedRows.Count == 0)
				return false;

			// Only data is skippable?
			if (Model.Objects.Count > 0 || Model.ItemList.Count == 0 || Model.ItemList[0].Count != 1)
				return false;

			var skippableAttribute = Model.ItemList[0][0]!.GetType().GetCustomAttribute<SkippableAttribute>();
			if (skippableAttribute == null && Model.Actions != null && Model.Actions.Count > 0)
				return false;

			return Model.Skippable;
		}
	}

	/// <summary>
	/// Selects a single item in the tab
	/// </summary>
	public void SelectItem(object? obj)
	{
		SelectItems(new List<object?> { obj });
	}

	/// <summary>
	/// Selects multiple items in the tab
	/// </summary>
	public void SelectItems(IList items)
	{
		if (OnSelectItems != null)
		{
			UiContext.Send(_ => OnSelectItems(this, new ItemsSelectedEventArgs(items)), null);
		}
		else
		{
			TabBookmark = TabBookmark.CreateList(items);
		}
	}

	/// <summary>
	/// Selects items by navigating a path of labels
	/// </summary>
	public void SelectPath(params string[] labels)
	{
		TabBookmark tabBookmark = new();
		tabBookmark.SelectPath(labels);
		SelectBookmark(tabBookmark);
	}

	/// <summary>
	/// Clears all selected items in the tab
	/// </summary>
	public void ClearSelection()
	{
		if (OnClearSelection != null)
		{
			UiContext.Send(_ => OnClearSelection(this, EventArgs.Empty), null);
		}
	}

	/// <summary>
	/// Whether this tab can be bookmarked as a link
	/// </summary>
	public bool IsLinkable
	{
		get
		{
			Type? type = iTab?.GetType();
			if (type == null)
				return false;

			return type.GetCustomAttribute<PublicDataAttribute>() != null &&
				TypeSchema.TypeHasConstructor(type);
		}
	}

	/// <summary>
	/// Creates a bookmark from the current tab state including selections and filters
	/// </summary>
	public virtual Bookmark CreateBookmark()
	{
		Bookmark bookmark = new()
		{
			Name = Label,
			TabType = iTab?.GetType(),
			TabBookmark = { IsRoot = true },
			CreatedTime = DateTime.Now,
		};
		GetBookmark(bookmark.TabBookmark);
		bookmark = bookmark.DeepClone(TaskInstance.Call); // Sanitize and test bookmark
		return bookmark;
	}

	/// <summary>
	/// Creates a bookmark and adds it to the navigation history
	/// </summary>
	public Bookmark CreateNavigatorBookmark()
	{
		Bookmark bookmark = RootInstance.CreateBookmark();
		Project.Navigator.Append(bookmark, true);
		return bookmark;
	}

	/// <summary>
	/// Populates a TabBookmark with the current tab state including child tabs and selections
	/// </summary>
	public virtual void GetBookmark(TabBookmark tabBookmark)
	{
		tabBookmark.Width = TabViewSettings.Width;
		var viewSettings = TabViewSettings.TryDeepClone() ?? new();

		/*if (DataRepoInstance != null)
		{
			foreach (var item in TabViewSettings.SelectedRows)
			{
				var dataRepoItem = new DataRepoItem()
				{
					GroupId = DataRepoInstance.GroupId,
					Key = item.dataKey,
					Value = item.dataValue,
				};
				tabBookmark.DataRepoItems.Add(dataRepoItem);
			}
		}*/

		if (iTab is ITabContainer tabContainer)
		{
			iTab = tabContainer.Tab;
		}

		if (iTab != null)
		{
			Type type = iTab.GetType();
			if (type.GetCustomAttribute<PrivateDataAttribute>() != null)
			{
				return;
			}

			if (type.GetCustomAttribute<TabRootAttribute>() != null)
			{
				tabBookmark.IsRoot = true;
				tabBookmark.Tab = iTab;
			}
			else if (type.GetCustomAttribute<PublicDataAttribute>() != null &&
				(tabBookmark.IsRoot || IsRoot))
			{
				tabBookmark.Tab = iTab;
			}
		}

		var lookup = ChildTabInstances.Values.ToDictionary(t => (t.SelectedRow ?? new()).ToString() ?? "");

		foreach (TabDataSettings dataSettings in viewSettings.TabDataSettings)
		{
			TabDataBookmark dataBookmark = new()
			{
				Filter = dataSettings.Filter,
				ColumnNameOrder = dataSettings.ColumnNameOrder,
				DataRepoGroupId = DataRepoInstance?.GroupId,
				DataRepoType = DataRepoInstance?.DataType,
				SelectionType = dataSettings.SelectionType,
			};
			tabBookmark.TabDatas.Add(dataBookmark);
			foreach (SelectedRow selectedRow in dataSettings.SelectedRows)
			{
				SelectedRowView selectedRowView = new(selectedRow);
				if (lookup.TryGetValue(selectedRow.ToString() ?? "", out TabInstance? tabInstance))
				{
					tabInstance.GetBookmark(selectedRowView.TabBookmark);
				}
				dataBookmark.SelectedRows.Add(selectedRowView);
			}
		}

		// Add children that don't have parents?
		/*foreach (TabInstance tabInstance in ChildTabInstances.Values)
		{
			string label = tabInstance.SelectedRow?.ToString() ?? tabInstance.Label;
			if (tabBookmark.SelectedRows.Any(s => s.ToString() == label))
				continue;

			var childBookmark = tabBookmark.AddChild(label);
			tabInstance.GetBookmark(childBookmark.TabBookmark);
		}*/
	}

	/// <summary>
	/// Loads a bookmark into the tab
	/// </summary>
	public void LoadBookmark(Bookmark bookmark)
	{
		TabBookmark = null;
		if (bookmark == null) return;
		
		if (iTab != null)
		{
			Type type = iTab.GetType();
			if (type.GetCustomAttribute<PrivateDataAttribute>() != null)
			{
				return;
			}
		}
		SelectBookmark(bookmark.TabBookmark);
	}

	/// <summary>
	/// Selects items and applies filters from a bookmark
	/// </summary>
	public virtual void SelectBookmark(TabBookmark tabBookmark, bool reload = false)
	{
		if (reload)
		{
			ClearSelection();
		}

		TabBookmark = tabBookmark;
		TabViewSettings = tabBookmark.ToViewSettings();

		if (OnLoadBookmark != null)
		{
			UiContext.Send(_ => OnLoadBookmark(this, EventArgs.Empty), null);
		}

		SaveTabSettings();
	}

	private void SaveDefaultBookmark()
	{
		Bookmark bookmark = RootInstance.CreateBookmark(); // create from base Tab
		bookmark.Name = CurrentBookmarkName;
		Data.App.Save(bookmark.Name, bookmark, TaskInstance.Call);
	}

	/// <summary>
	/// Loads the default bookmark if auto-selection is enabled
	/// </summary>
	public void LoadDefaultBookmark()
	{
		if (Project.UserSettings.AutoSelect == false)
			return;

		Bookmark? bookmark = Data.App.Load<Bookmark>(CurrentBookmarkName, TaskInstance.Call);
		if (bookmark != null)
		{
			TabBookmark = bookmark.TabBookmark;
		}
	}

	/// <summary>
	/// Loads tab view settings from bookmark or cache
	/// </summary>
	public TabViewSettings? LoadSettings(bool reload)
	{
		if (_settingLoaded && !reload && TabViewSettings != null)
			return TabViewSettings;

		if (TabBookmark != null)
		{
			TabViewSettings = TabBookmark.ToViewSettings();
		}
		else
		{
			LoadDefaultTabSettings();
		}
		_settingLoaded = true;
		return TabViewSettings;
	}

	/// <summary>
	/// Gets custom data from the current bookmark
	/// </summary>
	public T? GetBookmarkData<T>(string name = TabBookmark.DefaultDataName)
	{
		if (TabBookmark != null)
			return TabBookmark.GetData<T>(name);

		return default;
	}

	/// <summary>
	/// Saves data to the app data repository using interface types instead of actual object type if present
	/// </summary>
	public void SaveData<T>(string key, T obj)
	{
		Data.App.Save<T>(key, obj, TaskInstance.Call);
	}

	/// <summary>
	/// Saves data to the app data repository with a group identifier
	/// </summary>
	public void SaveData<T>(string groupId, string key, T obj)
	{
		Data.App.Save<T>(groupId, key, obj, TaskInstance.Call);
	}

	/// <summary>
	/// Loads data from the app data repository
	/// </summary>
	public T? LoadData<T>(string key) => Data.App.Load<T>(key, TaskInstance.Call);

	/// <summary>
	/// Loads data from the app data repository with a group identifier
	/// </summary>
	public T? LoadData<T>(string groupId, string key) => Data.App.Load<T>(groupId, key, TaskInstance.Call);

	/// <summary>
	/// Loads data from the app data repository or creates it if it doesn't exist
	/// </summary>
	public T LoadOrCreateData<T>(string key) => Data.App.LoadOrCreate<T>(key, TaskInstance.Call);

	/// <summary>
	/// Loads data from the app data repository with a group identifier or creates it if it doesn't exist
	/// </summary>
	public T LoadOrCreateData<T>(string groupId, string key) => Data.App.LoadOrCreate<T>(groupId, key, TaskInstance.Call);

	/// <summary>
	/// Loads the default tab settings and assigns them to TabViewSettings
	/// </summary>
	public TabViewSettings LoadDefaultTabSettings()
	{
		TabViewSettings = GetTabSettings();
		return TabViewSettings;
	}

	/// <summary>
	/// Retrieves tab settings from cache based on custom path or tab type
	/// </summary>
	public TabViewSettings GetTabSettings()
	{
		if (CustomPath != null)
		{
			// It's better to return the default constructor so the Tab autosizes instead of using the saved defaults which might have a width specified
			return Data.Cache.LoadOrCreate<TabViewSettings>(CustomPath, TaskInstance.Call);
		}

		Type type = GetType();
		if (type != typeof(TabInstance))
		{
			// Unique TabInstance
			TabViewSettings? tabViewSettings = Data.Cache.Load<TabViewSettings>(TabPath, TaskInstance.Call);
			if (tabViewSettings != null)
				return tabViewSettings;
		}
		else
		{
			TabViewSettings? tabViewSettings =
				Data.Cache.Load<TabViewSettings>(TypeLabelPath, TaskInstance.Call) ??
				Data.Cache.Load<TabViewSettings>(TypePath, TaskInstance.Call);
			if (tabViewSettings != null)
				return tabViewSettings;
		}

		return new TabViewSettings();
	}

	/// <summary>
	/// Saves the current tab view settings to cache and updates the default bookmark
	/// </summary>
	public void SaveTabSettings()
	{
		if (CustomPath != null)
		{
			Data.Cache.Save(CustomPath, TabViewSettings, TaskInstance.Call);
		}
		else
		{
			Type type = GetType();
			if (type != typeof(TabInstance))
			{
				// Unique TabInstance
				Data.Cache.Save(TabPath, TabViewSettings, TaskInstance.Call);
			}
			else
			{
				Data.Cache.Save(TypeLabelPath, TabViewSettings, TaskInstance.Call);
				Data.Cache.Save(TypePath, TabViewSettings, TaskInstance.Call);
			}
		}
		SaveDefaultBookmark();
	}

	/// <summary>
	/// Detects parent/child loops by checking if an object is owned by this tab or any parent
	/// </summary>
	public bool IsOwnerObject(object? obj)
	{
		if (obj == null)
			return false;

		if (Model.Object == obj)
			return true;

		Type type = obj.GetType();
		if (iTab != null && type == iTab.GetType())
		{
			foreach (PropertyInfo propertyInfo in type.GetProperties())
			{
				if (propertyInfo.GetCustomAttribute<DataKeyAttribute>() != null)
				{
					var objKey = propertyInfo.GetValue(obj);
					if (objKey == null)
						continue;

					var tabKey = propertyInfo.GetValue(iTab);
					// todo: support multiple [Key]s?
					if (objKey == tabKey)
						return true;

					if (objKey is string && propertyInfo.PropertyType == type && Equals(objKey, tabKey))
						return true;
				}
			}
		}

		if (ParentTabInstance != null)
			return ParentTabInstance.IsOwnerObject(obj);

		return false;
	}

	/// <summary>
	/// Notifies that an item has been modified
	/// </summary>
	public void ItemModified()
	{
		OnModified?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Creates a child tab instance with this instance as the parent
	/// </summary>
	public TabInstance CreateChild(TabModel model)
	{
		var childTabInstance = new TabInstance(Project, model)
		{
			ParentTabInstance = this,
		};

		if (TabBookmark != null)
		{
			if (TabBookmark.TryGetValue(model.Name, out TabBookmark? tabChildBookmark))
			{
				childTabInstance.TabBookmark = tabChildBookmark;
			}
		}
		return childTabInstance;
	}

	/// <summary>
	/// Updates the navigation history with the current tab state
	/// </summary>
	public void UpdateNavigator()
	{
		Bookmark bookmark = RootInstance.CreateBookmark(); // create from root Tab
		Project.Navigator.Update(bookmark);
	}

	/// <summary>
	/// Handles selection changed events and raises the OnSelectionChanged event
	/// </summary>
	public void SelectionChanged(object? sender, EventArgs e)
	{
		if (SelectedItems?.Count > 0)
		{
			var itemSelectedEventArgs = new ItemSelectedEventArgs(SelectedItems[0]!);
			OnSelectionChanged?.Invoke(sender, itemSelectedEventArgs);
		}
	}

	/// <summary>
	/// Triggers validation for the tab
	/// </summary>
	public void Validate()
	{
		OnValidate?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Copies text to the clipboard
	/// </summary>
	public void CopyToClipboard(string text)
	{
		OnCopyToClipboard?.Invoke(this, new CopyToClipboardEventArgs(text));
	}

	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		WriteIndented = true
	};

	/// <summary>
	/// Serializes an object to JSON and copies it to the clipboard
	/// </summary>
	public void CopyToClipboard(object? obj)
	{
		string json = JsonSerializer.Serialize(obj, JsonSerializerOptions);
		CopyToClipboard(json);
	}
}
