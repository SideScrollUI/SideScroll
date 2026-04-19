using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples;

public class TabSampleIcons : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		private const int IconsPerRow = 24;

		public override void Load(Call call, TabModel model)
		{
			var buttons = Icons.Svg.Items
				.Select(memberInfo => new ToolButton(memberInfo.Key.Name, memberInfo.Value));

			foreach (var chunk in buttons.Chunk(IconsPerRow))
			{
				TabToolbar tabToolbar = new()
				{
					Buttons = [.. chunk]
				};
				model.AddObject(tabToolbar);
			}
		}
	}
}
