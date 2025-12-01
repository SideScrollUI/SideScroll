using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples;

public class TabIcons : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			TabToolbar toolbarSvg = new()
			{
				Buttons = Icons.Svg.Items
					.Select(memberInfo => new ToolButton(memberInfo.Key.Name, memberInfo.Value))
					.ToList()
			};
			model.AddObject(toolbarSvg);
		}
	}
}
