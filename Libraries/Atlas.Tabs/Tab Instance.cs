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
using static Atlas.Core.TaskDelegateParams;

namespace Atlas.Tabs
{
	public interface ITab
	{
		TabInstance Create();
	}

	public interface ITabAsync
	{
		Task LoadAsync();
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

		public DataRepo DataShared => project.DataShared;

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
		private string CustomPath { get { return tabModel.CustomSettingsPath != null ? "Custom/" + GetType().FullName + "/" + tabModel.CustomSettingsPath : null; } }
		private string TabPath { get { return "Tab/" + GetType().FullName + "/" + tabModel.ObjectTypePath; } }
		//private string TabPath { get { return "Tab/" + GetType().FullName + "/" + tabModel.ObjectTypePath + "/" + Label; } }
		// deprecate?
		private string TypeLabelPath { get { return "TypePath/" + tabModel.ObjectTypePath + "/" + Label; } }
		private string TypePath { get { return "Type/" + tabModel.ObjectTypePath; } }

		private string LoadedPath { get { return "Loaded/" + tabModel.ObjectTypePath; } }

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
			tabInstance.tabBookmark = tabBookmark;
			FillInheritables(tabInstance);
			return tabInstance;
		}

		public TabInstance CreateChildTab(TabModel tabModel)
		{
			TabInstance tabInstance = new TabInstance(project, tabModel);
			//tabInstance.project = project;
			//tabInstance.iTab = iTab;
			tabInstance.ParentTabInstance = this;
			tabInstance.tabBookmark = tabBookmark;
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

		public void Invoke(Action action)
		{
			guiContext.Send(ActionCallback, action);
		}

		private void ActionCallback(object state)
		{
			Action action = (Action)state;
			action.Invoke();
		}

		public void Invoke(SendOrPostCallback callback, object param = null)
		{
			guiContext.Send(callback, param);
		}

		// switch to SendOrPostCallback?
		public void Invoke(CallActionParams callAction, params object[] objects)
		{
			TaskDelegateParams taskDelegate = new TaskDelegateParams(callAction.Method.Name, callAction, false, null, objects);
			guiContext.Send(ActionParamsCallback, taskDelegate);
		}

		private void ActionParamsCallback(object state)
		{
			TaskDelegateParams taskDelegate = (TaskDelegateParams)state;
			StartTask(taskDelegate);
		}

		public void StartTask(TaskCreator taskCreator)
		{
			Call call = new Call(taskCreator.Label);
			TaskInstance taskInstance = taskCreator.Start(call);
			tabModel.Tasks.Add(taskInstance);
		}

		public void StartTask(CallAction callAction, bool useTask)
		{
			TaskDelegate taskDelegate = new TaskDelegate(callAction.Method.Name, callAction, useTask);
			StartTask(taskDelegate);
		}

		public void StartTask(CallActionParams callAction, bool useTask, params object[] objects)
		{
			TaskDelegateParams taskDelegate = new TaskDelegateParams(callAction.Method.Name, callAction, useTask, null, objects);
			StartTask(taskDelegate);
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

		public virtual void Load()
		{
		}

		// Reload to initial state
		private bool isLoaded = false;
		public void Reintialize()
		{
			if (isLoaded)
				return;

			//MethodInfo methodInfo = GetDerivedLoadMethod();
			if (this is ITabAsync tabAsync)
			{
				tabModel.Clear(); // don't clear for Tab Instances, only auto generated
				Task.Run(() => tabAsync.LoadAsync()).Wait(); // Call this way to avoid .Result deadlock
			}
			else if (CanLoad)
			{
				tabModel.Clear(); // don't clear for Tab Instances, only auto generated
				Load(); // Creates a tabModel if none exists and adds other Controls
				isLoaded = true;
			}

			// Have return tabModel?
		}

		// calls Load and then Refresh
		public void Reload()
		{
			isLoaded = false;
			if (OnReload != null)
				guiContext.Send(_ => OnReload(this, new EventArgs()), null);
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
				if (tabViewSettings.SelectedRows.Count == 0)
					return false;
				if (tabModel.Objects.Count > 0)
					return false; // Only data is skippable?
				if (tabModel.Actions != null && tabModel.Actions.Count > 0)
					return false; // Only data is skippable?
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

		public Bookmark CreateBookmark()
		{
			Bookmark bookmark = new Bookmark();
			//bookmark.tabBookmark.Name = Label;
			GetBookmark(bookmark.tabBookmark);
			bookmark = Serialize.SerializerMemory.Clone<Bookmark>(taskInstance.call, bookmark); // sanitize
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

		public void SelectBookmark(TabBookmark tabBookmark)
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
			bookmark.Name = CurrentBookmarkName;
			project.DataShared.Save(bookmark.Name, bookmark, taskInstance.call);

			//bookmark.Name = Label;
			//project.navigator.Add(bookmark);

			/*Serializer serializer = new Serializer();
			Bookmark clonedBookmark = serializer.Clone<Bookmark>(log, bookmark);


			project.navigator.Add(clonedBookmark);*/
		}

		public void LoadDefaultBookmark()
		{
			if (project.projectSettings.AutoLoad == false)
				return;

			Bookmark bookmark = project.DataShared.Load<Bookmark>(CurrentBookmarkName, taskInstance.call);
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

		// replace with DataShared? Split call up?
		public void SaveData(string name, object obj)
		{
			project.DataShared.Save(name, obj, taskInstance.call);
		}

		public void SaveData(string directory, string name, object obj)
		{
			project.DataShared.Save(directory, name, obj, taskInstance.call);
		}

		public T LoadData<T>(string name, bool createIfNeeded = true)
		{
			T data = project.DataShared.Load<T>(name, taskInstance.call, createIfNeeded);
			return data;
		}

		public T LoadData<T>(string directory, string name, bool createIfNeeded = true)
		{
			T data = project.DataShared.Load<T>(directory, name, taskInstance.call, createIfNeeded);
			return data;
		}

		/*public ItemCollection<T> LoadAllData<T>(string directory = null)
		{
			ItemCollection<T> datas = project.DataShared.LoadAll<T>(taskInstance.call, directory);
			return datas;
		}*/

		public TabViewSettings LoadBookmark()
		{
			tabBookmark = null;
			return tabViewSettings;
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
			if (this.tabModel.Object == obj)
				return true;
			else if (ParentTabInstance != null)
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
			childTabInstance.tabBookmark = tabBookmark;
			return childTabInstance;
		}

		private object GetBookmarkObject(string name)
		{
			// FindMatches uses bookmarks
			TabBookmark tabBookmark = null;
			if (this.tabBookmark != null)
			{
				if (this.tabBookmark.tabChildBookmarks.TryGetValue(name, out tabBookmark))
				{
					if (tabBookmark.tabModel != null)
						return tabBookmark.tabModel;
				}
				/*foreach (Bookmark.Node node in tabInstance.tabBookmark.nodes)
				{
					tabBookmark = node;
					break;
				}*/
			}
			return tabBookmark;
		}

		public void UpdateNavigator()
		{
			Bookmark bookmark = RootInstance.CreateBookmark(); // create from root Tab
			project.Navigator.Update(bookmark);
		}
	}
}
