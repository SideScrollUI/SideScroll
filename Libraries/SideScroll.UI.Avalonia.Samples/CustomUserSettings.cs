using SideScroll.Attributes;
using SideScroll.Tabs.Settings;

namespace SideScroll.UI.Avalonia.Samples;

[Params]
public class CustomUserSettings : UserSettings
{
	[Separator, WordWrap]
	public string ApiUri { get; set; } = @"http://localhost:80/";

	[Separator, WordWrap]
	public int CustomLimit { get; set; } = 42;
}
