using System;
using System.Collections.Generic;
using Atlas.Core;
using Atlas.Serialize;

namespace Atlas.Tabs.Tools
{
	public class TabSettings : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				tabModel.Items = ListProperty.Create(this.project.projectSettings);
				tabModel.Editing = true;

				ItemCollection<TaskCreator> actions = new ItemCollection<TaskCreator>();
				actions.Add(new TaskDelegate("Reset", Reset));
				actions.Add(new TaskDelegate("Save", Save));
				tabModel.Actions = actions;
			}

			private void Reset(Call call)
			{
				// How do we replace a shared pointer that exists everywhere? references?
				//call.Application.Restart();
				var serializer = new SerializerFile(project.userSettings.SettingsPath);
				serializer.Save(call, new ProjectSettings());
				Environment.Exit(0);
			}

			private void Save(Call call)
			{
				var serializer = new SerializerFile(project.userSettings.SettingsPath);
				serializer.Save(call, project.projectSettings);
			}
		}
	}
}
