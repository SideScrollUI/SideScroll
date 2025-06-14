using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Time;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.Tabs.Settings;

[Params]
public class UserSettings
{
	[Hidden]
	public string? AppDataPath { get; set; }

	[Hidden]
	public string? LocalDataPath { get; set; }

	[Hidden]
	public string? LinkId { get; set; }

	[Hidden]
	public string SettingsPath => Paths.Combine(AppDataPath, "Settings.atlas");

	[Separator]
	public bool AutoLoad { get; set; } = true;

	[Range(1, 100)]
	public int MaxHistory { get; set; } = 20;

	[Range(1, 20)]
	public int VerticalTabLimit { get; set; } = 10;

	//public int MaxLogItems { get; set; } = 100_000;

	public static List<TimeZoneView> TimeZones { get; set; } = TimeZoneView.All;

	[Separator, BindList(nameof(TimeZones))]
	public TimeZoneView TimeZone { get; set; } = TimeZoneView.Local;

	public TimeFormatType TimeFormat { get; set; } = TimeFormatType.Minute;

	public static IList? Themes { get; set; }

	[Separator, BindList(nameof(Themes))]
	public string? Theme { get; set; }

	public override string ToString() => SettingsPath;
}
