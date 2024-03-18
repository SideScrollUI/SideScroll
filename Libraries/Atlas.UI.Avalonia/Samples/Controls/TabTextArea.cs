using Atlas.Core;
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
			var textArea = new TabControlTextArea(Resources.Samples.Text.Plain);
			model.AddObject(textArea, true);
		}
	}
}
