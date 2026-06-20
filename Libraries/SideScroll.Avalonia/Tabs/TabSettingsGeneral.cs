using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Tabs;

[PrivateData]
public class TabSettingsGeneral(UserSettings userSettings, CallAction save, CallAction reset) : ITab
{
	public UserSettings UserSettings => userSettings;
	public CallAction Save => save;
	public CallAction Reset => reset;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonReset { get; } = new("Reset", Icons.Svg.Reset)
		{
			Flyout = new ConfirmationFlyoutConfig("Are you sure you want to reset the settings?", "Reset"),
		};

		[Separator]
		public ToolButton ButtonSave { get; } = new("Save", Icons.Svg.Save, isDefault: true);
	}

	private class Instance(TabSettingsGeneral tab) : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			model.MaxDesiredWidth = 500;

			Toolbar toolbar = new();
			toolbar.ButtonReset.Action = tab.Reset;
			toolbar.ButtonSave.Action = tab.Save;
			model.AddObject(toolbar);

			model.AddForm(tab.UserSettings);
		}
	}
}
