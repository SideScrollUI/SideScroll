﻿using Atlas.Core;
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

	public class TabInstanceLoadAsync : TabInstance, ITabAsync
	{
		private ILoadAsync loadAsync;

		public TabInstanceLoadAsync(ILoadAsync loadAsync)
		{
			this.loadAsync = loadAsync;
		}

		public async Task LoadAsync(Call call, TabModel model)
		{
			object result = await loadAsync.LoadAsync(call);
			model.AddData(result);
		}
	}

	public class TabCreatorAsync : TabInstance, ITabAsync
	{
		private ITabCreatorAsync creatorAsync;
		private TabInstance innerChildInstance;

		public TabCreatorAsync(ITabCreatorAsync creatorAsync)
		{
			this.creatorAsync = creatorAsync;
		}

		public async Task LoadAsync(Call call, TabModel model)
		{
			ITab iTab = await creatorAsync.CreateAsync(call);
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

		//public delegate void MethodInvoker();

		public Project Project { get; set; }
		public ITab iTab;
		//public Log Log => TaskInstance.Log;
		public TaskInstance TaskInstance { get; set; } = new TaskInstance();
		public TabModel Model { get; set; } = new TabModel();
		public string Label { get { return Model.Name; } set { Model.Name = value; } }

		public DataRepo DataApp => Project.DataApp;

		public TabViewSettings tabViewSettings = new TabViewSettings();
		public TabBookmark tabBookmark;

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

		public SynchronizationContext uiContext;
		public TabBookmark filterBookmarkNode;

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
			public object obj;

			public EventSelectItem(object obj)
			{
				this.obj = obj;
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
		public bool isLoaded = false;
		public bool loadCalled = false; // Used by the view
		private bool staticModel = false;

		public TabInstance()
		{
			InitializeContext();
		}

		public TabInstance(Project project, TabModel model)
		{
			Project = project;
			Model = model;
			staticModel = true;
			InitializeContext();
			SetStartLoad();
		}

		public override string ToString() => Label;

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
			if (!staticModel)
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
			uiContext = uiContext ?? SynchronizationContext.Current ?? new SynchronizationContext();
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

		public void Invoke(Action action)
		{
			uiContext.Post(ActionCallback, action);
		}

		public void Invoke(SendOrPostCallback callback, object param = null)
		{
			uiContext.Post(callback, param);
		}

		public void Invoke(Call call, Action action)
		{
			uiContext.Post(ActionCallback, action);
		}

		// switch to SendOrPostCallback?
		public void Invoke(CallActionParams callAction, params object[] objects)
		{
			var taskDelegate = new TaskDelegateParams(null, callAction.Method.Name, callAction, false, null, objects);
			uiContext.Post(ActionParamsCallback, taskDelegate);
		}

		// switch to SendOrPostCallback?
		public void Invoke(Call call, CallActionParams callAction, params object[] objects)
		{
			var taskDelegate = new TaskDelegateParams(call, callAction.Method.Name, callAction, false, null, objects);
			uiContext.Post(CallActionParamsCallback, taskDelegate);
		}

		private void CallActionParamsCallback(object state)
		{
			var taskDelegate = (TaskDelegateParams)state;
			CallTask(taskDelegate, false);
		}

		// subtask?
		public void CallTask(TaskDelegateParams taskCreator, bool showTask)
		{
			taskCreator.Start(taskCreator.call);
		}

		public void StartTask(TaskCreator taskCreator, bool showTask, Call call = null)
		{
			call = call ?? new Call(taskCreator.Label);
			TaskInstance taskInstance = taskCreator.Start(call);
			taskInstance.ShowTask = showTask;
			Model.Tasks.Add(taskInstance);
		}

		public void StartTask(CallAction callAction, bool useTask, bool showTask)
		{
			var taskDelegate = new TaskDelegate(callAction.Method.Name, callAction, useTask);
			StartTask(taskDelegate, showTask);
		}

		public void StartAsync(CallActionAsync callAction, Call call = null, bool showTask = false)
		{
			var taskDelegate = new TaskDelegateAsync(callAction.Method.Name, callAction, true);
			StartTask(taskDelegate, showTask, call);
		}

		public void StartTask(CallActionParams callAction, bool useTask, bool showTask, params object[] objects)
		{
			var taskDelegate = new TaskDelegateParams(null, callAction.Method.Name, callAction, useTask, null, objects);
			StartTask(taskDelegate, showTask);
		}

		protected ItemCollection<ListProperty> GetListProperties()
		{
			var items = ListProperty.Create(this);
			var properties = new ItemCollection<ListProperty>();
			foreach (ListProperty listProperty in items)
			{
				if (listProperty.propertyInfo.DeclaringType == GetType())
					properties.Add(listProperty);
			}

			return properties;
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
			if (!force && isLoaded)
				return;

			isLoaded = true;
			loadCalled = false; // allow TabView to reload

			StartAsync(ReintializeAsync);
		}

		public async Task ReintializeAsync(Call call)
		{
			TabModel model = Model;
			if (!staticModel)
				model = await LoadModelAsync(call);

			try
			{
				Preload(model);
			}
			catch (Exception e)
			{
				call.Log.Add(e);
			}

			//Invoke(() => LoadUi(call, model));
			var subTask = call.AddSubTask("Loading");
			Invoke(() => LoadModelUI(subTask.Call, model)); // Some controls need to be created on the UI context
		}

		private async Task<TabModel> LoadModelAsync(Call call)
		{
			var model = new TabModel(Model.Name);
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

				var tabDataSettings = tabViewSettings.GetData(i);
				List<TabDataSettings.PropertyColumn> propertyColumns = tabDataSettings.GetPropertiesAsColumns(elementType);
				int itemCount = 0;
				foreach (object obj in iList)
				{
					foreach (var propertyColumn in propertyColumns)
					{
						propertyColumn.propertyInfo.GetValue(obj);
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
			// Set the context to the UI for items that support it
			foreach (IList iList in model.ItemList)
			{
				if (iList is IContext context)
					context.InitializeContext(true);
			}

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
			Model = model;
			LoadSettings(); // Load() initializes the tabModel.Object & CustomSettingsPath which gets used for the settings path
			OnModelChanged?.Invoke(this, new EventArgs());

			isLoaded = true;
		}

		// calls Load and then Refresh
		public void Reload()
		{
			isLoaded = false;
			loadCalled = false;
			if (OnReload != null)
			{
				if (this is ITabAsync tabAsync)
					OnReload.Invoke(this, new EventArgs());
				else
					uiContext.Send(_ => OnReload(this, new EventArgs()), null);
			}
			// todo: this needs to actually wait for reload
		}

		// Reloads Controls & Settings
		public void Refresh()
		{
			isLoaded = false;
			if (OnRefresh != null)
			{
				var onRefresh = OnRefresh; // create temporary copy since this gets delayed
				uiContext.Send(_ => onRefresh(this, new EventArgs()), null);
				// todo: this needs to actually wait for refresh?
			}
		}

		public void Resize()
		{
			if (OnResize != null)
			{
				var onResize = OnResize; // create temporary copy since this gets delayed
				uiContext.Send(_ => onResize(this, new EventArgs()), null);
			}
		}

		public IList SelectedItems { get; set; }
		public bool Skippable
		{
			get
			{
				if (tabViewSettings == null)
					return false;
				if (tabViewSettings.SelectionType == SelectionType.User && tabViewSettings.SelectedRows.Count == 0) // Need to split apart user selected rows?
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
				uiContext.Send(_ => OnSelectItem(this, new EventSelectItem(obj)), null);
			}
			else
			{
				tabBookmark = TabBookmark.Create(obj);
			}
		}

		public void ClearSelection()
		{
			if (OnClearSelection != null)
				uiContext.Send(_ => OnClearSelection(this, new EventArgs()), null);
		}

		protected IDataRepoInstance DataRepoInstance { get; set; }

		public virtual Bookmark CreateBookmark()
		{
			var bookmark = new Bookmark();
			bookmark.Type = iTab?.GetType();
			//bookmark.tabBookmark.Name = Label;
			GetBookmark(bookmark.TabBookmark);
			bookmark = bookmark.Clone<Bookmark>(TaskInstance.Call); // Sanitize and test bookmark
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
			tabBookmark.ViewSettings = tabViewSettings;
			tabBookmark.DataRepoDirectory = DataRepoInstance?.Directory;
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
			tabBookmark = null;
			if (bookmark != null)
				SelectBookmark(bookmark.TabBookmark);

			return tabViewSettings; // remove?
		}

		public virtual void SelectBookmark(TabBookmark tabBookmark)
		{
			this.tabViewSettings = tabBookmark.ViewSettings;
			this.tabBookmark = tabBookmark;
			if (OnLoadBookmark != null)
				uiContext.Send(_ => OnLoadBookmark(this, new EventArgs()), null);
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

			//bookmark.Name = Label;
			//project.navigator.Add(bookmark);

			/*Serializer serializer = new Serializer();
			Bookmark clonedBookmark = serializer.Clone<Bookmark>(log, bookmark);


			project.navigator.Add(clonedBookmark);*/
		}

		public void LoadDefaultBookmark()
		{
			if (Project.UserSettings.AutoLoad == false)
				return;

			Bookmark bookmark = Project.DataApp.Load<Bookmark>(CurrentBookmarkName, TaskInstance.Call);
			if (bookmark != null)
				tabBookmark = bookmark.TabBookmark;
		}

		public TabViewSettings LoadSettings()
		{
			if (tabBookmark != null && tabBookmark.ViewSettings != null)
			{
				tabViewSettings = tabBookmark.ViewSettings;
			}
			else
			{
				LoadDefaultTabSettings();
			}
			return tabViewSettings;
		}

		protected SortedDictionary<string, T> GetBookmarkSelectedData<T>()
		{
			return tabBookmark?.GetSelectedData<T>() ?? new SortedDictionary<string, T>();
		}

		protected T GetBookmarkData<T>(string name = "default")
		{
			if (tabBookmark != null)
				return tabBookmark.GetData<T>(name);
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

		/*public ItemCollection<T> LoadAllData<T>(string directory = null)
		{
			ItemCollection<T> datas = project.DataApp.LoadAll<T>(taskInstance.call, directory);
			return datas;
		}*/

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
			tabViewSettings = null;

			if (CustomPath != null)
			{
				tabViewSettings = Project.DataApp.Load<TabViewSettings>(CustomPath, TaskInstance.Call);
				if (tabViewSettings != null)
					return tabViewSettings;
			}

			Type type = GetType();
			if (type != typeof(TabInstance))
			{
				// Unique TabInstance
				tabViewSettings = Project.DataApp.Load<TabViewSettings>(TabPath, TaskInstance.Call);
				if (tabViewSettings != null)
					return tabViewSettings;
			}
			else
			{
				tabViewSettings = Project.DataApp.Load<TabViewSettings>(TypeLabelPath, TaskInstance.Call);
				if (tabViewSettings != null)
					return tabViewSettings;

				tabViewSettings = Project.DataApp.Load<TabViewSettings>(TypePath, TaskInstance.Call);
				if (tabViewSettings != null)
					return tabViewSettings;
			}

			tabViewSettings = new TabViewSettings();
			return tabViewSettings;
		}

		public void SaveTabSettings()
		{
			if (CustomPath != null)
				Project.DataApp.Save(CustomPath, tabViewSettings, TaskInstance.Call);

			Type type = GetType();
			if (type != typeof(TabInstance))
			{
				// Unique TabInstance
				Project.DataApp.Save(TabPath, tabViewSettings, TaskInstance.Call);
			}
			else
			{
				Project.DataApp.Save(TypeLabelPath, tabViewSettings, TaskInstance.Call);
				Project.DataApp.Save(TypePath, tabViewSettings, TaskInstance.Call);
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
			//childTabInstance.tabBookmark = tabBookmark;

			if (tabBookmark != null)
			{
				if (tabBookmark.ChildBookmarks.TryGetValue(model.Name, out TabBookmark tabChildBookmark))
				{
					childTabInstance.tabBookmark = tabChildBookmark;
				}
			}
			return childTabInstance;
		}

		private object GetBookmarkObject(string name)
		{
			// FindMatches uses bookmarks
			TabBookmark tabChildBookmark = null;
			if (tabBookmark != null)
			{
				if (tabBookmark.ChildBookmarks.TryGetValue(name, out tabChildBookmark))
				{
					if (tabChildBookmark.tabModel != null)
						return tabChildBookmark.tabModel;
				}
				/*foreach (Bookmark.Node node in tabInstance.tabBookmark.nodes)
				{
					tabChildBookmark = node;
					break;
				}*/
			}
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
