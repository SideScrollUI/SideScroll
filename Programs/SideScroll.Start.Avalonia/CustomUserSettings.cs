using SideScroll.Tabs.Settings;

namespace SideScroll.Start.Avalonia;

[Params]
public class CustomUserSettings : UserSettings
{
	[WordWrap]
	public string ApiUri { get; set; } = @"http://localhost:80/";

	[Separator, WordWrap]
	public int CustomLimit { get; set; } = 42;
}
