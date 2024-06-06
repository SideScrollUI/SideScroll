using Atlas.Core;
using Atlas.Core.Utilities;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls.TextEditor;

namespace Atlas.UI.Avalonia.Tabs;

public class TabTextFile(FilePath filePath) : ITab
{
	public FilePath FilePath = filePath;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabTextFile tab) : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			var tabAvaloniaEdit = new TabControlAvaloniaEdit(this);
			tabAvaloniaEdit.Load(tab.FilePath.Path);

			model.AddObject(tabAvaloniaEdit, true);
		}
	}
}
