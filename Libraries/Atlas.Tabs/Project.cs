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
		public string Name => ProjectSettings.Name; // for viewing purposes
		public string LinkType => ProjectSettings.LinkType; // for bookmarking
		public Version Version => ProjectSettings.Version;
		public virtual ProjectSettings ProjectSettings { get; set; }
		public virtual UserSettings UserSettings { get; set; }

		public Linker Linker { get; set; } = new Linker();

		public DataRepo DataShared => new DataRepo(DataRepoPath, "Shared");
		public DataRepo DataApp => new DataRepo(DataRepoPath, "Versions/" + ProjectSettings.DataVersion);

		public HttpCacheManager Http = new HttpCacheManager();

		public TypeObjectStore TypeObjectStore { get; set; } = new TypeObjectStore();
		public BookmarkNavigator Navigator { get; set; } = new BookmarkNavigator();
		public TaskInstanceCollection Tasks { get; set; } = new TaskInstanceCollection();

		private string DataRepoPath
		{
			get
			{
				if (UserSettings.BookmarkPath != null)
					return Paths.Combine(UserSettings.ProjectPath, "Bookmarks", UserSettings.BookmarkPath.HashSha256(), "Data");
				else
					return Paths.Combine(UserSettings.ProjectPath, "Current", "Data");
			}
		}

		public override string ToString() => Name;

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

		public void SaveSettings()
		{
			//tabInstance.project.DataApp.Save(projectSettings, new Call());

			var serializer = SerializerFile.Create(UserSettings.SettingsPath, "");
			serializer.Save(new Call(), ProjectSettings);
		}

		public Project Open(Bookmark bookmark)
		{
			UserSettings userSettings = UserSettings.DeepClone();
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
