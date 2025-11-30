using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Time;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.Tabs.Settings;

public class UserSettings
{
	[Header("Base")]
	public bool AutoSelect { get; set; } = true;

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

	public bool? EnableCustomTitleBar { get; set; }

	[Hidden]
	public DataSettings DataSettings { get; set; } = new();

	public override string ToString() => DataSettings.ToString();
}

public class DataSettings
{
	[ReadOnly(true), WordWrap]
	public string? AppDataPath { get; set; }

	[ReadOnly(true), WordWrap]
	public string? LocalDataPath { get; set; }

	[Hidden]
	public string? LinkId { get; set; }

	[Hidden]
	public string SettingsPath => Paths.Combine(AppDataPath, "Settings.atlas");

	[Range(1, 1000)]
	public int CacheDurationDays { get; set; } = 30;

	[Range(1, 100)]
	public int MaxHistory { get; set; } = 20;

	public override string ToString() => SettingsPath;
}
