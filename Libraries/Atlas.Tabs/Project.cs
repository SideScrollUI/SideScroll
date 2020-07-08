using Atlas.Core;
using Atlas.Extensions;
using Atlas.Network;
using Atlas.Serialize;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs
{
	public class Project
	{
		public string Name => ProjectSettings.Name;	// for viewing purposes
		public string LinkType => ProjectSettings.LinkType; // for bookmarking
		public Version Version => ProjectSettings.Version;
		public virtual ProjectSettings ProjectSettings { get; set; }
		public virtual UserSettings UserSettings { get; set; }

		public DataRepo DataShared => new DataRepo(DataRepoPath, "Shared");
		public DataRepo DataApp => new DataRepo(DataRepoPath, "Programs/" + Name + "/" + ProjectSettings.DataVersion);

		public HttpCacheManager httpCacheManager = new HttpCacheManager();

		public TypeObjectStore TypeObjectStore { get; set; } = new TypeObjectStore();
		public BookmarkNavigator Navigator { get; set; } = new BookmarkNavigator();
		public TaskInstanceCollection Tasks { get; set; } = new TaskInstanceCollection();

		private string DataRepoPath => Paths.Combine(UserSettings.ProjectPath, UserSettings.BookmarkPath?.HashSha256(), "Data");


		public Project()
		{
		}

		public Project(ProjectSettings projectSettings)
		{
			ProjectSettings = projectSettings;
			UserSettings = new UserSettings()
			{
				ProjectPath = projectSettings.DefaultProjectPath,
			};
		}

		public Project(ProjectSettings projectSettings, UserSettings userSettings)
		{
			ProjectSettings = projectSettings;
			UserSettings = userSettings;
		}

		public override string ToString()
		{
			return Name;
		}

		public void SaveSettings()
		{
			//tabInstance.project.DataApp.Save(projectSettings, new Call());

			var serializer = new SerializerFile(UserSettings.SettingsPath, "");
			serializer.Save(new Call(), ProjectSettings);
		}

		public Project Open(Bookmark bookmark)
		{
			var userSettings = UserSettings.Clone<UserSettings>();
			userSettings.BookmarkPath = bookmark.Address;
			var project = new Project(ProjectSettings, userSettings);
			//project.Import(bookmark);
			bookmark.TabBookmark.Import(project);
			return project;
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
			if (Items.TryGetValue(typeof(T), out object obj))
				return (T)obj;
			return default;
		}

		public object Get(Type type)
		{
			if (Items.TryGetValue(type, out object obj))
				return obj;
			return null;
		}
	}

	public interface IProject
	{
		void Restart();
	}
}
