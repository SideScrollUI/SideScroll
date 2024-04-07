using Atlas.Core;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Samples.Controls.CustomControl;

namespace Atlas.UI.Avalonia.Samples.Controls;

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
