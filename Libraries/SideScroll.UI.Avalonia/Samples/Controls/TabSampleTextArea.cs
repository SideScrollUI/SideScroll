using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.UI.Avalonia.Controls;

namespace SideScroll.UI.Avalonia.Samples.Controls;

public class TabSampleTextArea : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			model.MaxDesiredWidth = 800;

			var textArea = new TabControlTextArea(TextSamples.Plain);
			model.AddObject(textArea, true);
		}
	}
}
