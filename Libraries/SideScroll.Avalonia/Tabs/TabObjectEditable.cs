using SideScroll.Tabs;

namespace SideScroll.Avalonia.Tabs;

public class TabObjectEditable(object obj) : ITab
{
	public object Object => obj;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabObjectEditable tab) : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			model.MinDesiredWidth = 225;

			model.AddObject(tab.Object, true);
		}
	}
}
