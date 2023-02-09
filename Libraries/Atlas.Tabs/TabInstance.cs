using Atlas.Core;
using Atlas.Extensions;
using Atlas.Serialize;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static Atlas.Core.TaskDelegate;
using static Atlas.Core.TaskDelegateAsync;
using static Atlas.Core.TaskDelegateParams;

namespace Atlas.Tabs;

public interface ITab
{
	TabInstance Create();
}

public interface ITabAsync
{
	Task LoadAsync(Call call, TabModel model);
}

public interface IInnerTab
{
	ITab? Tab { get; }
}

public class TabInstanceLoadAsync : TabInstance, ITabAsync
{
	public readonly ILoadAsync LoadMethod;

	public TabInstanceLoadAsync(ILoadAsync loadAsync)
	{
		LoadMethod = loadAsync;
	}

	public async Task LoadAsync(Call call, TabModel model)
	{
		Task task = LoadMethod.LoadAsync(call);
		await task.ConfigureAwait(false);
		object result = ((dynamic)task).Result;
		model.AddData(result);
	}
}

public class TabCreatorAsync : TabInstance, ITabAsync
{
	public readonly ITabCreatorAsync CreatorAsync;

	private TabInstance? _innerChildInstance;

	public TabCreatorAsync(ITabCreatorAsync creatorAsync)
	{
		CreatorAsync = creatorAsync;
	}

	public async Task LoadAsync(Call call, TabModel model)
	{
		ITab? iTab = await CreatorAsync.CreateAsync(call);
		if (iTab == null)
			return;

		_innerChildInstance = CreateChildTab(iTab);
		if (_innerChildInstance is ITabAsync tabAsync)
			await tabAsync.LoadAsync(call, model);
	}

	public override void LoadUI(Call call, TabModel model)
	{
		_innerChildInstance!.LoadUI(call, model);
	}
}

public abstract class TabInstanceAsync : TabInstance, ITabAsync
{
	public abstract Task LoadAsync(Call call, TabModel model);
}

//	An Instance of a TabModel, created by TabView
public class TabInstance : IDisposable
{
	private const int MaxPreloadItems = 50; // preload all rows that might be visible to avoid freezing UI

	public const string CurrentBookmarkName = "Current";

	public Project Project { get; set; }
	public ITab? iTab; // Collision with derived Tab
	public TaskInstance TaskInstance { get; set; } = new();
	public TabModel Model { get; set; } = new();
	public string Label
	{ 
		get => Model.Name;
		set => Model.Name = value;
	}

	public DataRepo DataApp => Project.DataApp;
	public DataRepo DataTemp => Project.DataTemp;

	public TabViewSettings TabViewSettings = new();
	public TabBookmark? TabBookmark { get; set; }
	public TabBookmark? TabBookmarkLoaded { get; set; }
	public SelectedRow? SelectedRow { get; set; } // The parent selection that points to this tab

	public int Depth
	{
		get
		{
			int count = 1;
			if (ParentTabInstance != null)
				count += ParentTabInstance.Depth;
			return count;
		}
	}

	public TabInstance? ParentTabInstance { get; set; }
	public Dictionary<object, TabInstance> ChildTabInstances { get; set; } = new();

	public SynchronizationContext UiContext;
	public TabBookmark? FilterBookmarkNode;

	public class EventSelectItem : EventArgs
	{
		public readonly object Object;

		public EventSelectItem(object obj)
		{
			Object = obj;
		}
	}

	public class EventSelectItems : EventArgs
	{
		public readonly IList List;

		public EventSelectItems(IList list)
		{
			List = list;
		}
	}

	public event EventHandler<EventArgs>? OnRefresh;
	public event EventHandler<EventArgs>? OnReload;
	public event EventHandler<EventArgs>? OnModelChanged;
	public event EventHandler<EventArgs>? OnLoadBookmark;
	public event EventHandler<EventArgs>? OnClearSelection;
	public event EventHandler<EventSelectItems>? OnSelectItems;
	public event EventHandler<EventSelectItem>? OnSelectionChanged;
	public event EventHandler<EventArgs>? OnModified;
	public event EventHandler<EventArgs>? OnResize;

	public Action? DefaultAction; // Default action when Enter pressed

	// Relative paths for where all the TabSettings get stored, primarily used for loading future defaults
	// paths get hashed later to avoid having to encode and super long names breaking path limits
	private string? CustomPath => (Model.CustomSettingsPath != null) ? "Custom/" + GetType().FullName + "/" + Model.CustomSettingsPath : null;
	private string TabPath => "Tab/" + GetType().FullName + "/" + Model.ObjectTypePath;
	//private string TabPath => "Tab/" + GetType().FullName + "/" + Model.ObjectTypePath + "/" + Label;
	// deprecate?
	private string TypeLabelPath => "TypePath/" + Model.ObjectTypePath + "/" + Label;
	private string TypePath => "Type/" + Model.ObjectTypePath;

	private string LoadedPath => "Loaded/" + Model.ObjectTypePath;

	// Reload to initial state
	public bool IsLoaded { get; set; }
	public bool LoadCalled { get; set; } // Used by the view
	public bool StaticModel { get; set; }
	public bool ShowTasks { get; set; }
	public bool IsRoot { get; set; }

	public IList? SelectedItems { get; set; }

	protected IDataRepoInstance? DataRepoInstance { get; set; } // Bookmarks use this for saving/loading DataRepo values

	public TabInstance RootInstance => ParentTabInstance?.RootInstance ?? this;

	private bool _settingLoaded = false;

	public override string ToString() => Label;

	public TabInstance()
	{
		Project = new();

		InitializeContext();
	}

	public TabInstance(Project project, TabModel model)
	{
		Project = project;
		Model = model;
		StaticModel = true;

		InitializeContext();
		SetStartLoad();
	}

	public TabInstance? CreateChildTab(ITab iTab)
	{
		TabInstance tabInstance = iTab.Create();

		if (tabInstance.Project.LinkType == null)
			tabInstance.Project = Project;
		tabInstance.iTab = iTab;
		tabInstance.ParentTabInstance = this;
		//tabInstance.taskInstance = taskInstance.AddSubTask(taskInstance.call); // too slow?
		return tabInstance;
	}

	public virtual void Dispose()
	{
		ChildTabInstances.Clear();

		if (!StaticModel)
			Model.Clear();

		foreach (TaskInstance taskInstance in Model.Tasks)
		{
			taskInstance.Cancel();
		}
		TaskInstance.Cancel();
	}

	[MemberNotNull(nameof(UiContext))]
	private void InitializeContext()
	{
		UiContext ??= SynchronizationContext.Current ?? new SynchronizationContext();
	}

	private void ActionCallback(object? state)
	{
		Action action = (Action)state!;
		action.Invoke();
	}

	private void ActionParamsCallback(object? state)
	{
		TaskDelegateParams taskDelegate = (TaskDelegateParams)state!;
		StartTask(taskDelegate, false);
	}

	public void Invoke(Action action)
	{
		UiContext.Post(ActionCallback, action);
	}

	public void Invoke(SendOrPostCallback callback, object? param = null)
	{
		UiContext.Post(callback, param);
	}

	public void Invoke(Call call, Action action)
	{
		UiContext.Post(ActionCallback, action);
	}

	// switch to SendOrPostCallback?
	public void Invoke(CallActionParams callAction, params object[] objects)
	{
		var taskDelegate = new TaskDelegateParams(null, callAction.Method.Name, callAction, false, null, objects);
		UiContext.Post(ActionParamsCallback, taskDelegate);
	}

	// switch to SendOrPostCallback?
	public void Invoke(Call call, CallActionParams callAction, params object[] objects)
	{
		var taskDelegate = new TaskDelegateParams(call, callAction.Method.Name, callAction, false, null, objects);
		UiContext.Post(CallActionParamsCallback, taskDelegate);
	}

	private void CallActionParamsCallback(object? state)
	{
		var taskDelegate = (TaskDelegateParams)state!;
		StartTask(taskDelegate, false);
	}

	public TaskInstance StartTask(TaskCreator taskCreator, bool showTask, Call? call = null)
	{
		call ??= new Call(taskCreator.Label);
		TaskInstance taskInstance = taskCreator.Start(call);
		taskInstance.ShowTask = showTask || ShowTasks;
		if (taskInstance.ShowTask)
			Model.Tasks.Add(taskInstance);
		return taskInstance;
	}

	public TaskInstance StartTask(CallAction callAction, bool useTask, bool showTask)
	{
		var taskDelegate = new TaskDelegate(callAction, useTask);
		return StartTask(taskDelegate, showTask);
	}

	public TaskInstance StartAsync(CallActionAsync callAction, Call? call = null, bool showTask = false)
	{
		var taskDelegate = new TaskDelegateAsync(callAction, true);
		return StartTask(taskDelegate, showTask, call);
	}

	public void StartTask(CallActionParams callAction, bool useTask, bool showTask, params object[] objects)
	{
		var taskDelegate = new TaskDelegateParams(null, callAction.Method.Name.TrimEnd("Async").WordSpaced(), callAction, useTask, null, objects);
		StartTask(taskDelegate, showTask);
	}

	protected ItemCollection<ListProperty> GetListProperties()
	{
		return ListProperty.Create(this, false);
	}

	protected ItemCollection<ListMember> GetListMembers()
	{
		return ListMember.Create(this, false);
	}

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

	public bool HasLoadMethod => GetDerivedLoadMethod(nameof(Load), 2) != null;
	public bool HasLoadUIMethod => GetDerivedLoadMethod(nameof(LoadUI), 2) != null;

	public virtual void Load(Call call, TabModel model)
	{
	}

	public virtual void LoadUI(Call call, TabModel model)
	{
	}

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
			model = await LoadModelAsync(call);

		try
		{
			Preload(model);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}

		var subTask = call.AddSubTask("Loading");
		Invoke(() => LoadModelUI(subTask.Call, model)); // Some controls need to be created on the UI context
	}

	private async Task<TabModel> LoadModelAsync(Call call)
	{
		var model = new TabModel(Model.Name)
		{
			Tasks = Model.Tasks,
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
				//tabModel.Tasks.Add(call.taskInstance);
			}
			//StartAsync(ReinitializeAsync);
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
				&& !Model.Tasks.Contains(call.TaskInstance!))
				Model.Tasks.Add(call.TaskInstance!);
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
			Type elementType = listType.GetElementTypeForAll()!;

			var tabDataSettings = TabViewSettings.GetData(i);
			List<TabDataSettings.PropertyColumn> propertyColumns = tabDataSettings.GetPropertiesAsColumns(elementType);
			int itemCount = 0;
			foreach (object obj in iList)
			{
				foreach (var propertyColumn in propertyColumns)
				{
					propertyColumn.PropertyInfo.GetValue(obj);
				}
				itemCount++;
				if (itemCount > MaxPreloadItems)
					break;
			}

			if (iList is ItemCollection<ListProperty> propertyList)
				model.ItemList[i] = ListProperty.Sort(propertyList);

			if (iList is ItemCollection<ListMember> memberList)
				model.ItemList[i] = ListMember.Sort(memberList);
		}
	}

	public void LoadModelUI(Call call, TabModel model)
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
				context.InitializeContext(true);
		}

		LoadSettings(false);
		OnModelChanged?.Invoke(this, EventArgs.Empty);

		IsLoaded = true;
	}

	// calls Load and then Refresh
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
				OnReload.Invoke(this, EventArgs.Empty);
			else
				UiContext.Send(_ => OnReload(this, EventArgs.Empty), null);
		}
		// todo: this needs to actually wait for reload
	}

	// Reloads Controls & Settings
	public void Refresh()
	{
		IsLoaded = false;

		if (OnRefresh != null)
		{
			var onRefresh = OnRefresh; // create temporary copy since this gets delayed
			UiContext.Send(_ => onRefresh(this, EventArgs.Empty), null);
			// todo: this needs to actually wait for refresh?
		}
	}

	public void Resize()
	{
		if (OnResize != null)
		{
			var onResize = OnResize; // create temporary copy since this gets delayed
			UiContext.Send(_ => onResize(this, EventArgs.Empty), null);
		}
	}

	public bool Skippable
	{
		get
		{
			if (TabViewSettings == null)
				return false;

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

	public void SelectItem(object? obj)
	{
		SelectItems(new List<object?> { obj });
	}

	public void SelectItems(IList items)
	{
		if (OnSelectItems != null)
		{
			UiContext.Send(_ => OnSelectItems(this, new EventSelectItems(items)), null);
		}
		else
		{
			TabBookmark = TabBookmark.CreateList(items);
		}
	}

	public void ClearSelection()
	{
		if (OnClearSelection != null)
			UiContext.Send(_ => OnClearSelection(this, EventArgs.Empty), null);
	}

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

	public virtual Bookmark CreateBookmark()
	{
		Bookmark bookmark = new()
		{
			Name = Label,
			Type = iTab?.GetType(),
			TabBookmark = {IsRoot = true}
		};
		GetBookmark(bookmark.TabBookmark);
		bookmark = bookmark.DeepClone(TaskInstance.Call)!; // Sanitize and test bookmark
		return bookmark;
	}

	public Bookmark CreateNavigatorBookmark()
	{
		Bookmark bookmark = RootInstance.CreateBookmark();
		Project.Navigator.Append(bookmark, true);
		return bookmark;
	}

	public virtual void GetBookmark(TabBookmark tabBookmark)
	{
		tabBookmark.Name = Label;
		tabBookmark.ViewSettings = TabViewSettings.DeepClone() ?? new TabViewSettings();
		tabBookmark.DataRepoGroupId = DataRepoInstance?.GroupId;
		tabBookmark.SelectedRow = SelectedRow;

		/*if (DataRepoInstance != null)
		{
			foreach (var item in tabViewSettings.SelectedRows)
			{
				var dataRepoItem = new DataRepoItem()
				{
					Directory = DataRepoInstance.Directory,
					Key = item.dataKey,
					Value = item.dataValue,
				};
				tabBookmark.DataRepoItems.Add(dataRepoItem);
			}
		}*/

		if (iTab is IInnerTab innerTab)
			iTab = innerTab.Tab;

		if (iTab != null)
		{
			Type type = iTab.GetType();
			if (type.GetCustomAttribute<TabRootAttribute>() != null)
			{
				tabBookmark.IsRoot = true;
				tabBookmark.Tab = iTab;
			}
			else if (type.GetCustomAttribute<PublicDataAttribute>() != null && 
				(tabBookmark.IsRoot || IsRoot))
			{
				tabBookmark.Tab = iTab;
				tabBookmark.Bookmark!.Name = iTab.ToString();
			}
		}

		foreach (TabInstance tabInstance in ChildTabInstances.Values)
		{
			// Change this to a Key
			if (tabBookmark.ChildBookmarks.ContainsKey(tabInstance.Label))
				continue;

			var childBookmark = tabBookmark.AddChild(tabInstance.Label);
			tabInstance.GetBookmark(childBookmark);
		}
	}

	public TabViewSettings LoadBookmark(Bookmark bookmark)
	{
		TabBookmark = null;
		if (bookmark != null)
			SelectBookmark(bookmark.TabBookmark);

		return TabViewSettings; // remove?
	}

	public virtual void SelectBookmark(TabBookmark tabBookmark, bool reload = false)
	{
		if (reload)
			ClearSelection();

		TabBookmark = tabBookmark;
		TabViewSettings = tabBookmark.ViewSettings;

		if (OnLoadBookmark != null)
			UiContext.Send(_ => OnLoadBookmark(this, EventArgs.Empty), null);

		SaveTabSettings();
	}

	private void SaveDefaultBookmark()
	{
		Bookmark bookmark = RootInstance.CreateBookmark(); // create from base Tab
		if (bookmark == null)
			return;

		bookmark.Name = CurrentBookmarkName;
		DataApp.Save(bookmark.Name, bookmark, TaskInstance.Call);
	}

	public void LoadDefaultBookmark()
	{
		if (Project.UserSettings.AutoLoad == false)
			return;

		Bookmark? bookmark = DataApp.Load<Bookmark>(CurrentBookmarkName, TaskInstance.Call);
		if (bookmark != null)
			TabBookmark = bookmark.TabBookmark;
	}

	public TabViewSettings? LoadSettings(bool reload)
	{
		if (_settingLoaded && !reload && TabViewSettings != null)
			return TabViewSettings;

		if (TabBookmark != null && TabBookmark.ViewSettings != null)
		{
			TabViewSettings = TabBookmark.ViewSettings;
		}
		else
		{
			LoadDefaultTabSettings();
		}
		_settingLoaded = true;
		return TabViewSettings;
	}

	protected SortedDictionary<string, T> GetBookmarkSelectedData<T>()
	{
		return TabBookmark?.GetSelectedData<T>() ?? new SortedDictionary<string, T>();
	}

	public T? GetBookmarkData<T>(string name = TabBookmark.DefaultDataName)
	{
		if (TabBookmark != null)
			return TabBookmark.GetData<T>(name);

		return default;
	}

	// replace with DataShared? Split call up?
	public void SaveData(string name, object obj)
	{
		DataApp.Save(name, obj, TaskInstance.Call);
	}

	public void SaveData(string directory, string name, object obj)
	{
		DataApp.Save(directory, name, obj, TaskInstance.Call);
	}

	// todo: Should createIfNeeded = false?
	public T? LoadData<T>(string name, bool createIfNeeded = true)
	{
		T? data = DataApp.Load<T>(name, TaskInstance.Call, createIfNeeded);
		return data;
	}

	public T? LoadData<T>(string directory, string name, bool createIfNeeded = true)
	{
		T? data = DataApp.Load<T>(directory, name, TaskInstance.Call, createIfNeeded);
		return data;
	}

	public TabViewSettings LoadDefaultTabSettings()
	{
		TabViewSettings = GetTabSettings();
		return TabViewSettings;
	}

	public TabViewSettings GetTabSettings()
	{
		if (CustomPath != null)
		{
			TabViewSettings? tabViewSettings = DataTemp.Load<TabViewSettings>(CustomPath, TaskInstance.Call);
			if (tabViewSettings != null)
				return tabViewSettings;
		}

		Type type = GetType();
		if (type != typeof(TabInstance))
		{
			// Unique TabInstance
			TabViewSettings? tabViewSettings = DataTemp.Load<TabViewSettings>(TabPath, TaskInstance.Call);
			if (tabViewSettings != null)
				return tabViewSettings;
		}
		else
		{
			TabViewSettings? tabViewSettings = DataTemp.Load<TabViewSettings>(TypeLabelPath, TaskInstance.Call);
			if (tabViewSettings != null)
				return tabViewSettings;

			tabViewSettings = DataTemp.Load<TabViewSettings>(TypePath, TaskInstance.Call);
			if (tabViewSettings != null)
				return tabViewSettings;
		}

		return new TabViewSettings();
	}

	public void SaveTabSettings()
	{
		if (CustomPath != null)
		{
			DataTemp.Save(CustomPath, TabViewSettings, TaskInstance.Call);
		}

		Type type = GetType();
		if (type != typeof(TabInstance))
		{
			// Unique TabInstance
			DataTemp.Save(TabPath, TabViewSettings, TaskInstance.Call);
		}
		else
		{
			DataTemp.Save(TypeLabelPath, TabViewSettings, TaskInstance.Call);
			DataTemp.Save(TypePath, TabViewSettings, TaskInstance.Call);
		}
		SaveDefaultBookmark();
	}

	protected void SetStartLoad()
	{
		DataTemp.Save(LoadedPath, true, TaskInstance.Call);
	}

	public void SetEndLoad()
	{
		DataTemp.Delete(null, typeof(bool), LoadedPath);
	}

	// for detecting parent/child loops
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

	public void ItemModified()
	{
		OnModified?.Invoke(this, EventArgs.Empty);
	}

	public TabInstance CreateChild(TabModel model)
	{
		var childTabInstance = new TabInstance(Project, model)
		{
			ParentTabInstance = this,
		};

		if (TabBookmark != null)
		{
			if (TabBookmark.ChildBookmarks.TryGetValue(model.Name, out TabBookmark? tabChildBookmark))
			{
				childTabInstance.TabBookmark = tabChildBookmark;
			}
		}
		return childTabInstance;
	}

	private object? GetBookmarkObject(string name)
	{
		if (TabBookmark == null)
			return null;

		// FindMatches uses bookmarks
		if (TabBookmark.ChildBookmarks.TryGetValue(name, out TabBookmark? tabChildBookmark))
		{
			if (tabChildBookmark.TabModel != null)
				return tabChildBookmark.TabModel;
		}

		return tabChildBookmark;
	}

	public void UpdateNavigator()
	{
		Bookmark bookmark = RootInstance.CreateBookmark(); // create from root Tab
		Project.Navigator.Update(bookmark);
	}

	public void SelectionChanged(object? sender, EventArgs e)
	{
		if (SelectedItems?.Count > 0)
		{
			var eSelectItem = new EventSelectItem(SelectedItems[0]!);
			OnSelectionChanged?.Invoke(sender, eSelectItem);
		}
	}
}
