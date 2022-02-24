using Atlas.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Atlas.Tabs.Tools;

public interface IFileTypeView
{
	string Path { get; set; }
}

public class TabFile : ITab
{
	public static HashSet<string> TextExtensions = new()
	{
		".csv",
		".html",
		".ini",
		".log",
		".md",
		".txt",
	};

	public static Dictionary<string, Type> ExtensionTypes = new();

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

			string path = Tab.Path;
			string extension = System.IO.Path.GetExtension(path);

			if (ExtensionTypes.TryGetValue(extension, out Type type))
			{
				var tab = (IFileTypeView)Activator.CreateInstance(type);
				tab.Path = path;
				items.Add(new ListItem(extension, tab));
			}

			if (extension == ".json")
			{
				string text = File.ReadAllText(path);
				items.Add(new ListItem("Contents", text));
				items.Add(new ListItem("Json", LazyJsonNode.Parse(text)));
			}
			else
			{
				bool isText = TextExtensions.Contains(extension);
				if (!isText)
				{
					try
					{
						using StreamReader streamReader = File.OpenText(path);

						var buffer = new char[100];
						streamReader.Read(buffer, 0, buffer.Length);
						isText = !buffer.Any(ch => char.IsControl(ch) && ch != '\r' && ch != '\n');
					}
					catch (Exception)
					{
					}
				}

				if (isText)
				{
					items.Add(new ListItem("Contents", new FilePath(path)));
				}
				else
				{
					items.Add(new ListItem("Contents", null));
				}
			}
			model.Items = items;

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Open Folder", OpenFolder, true),
			};
		}

		private void OpenFolder(Call call)
		{
			ProcessUtils.OpenFolder(Tab.Path);
		}
	}
}
