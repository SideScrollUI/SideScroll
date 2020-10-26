using Atlas.Core;
using Atlas.Extensions;
using Atlas.Serialize;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static Atlas.Core.TaskDelegate;
using static Atlas.Core.TaskDelegateAsync;
using static Atlas.Core.TaskDelegateParams;

namespace Atlas.Tabs
{
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
		ITab Tab { get; }
	}

	public class TabInstanceLoadAsync : TabInstance, ITabAsync
	{
		public ILoadAsync LoadMethod;

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
		public ITabCreatorAsync CreatorAsync;

		private TabInstance innerChildInstance;

		public TabCreatorAsync(ITabCreatorAsync creatorAsync)
		{
			CreatorAsync = creatorAsync;
		}

		public async Task LoadAsync(Call call, TabModel model)
		{
			ITab iTab = await CreatorAsync.CreateAsync(call);
			innerChildInstance = CreateChildTab(iTab);
			if (innerChildInstance is ITabAsync tabAsync)
				await tabAsync.LoadAsync(call, model);
		}

		public override void LoadUI(Call call, TabModel model)
		{
			innerChildInstance.LoadUI(call, model);
		}
	}

	public abstract class TabInstanceAsync : TabInstance, ITabAsync
	{
		public abstract Task LoadAsync(Call call, TabModel model);
	}

	//	An Instance of a TabModel, created by TabView
	public class TabInstance : IDisposable
	{
		public const string CurrentBookmarkName = "Current";

		public Project Project { get; set; }
		public ITab iTab; // Collision with derived Tab
		//public Log Log => TaskInstance.Log;
		public TaskInstance TaskInstance { get; set; } = new TaskInstance();
		public TabModel Model { get; set; } = new TabModel();
		public string Label { get { return Model.Name; } set { Model.Name = value; } }

		public DataRepo DataApp => Project.DataApp;

		public TabViewSettings TabViewSettings = new TabViewSettings();
		public TabBookmark TabBookmark { get; set; }
		public SelectedRow SelectedRow { get; set; } // The parent selection that points to this tab

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

		public TabInstance ParentTabInstance { get; set; }
		public Dictionary<object, TabInstance> ChildTabInstances { get; set; } = new Dictionary<object, TabInstance>();

		public SynchronizationContext UiContext;
		public TabBookmark FilterBookmarkNode;

		public event EventHandler<EventArgs> OnRefresh;
		public event EventHandler<EventArgs> OnReload;
		public event EventHandler<EventArgs> OnModelChanged;
		public event EventHandler<EventArgs> OnLoadBookmark;
		public event EventHandler<EventArgs> OnClearSelection;
		public event EventHandler<EventSelectItem> OnSelectItem;
		public event EventHandler<EventSelectItem> OnSelectionChanged;
		public event EventHandler<EventArgs> OnModified;
		public event EventHandler<EventArgs> OnResize;

		public class EventSelectItem : EventArgs
		{
			public object Object;

			public EventSelectItem(object obj)
			{
				Object = obj;
			}
		}

		// Relative paths for where all the TabSettings get stored, primarily used for loading future defaults
		// paths get hashed later to avoid having to encode and super long names breaking path limits
		private string CustomPath => (Model.CustomSettingsPath != null) ? "Custom/" + GetType().FullName + "/" + Model.CustomSettingsPath : null;
		private string TabPath => "Tab/" + GetType().FullName + "/" + Model.ObjectTypePath;
		//private string TabPath => "Tab/" + GetType().FullName + "/" + tabModel.ObjectTypePath + "/" + Label;
		// deprecate?
		private string TypeLabelPath => "TypePath/" + Model.ObjectTypePath + "/" + Label;
		private string TypePath => "Type/" + Model.ObjectTypePath;

		private string LoadedPath => "Loaded/" + Model.ObjectTypePath;

		// Reload to initial state
		public bool IsLoaded { get; set; }
		public bool LoadCalled { get; set; } // Used by the view
		public bool StaticModel { get; set; }
		public bool ShowTasks { get; set; }
		public IList SelectedItems { get; set; }

		protected IDataRepoInstance DataRepoInstance { get; set; } // Bookmarks use this for saving/loading DataRepo values

		public override string ToString() => Label;

		public TabInstance()
		{
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

		public TabInstance CreateChildTab(ITab iTab)
		{
			TabInstance tabInstance = iTab.Create();
			if (tabInstance == null)
				return null;

			tabInstance.Project = tabInstance.Project ?? Project;
			tabInstance.iTab = iTab;
			tabInstance.ParentTabInstance = this;
			//tabInstance.taskInstance.call.Log =
			//tabInstance.taskInstance = taskInstance.AddSubTask(taskInstance.call); // too slow?
			//tabInstance.tabBookmark = tabBookmark;
			FillInheritables(tabInstance);
			return tabInstance;
		}

		// Incomplete, we don't ever set the ObjectStore values, what do we set them to?
		private void FillInheritables(TabInstance tabInstance)
		{
			Type type = tabInstance.GetType();
			FieldInfo[] fieldInfos = type.GetFields().OrderBy(x => x.MetadataToken).ToArray();
			PropertyInfo[] propertyInfos = type.GetProperties().OrderBy(x => x.MetadataToken).ToArray();

			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				if (fieldInfo.GetCustomAttribute(typeof(InheritAttribute)) == null)
					continue;

				object obj = Project.TypeObjectStore.Get(fieldInfo.FieldType);
				fieldInfo.SetValue(tabInstance, obj);
			}

			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				//if (!propertyInfo.DeclaringType.IsNotPublic)
				{
					if (propertyInfo.GetCustomAttribute(typeof(InheritAttribute)) == null)
						continue;

					object obj = Project.TypeObjectStore.Get(propertyInfo.PropertyType);
					propertyInfo.SetValue(tabInstance, obj);
				}
			}
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

		private void InitializeContext()
		{
			//Debug.Assert(context == null || SynchronizationContext.Current == context);
			UiContext = UiContext ?? SynchronizationContext.Current ?? new SynchronizationContext();
		}

		public TabInstance RootInstance
		{
			get
			{
				if (ParentTabInstance == null)
					return this;
				return ParentTabInstance.RootInstance;
			}
		}

		private void ActionCallback(object state)
		{
			Action action = (Action)state;
			action.Invoke();
		}

		private void ActionParamsCallback(object state)
		{
			TaskDelegateParams taskDelegate = (TaskDelegateParams)state;
			StartTask(taskDelegate, false);
		}

		// make generic? not useful yet, causes flickering
		public void ScheduleTask(int milliSeconds, Action action)
		{
			Task.Delay(milliSeconds).ContinueWith(t => action());
		}

		public void ScheduleTask(TimeSpan timeSpan, Action action)
		{
			Task.Delay(timeSpan).ContinueWith(t => action());
		}

		public void Invoke(Action action)
		{
			UiContext.Post(ActionCallback, action);
		}

		public void Invoke(SendOrPostCallback callback, object param = null)
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

		private void CallActionParamsCallback(object state)
		{
			var taskDelegate = (TaskDelegateParams)state;
			CallTask(taskDelegate, false);
		}

		// subtask?
		public void CallTask(TaskDelegateParams taskCreator, bool showTask)
		{
			taskCreator.Start(taskCreator.Call);
		}

		public void StartTask(TaskCreator taskCreator, bool showTask, Call call = null)
		{
			call = call ?? new Call(taskCreator.Label);
			TaskInstance taskInstance = taskCreator.Start(call);
			taskInstance.ShowTask = showTask || ShowTasks;
			if (taskInstance.ShowTask)
				Model.Tasks.Add(taskInstance);
		}

		public void StartTask(CallAction callAction, bool useTask, bool showTask)
		{
			var taskDelegate = new TaskDelegate(callAction.Method.Name.TrimEnd("Async"), callAction, useTask);
			StartTask(taskDelegate, showTask);
		}

		public void StartAsync(CallActionAsync callAction, Call call = null, bool showTask = false)
		{
			var taskDelegate = new TaskDelegateAsync(callAction.Method.Name.TrimEnd("Async"), callAction, true);
			StartTask(taskDelegate, showTask, call);
		}

		public void StartTask(CallActionParams callAction, bool useTask, bool showTask, params object[] objects)
		{
			var taskDelegate = new TaskDelegateParams(null, callAction.Method.Name.TrimEnd("Async"), callAction, useTask, null, objects);
			StartTask(taskDelegate, showTask);
		}

		protected ItemCollection<ListProperty> GetListProperties()
		{
			var items = ListProperty.Create(this);
			var properties = new ItemCollection<ListProperty>();
			foreach (ListProperty listProperty in items)
			{
				if (listProperty.PropertyInfo.DeclaringType == GetType())
					properties.Add(listProperty);
			}

			return properties;
		}

		protected List<IListItem> GetListItems()
		{
			return ListItem.Create(this, false);
		}

		private MethodInfo GetDerivedLoadMethod(string name, int paramCount)
		{
			try
			{
				Type type = GetType(); // gets derived type
				var methods = type.GetMethods().Where(m => m.Name == name).ToList();
				foreach (var method in methods)
				{
					var parameters = method.GetParameters();
					if (paramCount == parameters.Length)
						return method;
				}
			}
			catch (Exception)
			{
			}
			return null;
		}

		// Check if derived type implements Load()
		public bool CanLoad
		{
			get
			{
				MethodInfo methodInfo = GetDerivedLoadMethod(nameof(Load), 2);
				return (methodInfo?.DeclaringType != typeof(TabInstance));
			}
		}

		public bool CanLoadUI
		{
			get
			{
				MethodInfo methodInfo = GetDerivedLoadMethod(nameof(LoadUI), 2);
				return (methodInfo?.DeclaringType != typeof(TabInstance));
			}
		}

		public virtual void Load(Call call, TabModel model)
		{
		}

		public virtual void LoadUI(Call call, TabModel model)
		{
		}

		public void Reintialize(bool force)
		{
			if (!force && IsLoaded)
				return;

			IsLoaded = true;
			LoadCalled = false; // allow TabView to reload

			StartAsync(ReintializeAsync, TaskInstance.Call);
		}

		public async Task ReintializeAsync(Call call)
		{
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
			var model = new TabModel(Model.Name);
			model.Tasks = Model.Tasks;
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
				//StartAsync(ReintializeAsync);
			}
			if (CanLoad)
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
				Type elementType = listType.GetElementTypeForAll();

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
					if (itemCount > 50)
						break;
				}

				if (iList is ItemCollection<ListProperty> propertyList)
					model.ItemList[i] = ListProperty.Sort(propertyList);

				if (iList is ItemCollection<ListMember> memberList)
					model.ItemList[i] = ListMember.Sort(memberList);
				i++;
			}
		}

		public void LoadModelUI(Call call, TabModel model)
		{
			if (CanLoadUI)
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

			Model = model;
			LoadSettings(); // Load() initializes the tabModel.Object & CustomSettingsPath which gets used for the settings path
			OnModelChanged?.Invoke(this, new EventArgs());

			IsLoaded = true;
		}

		// calls Load and then Refresh
		public void Reload()
		{
			IsLoaded = false;
			LoadCalled = false;
			if (OnReload != null)
			{
				if (this is ITabAsync tabAsync)
					OnReload.Invoke(this, new EventArgs());
				else
					UiContext.Send(_ => OnReload(this, new EventArgs()), null);
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
				UiContext.Send(_ => onRefresh(this, new EventArgs()), null);
				// todo: this needs to actually wait for refresh?
			}
		}

		public void Resize()
		{
			if (OnResize != null)
			{
				var onResize = OnResize; // create temporary copy since this gets delayed
				UiContext.Send(_ => onResize(this, new EventArgs()), null);
			}
		}

		public bool Skippable
		{
			get
			{
				if (TabViewSettings == null)
					return false;

				//if (TabViewSettings.SelectionType == SelectionType.User && TabViewSettings.SelectedRows.Count == 0) // Need to split apart user selected rows?
				if (TabViewSettings.SelectedRows.Count == 0)
					return false;

				// Only data is skippable?
				if (Model.Objects.Count > 0 || Model.ItemList.Count == 0 || Model.ItemList[0].Count != 1)
					return false;

				var skippableAttribute = Model.ItemList[0][0].GetType().GetCustomAttribute<SkippableAttribute>();
				if (skippableAttribute == null && Model.Actions != null && Model.Actions.Count > 0)
					return false; 

				return Model.Skippable;
			}
		}

		public void SelectItem(object obj)
		{
			if (OnSelectItem != null)
			{
				UiContext.Send(_ => OnSelectItem(this, new EventSelectItem(obj)), null);
			}
			else
			{
				TabBookmark = TabBookmark.Create(obj);
			}
		}

		public void ClearSelection()
		{
			if (OnClearSelection != null)
				UiContext.Send(_ => OnClearSelection(this, new EventArgs()), null);
		}

		public bool IsLinkable
		{
			get
			{
				Type type = iTab?.GetType();
				if (type == null)
					return false;
				return TypeSchema.HasEmptyConstructor(type);
			}
		}

		public virtual Bookmark CreateBookmark()
		{
			var bookmark = new Bookmark
			{
				Type = iTab?.GetType(),
			};
			GetBookmark(bookmark.TabBookmark);
			bookmark = bookmark.DeepClone(TaskInstance.Call); // Sanitize and test bookmark
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
			tabBookmark.ViewSettings = TabViewSettings.DeepClone();
			tabBookmark.DataRepoDirectory = DataRepoInstance?.Directory;
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
			if (iTab?.GetType().GetCustomAttribute<TabRootAttribute>() != null)
			{
				tabBookmark.IsRoot = true;
				tabBookmark.Tab = iTab;
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

		public virtual void SelectBookmark(TabBookmark tabBookmark)
		{
			TabViewSettings = tabBookmark.ViewSettings;
			TabBookmark = tabBookmark;
			if (OnLoadBookmark != null)
				UiContext.Send(_ => OnLoadBookmark(this, new EventArgs()), null);
			//this.bookmarkNode = null; // have to wait until TabData's Load, which might be after this
			/*foreach (TabInstance tabInstance in children)
			{
			}*/
			SaveTabSettings();
		}

		private void SaveDefaultBookmark()
		{
			Bookmark bookmark = RootInstance.CreateBookmark(); // create from base Tab
			if (bookmark == null)
				return;

			bookmark.Name = CurrentBookmarkName;
			Project.DataApp.Save(bookmark.Name, bookmark, TaskInstance.Call);

			//project.navigator.Add(bookmark);

			/*
			Bookmark clonedBookmark = bookmark.DeepClone();
			project.navigator.Add(clonedBookmark);*/
		}

		public void LoadDefaultBookmark()
		{
			if (Project.UserSettings.AutoLoad == false)
				return;

			Bookmark bookmark = Project.DataApp.Load<Bookmark>(CurrentBookmarkName, TaskInstance.Call);
			if (bookmark != null)
				TabBookmark = bookmark.TabBookmark;
		}

		public TabViewSettings LoadSettings()
		{
			if (TabBookmark != null && TabBookmark.ViewSettings != null)
			{
				TabViewSettings = TabBookmark.ViewSettings;
			}
			else
			{
				LoadDefaultTabSettings();
			}
			return TabViewSettings;
		}

		protected SortedDictionary<string, T> GetBookmarkSelectedData<T>()
		{
			return TabBookmark?.GetSelectedData<T>() ?? new SortedDictionary<string, T>();
		}

		protected T GetBookmarkData<T>(string name = TabBookmark.DefaultDataName)
		{
			if (TabBookmark != null)
				return TabBookmark.GetData<T>(name);

			return default;
		}

		// replace with DataShared? Split call up?
		public void SaveData(string name, object obj)
		{
			Project.DataApp.Save(name, obj, TaskInstance.Call);
		}

		public void SaveData(string directory, string name, object obj)
		{
			Project.DataApp.Save(directory, name, obj, TaskInstance.Call);
		}

		public T LoadData<T>(string name, bool createIfNeeded = true)
		{
			T data = Project.DataApp.Load<T>(name, TaskInstance.Call, createIfNeeded);
			return data;
		}

		public T LoadData<T>(string directory, string name, bool createIfNeeded = true)
		{
			T data = Project.DataApp.Load<T>(directory, name, TaskInstance.Call, createIfNeeded);
			return data;
		}

		/*private void LoadBookmark2()
		{
			LoadTabSettings();
			int index = 0;
			foreach (TabData tabData in tabDatas)
			{
				tabData.tabDataSettings = tabSettings.GetData(index++);
				tabData.LoadSavedSettings();

				//if (tabInstance.bookmarkNode != null)
				foreach (TabInstance childTabInstance in children)
				{
					BookmarkNode childBookmarkNode = null;
					if (bookmarkNode.Nodes.TryGetValue(childTabInstance.Label, out childBookmarkNode))
					{
						//child.bookmarkNode = bookmarkNode;
						childTabInstance.SelectBookmark(childBookmarkNode);
					}
				}
			}
		}*/

		public TabViewSettings LoadDefaultTabSettings()
		{
			TabViewSettings = null;

			if (CustomPath != null)
			{
				TabViewSettings = Project.DataApp.Load<TabViewSettings>(CustomPath, TaskInstance.Call);
				if (TabViewSettings != null)
					return TabViewSettings;
			}

			Type type = GetType();
			if (type != typeof(TabInstance))
			{
				// Unique TabInstance
				TabViewSettings = Project.DataApp.Load<TabViewSettings>(TabPath, TaskInstance.Call);
				if (TabViewSettings != null)
					return TabViewSettings;
			}
			else
			{
				TabViewSettings = Project.DataApp.Load<TabViewSettings>(TypeLabelPath, TaskInstance.Call);
				if (TabViewSettings != null)
					return TabViewSettings;

				TabViewSettings = Project.DataApp.Load<TabViewSettings>(TypePath, TaskInstance.Call);
				if (TabViewSettings != null)
					return TabViewSettings;
			}

			TabViewSettings = new TabViewSettings();
			return TabViewSettings;
		}

		public void SaveTabSettings()
		{
			if (CustomPath != null)
				Project.DataApp.Save(CustomPath, TabViewSettings, TaskInstance.Call);

			Type type = GetType();
			if (type != typeof(TabInstance))
			{
				// Unique TabInstance
				Project.DataApp.Save(TabPath, TabViewSettings, TaskInstance.Call);
			}
			else
			{
				Project.DataApp.Save(TypeLabelPath, TabViewSettings, TaskInstance.Call);
				Project.DataApp.Save(TypePath, TabViewSettings, TaskInstance.Call);
			}
			SaveDefaultBookmark();
		}

		protected void SetStartLoad()
		{
			Project.DataApp.Save(LoadedPath, true, TaskInstance.Call);
		}

		public void SetEndLoad()
		{
			Project.DataApp.Delete(typeof(bool), LoadedPath);
		}

		// for detecting parent/child loops
		public bool IsOwnerObject(object obj)
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
						var tabKey = propertyInfo.GetValue(iTab);
						// todo: support multiple [Key]s?
						if (objKey == tabKey)
							return true;

						if (objKey.GetType() == typeof(string) && Equals(objKey, tabKey))
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
			OnModified?.Invoke(this, null);
		}

		public TabInstance CreateChild(TabModel model)
		{
			var childTabInstance = new TabInstance(Project, model)
			{
				ParentTabInstance = this,
			};

			if (TabBookmark != null)
			{
				if (TabBookmark.ChildBookmarks.TryGetValue(model.Name, out TabBookmark tabChildBookmark))
				{
					childTabInstance.TabBookmark = tabChildBookmark;
				}
			}
			return childTabInstance;
		}

		private object GetBookmarkObject(string name)
		{
			if (TabBookmark == null)
				return null;
			
			// FindMatches uses bookmarks
			if (TabBookmark.ChildBookmarks.TryGetValue(name, out TabBookmark tabChildBookmark))
			{
				if (tabChildBookmark.TabModel != null)
					return tabChildBookmark.TabModel;
			}
			/*foreach (Bookmark.Node node in tabInstance.tabBookmark.nodes)
			{
				tabChildBookmark = node;
				break;
			}*/
			return tabChildBookmark;
		}

		public void UpdateNavigator()
		{
			Bookmark bookmark = RootInstance.CreateBookmark(); // create from root Tab
			Project.Navigator.Update(bookmark);
		}

		public void SelectionChanged(object sender, EventArgs e)
		{
			if (SelectedItems?.Count > 0)
			{
				var eSelectItem = new EventSelectItem(SelectedItems[0]);
				OnSelectionChanged?.Invoke(sender, eSelectItem);
			}
		}
	}
}
