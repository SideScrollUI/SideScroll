using Atlas.Core;
using Atlas.Resources;

namespace Atlas.Tabs.Test;

public class TabIcons : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			var toolbar = new TabToolbar();
			foreach (var memberInfo in Icons.Streams.GetItems())
			{
				if (memberInfo.Key.Name == "Logo")
					continue;

				toolbar.Buttons.Add(new ToolButton(memberInfo.Key.Name, memberInfo.Value));
			}
			model.AddObject(toolbar);
		}
	}
}
