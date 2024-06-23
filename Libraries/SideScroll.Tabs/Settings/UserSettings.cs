using SideScroll.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.Tabs.Settings;

[Params]
public class UserSettings
{
	[Hidden]
	public string? ProjectPath { get; set; }
	[Hidden]
	public string? BookmarkPath { get; set; }

	[Hidden]
	public string SettingsPath => Paths.Combine(ProjectPath, "Settings.atlas");

	[Separator]
	public bool AutoLoad { get; set; } = true;

	[Range(1, 20)]
	public int VerticalTabLimit { get; set; } = 10;

	//public int MaxLogItems { get; set; } = 100_000;

	public static List<string> Themes { get; set; } = [];

	[BindList(nameof(Themes))]
	public string? Theme { get; set; }

	public override string ToString() => SettingsPath;
}
