using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
		Task LoadAsync(Call call);
	}

	//	An Instance of a TabModel, created by TabView
	public class TabInstance : IDisposable
	{
		public const string CurrentBookmarkName = "Current";

		public delegate void MethodInvoker();

		public Project project;
		public ITab iTab;
		//public Log Log { get {taskInstance.log; } }
		public TaskInstance taskInstance = new TaskInstance();
		public TabModel tabModel = new TabModel();
		public string Label { get { return tabModel.Name; } set { tabModel.Name = value; } }

		public DataRepo DataApp => project.DataApp;

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
		public Dictionary<object, TabInstance> childTabInstances = new Dictionary<object, TabInstance>();

		private SynchronizationContext guiContext; // inherited from creator (which can be a Parent Log)
		public TabBookmark filterBookmarkNode;

		public event EventHandler<EventArgs> OnRefresh;
		public event EventHandler<EventArgs> OnReload;
		public event EventHandler<EventArgs> OnLoadBookmark;
		public event EventHandler<EventArgs> OnClearSelection;
		public event EventHandler<EventSelectItem> OnSelectItem;
		public event EventHandler<EventArgs> OnModified;

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
		private string CustomPath => (tabModel.CustomSettingsPath != null) ? "Custom/" + GetType().FullName + "/" + tabModel.CustomSettingsPath : null;
		private string TabPath => "Tab/" + GetType().FullName + "/" + tabModel.ObjectTypePath;
		//private string TabPath => "Tab/" + GetType().FullName + "/" + tabModel.ObjectTypePath + "/" + Label;
		// deprecate?
		private string TypeLabelPath => "TypePath/" + tabModel.ObjectTypePath + "/" + Label;
		private string TypePath => "Type/" + tabModel.ObjectTypePath;

		private string LoadedPath => "Loaded/" + tabModel.ObjectTypePath;

		// Reload to initial state
		public bool isLoaded = false;
		public bool loadCalled = false; // Used by the view

		public TabInstance()
		{
			InitializeContext();
		}

		public TabInstance(Project project, TabModel tabModel)
		{
			this.project = project;
			this.tabModel = tabModel;
			InitializeContext();
			SetStartLoad();
		}

		public TabInstance CreateChildTab(ITab iTab)
		{
			TabInstance tabInstance = iTab.Create();
			tabInstance.project = project;
			tabInstance.iTab = iTab;
			tabInstance.ParentTabInstance = this;
			//tabInstance.taskInstance.call.log =
			//tabInstance.taskInstance = taskInstance.AddSubTask(taskInstance.call); // too slow?
			//tabInstance.tabBookmark = tabBookmark;
			FillInheritables(tabInstance);
			return tabInstance;
		}

		public TabInstance CreateChildTab(TabModel tabModel)
		{
			TabInstance tabInstance = new TabInstance(project, tabModel);
			//tabInstance.project = project;
			//tabInstance.iTab = iTab;
			tabInstance.ParentTabInstance = this;
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

				object obj = project.TypeObjectStore.Get(fieldInfo.FieldType);
				fieldInfo.SetValue(tabInstance, obj);
			}

			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				//if (!propertyInfo.DeclaringType.IsNotPublic)
				{
					if (propertyInfo.GetCustomAttribute(typeof(InheritAttribute)) == null)
						continue;

					object obj = project.TypeObjectStore.Get(propertyInfo.PropertyType);
					propertyInfo.SetValue(tabInstance, obj);
				}
			}
		}

		public override string ToString()
		{
			return Label;
		}

		public virtual void Dispose()
		{
			childTabInstances.Clear();
			if (CanLoad)
			{
				tabModel.Clear();
			}
			foreach (TaskInstance taskInstance in tabModel.Tasks)
			{
				taskInstance.Cancel();
			}
			taskInstance.Cancel();
		}

		private void InitializeContext()
		{
			//Debug.Assert(context == null || SynchronizationContext.Current == context);
			if (this.guiContext == null)
			{
				this.guiContext = SynchronizationContext.Current;
				if (this.guiContext == null)
					this.guiContext = new SynchronizationContext();
			}
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
			guiContext.Send(ActionCallback, action);
		}

		public void Invoke(SendOrPostCallback callback, object param = null)
		{
			guiContext.Send(callback, param);
		}

		public void Invoke(Call call, Action action)
		{
			guiContext.Send(ActionCallback, action);
		}

		// switch to SendOrPostCallback?
		public void Invoke(CallActionParams callAction, params object[] objects)
		{
			TaskDelegateParams taskDelegate = new TaskDelegateParams(null, callAction.Method.Name, callAction, false, null, objects);
			guiContext.Send(ActionParamsCallback, taskDelegate);
		}

		// switch to SendOrPostCallback?
		public void Invoke(Call call, CallActionParams callAction, params object[] objects)
		{
			TaskDelegateParams taskDelegate = new TaskDelegateParams(call, callAction.Method.Name, callAction, false, null, objects);
			guiContext.Send(CallActionParamsCallback, taskDelegate);
		}

		private void CallActionParamsCallback(object state)
		{
			TaskDelegateParams taskDelegate = (TaskDelegateParams)state;
			CallTask(taskDelegate, false);
		}

		// subtask?
		public void CallTask(TaskDelegateParams taskCreator, bool showTask)
		{
			TaskInstance taskInstance = taskCreator.Start(taskCreator.call);
		}

		public void StartTask(TaskCreator taskCreator, bool showTask)
		{
			Call call = new Call(taskCreator.Label);
			TaskInstance taskInstance = taskCreator.Start(call);
			taskInstance.ShowTask = showTask;
			tabModel.Tasks.Add(taskInstance);
		}

		public void StartTask(CallAction callAction, bool useTask, bool showTask)
		{
			TaskDelegate taskDelegate = new TaskDelegate(callAction.Method.Name, callAction, useTask);
			StartTask(taskDelegate, showTask);
		}

		public void StartAsync(CallActionAsync callAction, bool showTask = false)
		{
			var taskDelegate = new TaskDelegateAsync(callAction.Method.Name, callAction, true);
			StartTask(taskDelegate, showTask);
		}

		public void StartTask(CallActionParams callAction, bool useTask, bool showTask, params object[] objects)
		{
			TaskDelegateParams taskDelegate = new TaskDelegateParams(null, callAction.Method.Name, callAction, useTask, null, objects);
			StartTask(taskDelegate, showTask);
		}

		private MethodInfo GetDerivedLoadMethod()
		{
			Type type = GetType(); // gets derived type
			if (type.GetMethods().Where(m => m.Name == nameof(Load)).ToList().Count > 1)
				return null;
			MethodInfo methodInfo = type.GetMethod(nameof(Load));//, new Type[] { typeof(Call) });
			return methodInfo;
		}

		// Check if derived type implements Load()
		public bool CanLoad
		{
			get
			{
				MethodInfo methodInfo = GetDerivedLoadMethod();
				return (methodInfo.DeclaringType != typeof(TabInstance));
			}
		}

		public virtual void Load(Call call)
		{
		}

		public void Reintialize(bool force)
		{
			if (!force && isLoaded)
				return;

			loadCalled = false; // allow TabView to reload
			
			if (this is ITabAsync || CanLoad)
				tabModel.Clear(); // don't clear for Tab Instances, only auto generated

			try
			{
				//MethodInfo methodInfo = GetDerivedLoadMethod();
				if (this is ITabAsync tabAsync)
				{
					Task.Run(() => tabAsync.LoadAsync(taskInstance.call)).GetAwaiter().GetResult();
				}
				if (CanLoad)
				{
					var subTask = taskInstance.call.AddSubTask("Loading");
					//using (CallTimer loadCall = )
					{
						if (this is ITabAsync)
							Invoke(() => Load(subTask.call));
						else
							Load(subTask.call); // Creates a tabModel if none exists and adds other Controls
												//if (subTask.TaskStatus ==TaskStatus.
					}
					isLoaded = true;
				}
			}
			catch (Exception e)
			{
				tabModel.AddData(e);
			}
			LoadSettings(); // Load() initializes the tabModel.Object which gets used for the settings path

			// Have return tabModel?
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
					guiContext.Send(_ => OnReload(this, new EventArgs()), null);
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
				guiContext.Send(_ => onRefresh(this, new EventArgs()), null);
				// todo: this needs to actually wait for refresh?
			}
		}

		public IList SelectedItems { get; set; }
		public bool Skippable
		{
			get
			{
				if (tabViewSettings.SelectionType == SelectionType.User && tabViewSettings.SelectedRows.Count == 0) // Need to split apart user selected rows?
					return false;
				// Only data is skippable?
				if (tabModel.Objects.Count > 0 || tabModel.ItemList.Count == 0 || tabModel.ItemList[0].Count == 0)
					return false;
				var skippableAttribute = tabModel.ItemList[0][0].GetType().GetCustomAttribute<SkippableAttribute>();
				if (skippableAttribute == null && tabModel.Actions != null && tabModel.Actions.Count > 0)
					return false; 
				return tabModel.Skippable;
			}
		}

		public void SelectItem(object obj)
		{
			if (OnSelectItem != null)
				guiContext.Send(_ => OnSelectItem(this, new EventSelectItem(obj)), null);
		}

		public void ClearSelection()
		{
			if (OnClearSelection != null)
				guiContext.Send(_ => OnClearSelection(this, new EventArgs()), null);
		}

		public virtual Bookmark CreateBookmark()
		{
			Bookmark bookmark = new Bookmark();
			//bookmark.tabBookmark.Name = Label;
			GetBookmark(bookmark.tabBookmark);
			bookmark = bookmark.Clone<Bookmark>(taskInstance.call); // sanitize
			return bookmark;
		}

		public Bookmark CreateNavigatorBookmark()
		{
			Bookmark bookmark = RootInstance.CreateBookmark();
			project.Navigator.Append(bookmark, true);
			return bookmark;
		}

		public void GetBookmark(TabBookmark tabBookmark)
		{
			tabBookmark.Name = Label;
			tabBookmark.tabViewSettings = tabViewSettings;
			foreach (TabInstance tabInstance in childTabInstances.Values)
			{
				TabBookmark childBookmark = new TabBookmark();
				//childBookmark.Name = tabInstance.Label;
				tabInstance.GetBookmark(childBookmark);
				// Change this to a Key
				if (tabBookmark.tabChildBookmarks.ContainsKey(tabInstance.Label))
				{
					// log
				}
				else
				{
					tabBookmark.tabChildBookmarks.Add(tabInstance.Label, childBookmark);
				}
			}
		}

		public TabViewSettings LoadBookmark(Bookmark bookmark)
		{
			tabBookmark = null;
			if (bookmark != null)
				this.SelectBookmark(bookmark.tabBookmark);

			return tabViewSettings; // remove?
		}

		public virtual void SelectBookmark(TabBookmark tabBookmark)
		{
			this.tabViewSettings = tabBookmark.tabViewSettings;
			this.tabBookmark = tabBookmark;
			if (OnLoadBookmark != null)
				guiContext.Send(_ => OnLoadBookmark(this, new EventArgs()), null);
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
			project.DataApp.Save(bookmark.Name, bookmark, taskInstance.call);

			//bookmark.Name = Label;
			//project.navigator.Add(bookmark);

			/*Serializer serializer = new Serializer();
			Bookmark clonedBookmark = serializer.Clone<Bookmark>(log, bookmark);


			project.navigator.Add(clonedBookmark);*/
		}

		public void LoadDefaultBookmark()
		{
			if (project.userSettings.AutoLoad == false)
				return;

			Bookmark bookmark = project.DataApp.Load<Bookmark>(CurrentBookmarkName, taskInstance.call);
			if (bookmark != null)
				this.tabBookmark = bookmark.tabBookmark;
		}

		public TabViewSettings LoadSettings()
		{
			if (tabBookmark != null && tabBookmark.tabViewSettings != null)
			{
				tabViewSettings = tabBookmark.tabViewSettings;
			}
			else
			{
				LoadDefaultTabSettings();
			}
			return tabViewSettings;
		}

		protected SortedDictionary<string, T> GetBookmarkData<T>()
		{
			var items = new SortedDictionary<string, T>();
			if (tabBookmark != null)
			{
				foreach (var row in tabBookmark.tabViewSettings.SelectedRows)
				{
					if (row.dataKey != null && row.dataValue != null && row.dataValue.GetType() == typeof(T))
						items[row.dataKey] = (T)row.dataValue;
				}
			}
			return items;
		}

		// replace with DataShared? Split call up?
		public void SaveData(string name, object obj)
		{
			project.DataApp.Save(name, obj, taskInstance.call);
		}

		public void SaveData(string directory, string name, object obj)
		{
			project.DataApp.Save(directory, name, obj, taskInstance.call);
		}

		public T LoadData<T>(string name, bool createIfNeeded = true)
		{
			T data = project.DataApp.Load<T>(name, taskInstance.call, createIfNeeded);
			return data;
		}

		public T LoadData<T>(string directory, string name, bool createIfNeeded = true)
		{
			T data = project.DataApp.Load<T>(directory, name, taskInstance.call, createIfNeeded);
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
				tabViewSettings = project.DataApp.Load<TabViewSettings>(CustomPath, taskInstance.call);
				if (tabViewSettings != null)
					return tabViewSettings;
			}

			Type type = GetType();
			if (type != typeof(TabInstance))
			{
				// Unique TabInstance
				tabViewSettings = project.DataApp.Load<TabViewSettings>(TabPath, taskInstance.call);
				if (tabViewSettings != null)
					return tabViewSettings;
			}
			else
			{
				tabViewSettings = project.DataApp.Load<TabViewSettings>(TypeLabelPath, taskInstance.call);
				if (tabViewSettings != null)
					return tabViewSettings;

				tabViewSettings = project.DataApp.Load<TabViewSettings>(TypePath, taskInstance.call);
				if (tabViewSettings != null)
					return tabViewSettings;
			}

			tabViewSettings = new TabViewSettings();
			return tabViewSettings;
		}

		public void SaveTabSettings()
		{
			if (CustomPath != null)
				project.DataApp.Save(CustomPath, tabViewSettings, taskInstance.call);

			Type type = GetType();
			if (type != typeof(TabInstance))
			{
				// Unique TabInstance
				project.DataApp.Save(TabPath, tabViewSettings, taskInstance.call);
			}
			else
			{
				project.DataApp.Save(TypeLabelPath, tabViewSettings, taskInstance.call);
				project.DataApp.Save(TypePath, tabViewSettings, taskInstance.call);
			}
			SaveDefaultBookmark();
		}

		protected void SetStartLoad()
		{
			project.DataApp.Save(LoadedPath, true, taskInstance.call);
		}

		public void SetEndLoad()
		{
			project.DataApp.Delete(typeof(bool), LoadedPath);
		}

		// for detecting parent/child loops
		public bool IsOwnerObject(object obj)
		{
			if (obj == null)
				return false;

			if (this.tabModel.Object == obj)
				return true;

			Type type = obj.GetType();
			if (iTab != null && type == iTab.GetType())
			{
				foreach (PropertyInfo propertyInfo in type.GetProperties())
				{
					if (propertyInfo.GetCustomAttribute<KeyAttribute>() != null)
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

		public TabInstance CreateChild(TabModel tabModel)
		{
			TabInstance childTabInstance = new TabInstance(project, tabModel);
			childTabInstance.ParentTabInstance = this;
			//childTabInstance.tabBookmark = tabBookmark;

			if (tabBookmark != null)
			{
				TabBookmark tabChildBookmark = null;
				if (tabBookmark.tabChildBookmarks.TryGetValue(tabModel.Name, out tabChildBookmark))
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
				if (tabBookmark.tabChildBookmarks.TryGetValue(name, out tabChildBookmark))
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
			project.Navigator.Update(bookmark);
		}
	}
}
