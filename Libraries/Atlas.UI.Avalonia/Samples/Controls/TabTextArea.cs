using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;

namespace Atlas.UI.Avalonia.Samples.Controls;

public class TabTextArea : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			var textArea = new TabControlTextArea(TextSamples.Plain);
			model.AddObject(textArea, true);
		}
	}
}
