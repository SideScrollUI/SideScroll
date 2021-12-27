using Atlas.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Tabs.Tools
{
	public class TabFile : ITab
	{
		public static HashSet<string> TextExtensions = new HashSet<string>()
		{
			".txt",
			".md",
		};

		public string Path;

		public TabFile(string path)
		{
			Path = path;
		}

		public TabInstance Create() => new Instance(this);

		public class Instance : TabInstance
		{
			public TabFile Tab;

			public Instance(TabFile tab)
			{
				Tab = tab;
			}

			public override void Load(Call call, TabModel model)
			{
				var items = new ItemCollection<ListItem>();

				string extension = System.IO.Path.GetExtension(Tab.Path);

				if (extension == ".json")
				{
					items.Add(new ListItem("Contents", LazyJsonNode.LoadPath(Tab.Path)));
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
							using StreamReader streamReader = File.OpenText(Tab.Path);

							var buffer = new char[100];
							streamReader.Read(buffer, 0, buffer.Length);
							isText = true;
						}
						catch (Exception)
						{
						}
					}

					if (isText)
					{
						items.Add(new ListItem("Contents", new FilePath(Tab.Path)));
					}
					else
					{
						items.Add(new ListItem("Contents", null));
					}
				}
				model.Items = items;
			}
		}
	}
}
