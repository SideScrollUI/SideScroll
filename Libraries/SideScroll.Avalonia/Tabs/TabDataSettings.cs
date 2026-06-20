using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Tabs;

[PrivateData]
public class TabDataSettings(UserSettings userSettings, CallAction save) : ITab
{
	public UserSettings UserSettings => userSettings;
	public CallAction Save => save;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonSave { get; } = new("Save", Icons.Svg.Save, isDefault: true);
	}

	private class Instance(TabDataSettings tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonSave.Action = tab.Save;
			model.AddObject(toolbar);

			model.AddForm(tab.UserSettings.DataSettings);
		}
	}
}
