using Atlas.Core;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;

namespace Atlas.UI.Avalonia.Tabs;

public class TabTextFile : ITab
{
	public FilePath FilePath;

	public TabTextFile(FilePath filePath)
	{
		FilePath = filePath;
	}

	public TabInstance Create() => new Instance(this);

	public class Instance(TabTextFile Tab) : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			var tabAvaloniaEdit = new TabControlAvaloniaEdit(this);
			tabAvaloniaEdit.Load(Tab.FilePath.Path);

			model.AddObject(tabAvaloniaEdit, true);
		}
	}
}
