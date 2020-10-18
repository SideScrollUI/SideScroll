using Atlas.Core;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Tabs.Test
{
	public class TabTestProcess : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				/*tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Sample Text", "This is some sample text\n\n1\n2\n3"),
				};*/
				model.Actions = new List<TaskCreator>
				{
					new TaskDelegate("Open Folder", OpenFolder, true),
				};
			}

			private void OpenFolder(Call call)
			{
				ProcessUtils.OpenFolder(Directory.GetCurrentDirectory());
			}
		}
	}
}
