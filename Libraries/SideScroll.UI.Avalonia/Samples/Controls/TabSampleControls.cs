using SideScroll;
using SideScroll.Tabs;
using SideScroll.UI.Avalonia.Samples.Controls.CustomControl;

namespace SideScroll.UI.Avalonia.Samples.Controls;

public class TabSampleControls : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("Custom Control", new TabCustomControl()),
				new("Text Area", new TabTextArea()),
			};
		}
	}
}
