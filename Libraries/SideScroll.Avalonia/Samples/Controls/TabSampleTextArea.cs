using SideScroll.Avalonia.Controls;
using SideScroll.Resources;
using SideScroll.Tabs;

namespace SideScroll.Avalonia.Samples.Controls;

public class TabSampleTextArea : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			model.MaxDesiredWidth = 800;

			var textArea = new TabTextArea(TextSamples.Plain);
			model.AddObject(textArea, true);
		}
	}
}
