using Atlas.Core;
using Atlas.Resources;
using System;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Tabs.Tools;

public interface IFileTypeView
{
	string Path { get; set; }
}

public class TabFile : ITab
{
	public static Dictionary<string, Type> ExtensionTypes = new();

	public string Path;

	public TabFile(string path)
	{
		Path = path;
	}

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonOpenFolder { get; set; } = new("Open Folder", Icons.Streams.OpenFolder);

		[Separator]
		public ToolButton ButtonDelete { get; set; } = new("Delete", Icons.Streams.Delete);
	}

	public class Instance : TabInstance
	{
		public TabFile Tab;

		public Instance(TabFile tab)
		{
			Tab = tab;
		}

		public override void Load(Call call, TabModel model)
		{
			string path = Tab.Path;
			if (!File.Exists(path))
			{
				model.AddObject("File doesn't exist");
				return;
			}

			var toolbar = new Toolbar();
			toolbar.ButtonOpenFolder.Action = OpenFolder;
			toolbar.ButtonDelete.Action = Delete;
			model.AddObject(toolbar);

			var items = new List<ListItem>();

			string extension = System.IO.Path.GetExtension(path).ToLower();

			if (ExtensionTypes.TryGetValue(extension, out Type? type))
			{
				var tab = (IFileTypeView)Activator.CreateInstance(type)!;
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
				if (FileUtils.IsTextFile(path))
				{
					items.Add(new ListItem("Contents", new FilePath(path)));
				}
			}
			items.Add(new ListItem("File Info", new FileInfo(path)));

			model.Items = items;
		}

		private void OpenFolder(Call call)
		{
			ProcessUtils.OpenFolder(Tab.Path);
		}

		private void Delete(Call call)
		{
			if (File.Exists(Tab.Path))
				File.Delete(Tab.Path);

			Refresh();
		}
	}
}
