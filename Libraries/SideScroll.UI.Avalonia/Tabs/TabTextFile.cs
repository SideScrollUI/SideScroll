using SideScroll;
using SideScroll.Utilities;
using SideScroll.Tabs;
using SideScroll.UI.Avalonia.Controls.TextEditor;

namespace SideScroll.UI.Avalonia.Tabs;

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
