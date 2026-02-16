using SideScroll.Tabs;

namespace SideScroll.Avalonia.Tabs;

public class TabObjectEditable(object obj) : ITab
{
	public object Object => obj;

	public TabInstance Create() => new Instance(this);

	private class Instance(TabObjectEditable tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.MinDesiredWidth = 225;

			model.AddForm(tab.Object, true);
		}
	}
}
