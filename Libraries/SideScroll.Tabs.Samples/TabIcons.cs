using SideScroll;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples;

public class TabIcons : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			TabToolbar toolbarSvg = new();
			foreach (var memberInfo in Icons.Svg.GetItems())
			{
				toolbarSvg.Buttons.Add(new ToolButton(memberInfo.Key.Name, memberInfo.Value));
			}
			model.AddObject(toolbarSvg);

			TabToolbar toolbarPng = new();
			foreach (var memberInfo in Icons.Png.GetItems())
			{
				if (memberInfo.Key.Name == "Logo") continue;

				toolbarPng.Buttons.Add(new ToolButton(memberInfo.Key.Name, memberInfo.Value));
			}
			model.AddObject(toolbarPng);
		}
	}
}
