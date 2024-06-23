using SideScroll.Serialize;
using SideScroll.Tabs.Lists;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Tools;

public class TabProjectSettings : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = ListProperty.Create(Project.ProjectSettings);
			model.Editing = true;

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Reset", Reset),
				new TaskDelegate("Save", Save),
			};
		}

		private void Reset(Call call)
		{
			// How do we replace a shared pointer that exists everywhere? references?
			//call.Application.Restart();
			var serializer = SerializerFile.Create(Project.UserSettings.SettingsPath);
			serializer.Save(call, new ProjectSettings());
			Environment.Exit(0);
		}

		private void Save(Call call)
		{
			var serializer = SerializerFile.Create(Project.UserSettings.SettingsPath);
			serializer.Save(call, Project.ProjectSettings);
		}
	}
}
