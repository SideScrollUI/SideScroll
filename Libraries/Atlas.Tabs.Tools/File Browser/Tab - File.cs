using Atlas.Core;
using Atlas.Tabs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Atlas.Tabs.Tools
{
	public class TabFile : ITab
	{
		public static HashSet<string> TextExtensions = new HashSet<string>()
		{
			".txt",
            ".md",
            ".fna",
			".faa"
		};
		public string path;

		public TabFile(string path)
		{
			this.path = path;
		}

		public TabInstance Create() { return new Instance(this); }

		public class Instance : TabInstance
		{
			private TabFile tab;

			public Instance(TabFile tab)
			{
				this.tab = tab;
			}

			public override void Load(Call call)
			{
				ItemCollection<ListItem> items = new ItemCollection<ListItem>();

				string extension = Path.GetExtension(tab.path);

				if (extension == ".json")
				{
					items.Add(new ListItem("Contents", LazyJsonNode.LoadPath(tab.path)));
					//items.Add(new ListItem("Contents", JsonValue.Parse(File.ReadAllText(path))));
				}
				else
				{
					bool isText = TextExtensions.Contains(extension);
					if (!isText)
					{
						try
						{
							// doesn't work
							using (StreamReader streamReader = File.OpenText(tab.path))
							{
								char[] buffer = new char[100];
								streamReader.Read(buffer, 0, buffer.Length);
								isText = true;
							}
						}
						catch (Exception)
						{
						}
					}

					if (isText)
					{
						items.Add(new ListItem("Contents", new FilePath(tab.path)));
					}
					else
					{
						items.Add(new ListItem("Contents", null));
					}
				}
				tabModel.Items = items;

				/*ItemCollection<TaskCreator> actions = new ItemCollection<TaskCreator>();
				actions.Add(new TaskDelegate("Save", Save));
				tabModel.Actions = actions;*/
			}

			private void Save(Call call)
			{
			}
		}
	}
}
