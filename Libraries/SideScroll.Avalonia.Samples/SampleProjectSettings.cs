using SideScroll.Resources;
using SideScroll.Tabs.Settings;

namespace SideScroll.Avalonia.Samples;

public class SampleProjectSettings : ProjectSettings
{
	public override SampleUserSettings DefaultUserSettings => new()
	{
		EnableCustomTitleBar = DefaultEnableCustomTitlebar,
		DataSettings = new()
		{
			AppDataPath = DefaultAppDataPath,
			LocalDataPath = DefaultLocalDataPath,
		},
	};

	public static SampleProjectSettings Default => new()
	{
		Name = "SideScroll",
		LinkType = "sidescroll",
		Version = ProgramVersion(),
		DataVersion = new Version(0, 20, 0),
		CustomTitleIcon = Logo.Svg.SideScrollTranslucent,
	};
}
