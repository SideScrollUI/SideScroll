using SideScroll.Avalonia.Controls.TextEditor;
using SideScroll.Tabs;
using SideScroll.Utilities;

namespace SideScroll.Avalonia.Tabs;

public class TabTextFile(FilePath filePath) : ITab
{
	public FilePath FilePath => filePath;

	public TabInstance Create() => new Instance(this);

	private class Instance(TabTextFile tab) : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			TabAvaloniaEdit tabAvaloniaEdit = new();
			tabAvaloniaEdit.Load(tab.FilePath.Path);
			model.AddObject(tabAvaloniaEdit, true);
		}
	}
}
