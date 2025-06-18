using SideScroll.Attributes;
using SideScroll.Tabs.Settings;

namespace SideScroll.Avalonia.Samples;

[Params]
public class CustomUserSettings : UserSettings
{
	[Header("Custom"), WordWrap]
	public string ApiUri { get; set; } = @"http://localhost:80/";

	public int CustomLimit { get; set; } = 42;
}
