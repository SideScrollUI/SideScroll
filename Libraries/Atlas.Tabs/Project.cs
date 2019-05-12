using Atlas.Core;
using Atlas.Network;
using Atlas.Serialize;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs
{
	public class Project
	{
		public string Name => projectSettings.Name; // for viewing purposes
		public string Version { get; set; } = "0";
		public virtual ProjectSettings projectSettings { get; set; }

		public DataRepo DataShared { get { return new DataRepo(DataRepoPath, "Shared"); } }
		public DataRepo DataApp { get { return new DataRepo(DataRepoPath, "Programs/" + Name + "/" + Version); } }

		public HttpCacheManager httpCacheManager = new HttpCacheManager();

		public TypeObjectStore TypeObjectStore { get; set; } = new TypeObjectStore();
		public BookmarkNavigator Navigator { get; set; } = new BookmarkNavigator();
		public TaskInstanceCollection Tasks { get; set; } = new TaskInstanceCollection();


		public Project()
		{
		}

		public Project(ProjectSettings settings)
		{
			this.projectSettings = settings;
		}

		public Project(string projectPath, string name)
		{
			Call call = new Call();
			string settingsPath = ProjectSettings.GetSettingsPath(projectPath);
			var serializer = new SerializerFile(settingsPath, name);
			projectSettings = serializer.LoadOrCreate<ProjectSettings>(call);
			projectSettings.ProjectPath = projectPath;
			projectSettings.Name = name;
		}

		public void SaveSettings()
		{
			//tabInstance.project.DataApp.Save(projectSettings, new Call());

			var serializer = new SerializerFile(projectSettings.SettingsPath, "");
			serializer.Save(new Call(), projectSettings);
		}

		public override string ToString()
		{
			return Name;
		}

		private string DataRepoPath
		{
			get { return Paths.Combine(projectSettings.ProjectPath, "Data"); }
		}
	}

	public class TypeObjectStore
	{
		public Dictionary<Type, object> Items { get; set; } = new Dictionary<Type, object>();

		public void Add(object obj)
		{
			Items.Add(obj.GetType(), obj);
		}

		public T Get<T>()
		{
			object obj;
			if (Items.TryGetValue(typeof(T), out obj))
				return (T)obj;
			return default(T);
		}

		public object Get(Type type)
		{
			object obj;
			if (Items.TryGetValue(type, out obj))
				return obj;
			return null;
		}
	}

	public interface IProject
	{
		void Restart();
	}
}
