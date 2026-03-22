using SideScroll.Resources;
using SideScroll.Tabs.Settings;

namespace SideScroll.Avalonia.Samples;

public static class SampleProjectSettings
{
	public static ProjectSettings Settings => new()
	{
		Name = "SideScroll",
		LinkType = "sidescroll",
		Version = ProjectSettings.ProgramVersion(),
		DataVersion = new Version(0, 16, 0),
		UseJsonSerialization = true, // Enable JSON serialization for testing
		CustomTitleIcon = Logo.Svg.SideScrollTranslucent,
	};
}
