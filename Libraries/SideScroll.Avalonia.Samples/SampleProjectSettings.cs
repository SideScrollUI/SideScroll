using SideScroll.Tabs.Settings;

namespace SideScroll.Avalonia.Samples;

public static class SampleProjectSettings
{
	public static ProjectSettings Settings => new()
	{
		Name = "SideScroll",
		LinkType = "sidescroll",
		Version = ProjectSettings.ProgramVersion(),
		DataVersion = new Version(0, 1),
	};
}
