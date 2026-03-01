using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Time;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.Tabs.Settings;

/// <summary>
/// User-specific settings for SideScroll including UI preferences, time zones, and data storage configuration
/// </summary>
public class UserSettings
{
	/// <summary>
	/// Gets or sets whether to automatically select items in tabs (default: true)
	/// </summary>
	[Header("Base")]
	public bool AutoSelect { get; set; } = true;

	/// <summary>
	/// Gets or sets the maximum number of vertical tabs to display (range: 1-20, default: 10)
	/// </summary>
	[Range(1, 20)]
	public int VerticalTabLimit { get; set; } = 10;

	/// <summary>
	/// Gets or sets the list of available time zones
	/// </summary>
	public static List<TimeZoneView> TimeZones { get; set; } = TimeZoneView.All;

	/// <summary>
	/// Gets or sets the selected time zone (default: local time zone)
	/// </summary>
	[Separator, BindList(nameof(TimeZones))]
	public TimeZoneView TimeZone { get; set; } = TimeZoneView.Local;

	/// <summary>
	/// Gets or sets the time format display type (default: Minute precision)
	/// </summary>
	public TimeFormatType TimeFormat { get; set; } = TimeFormatType.Minute;

	/// <summary>
	/// Gets or sets the list of available themes
	/// </summary>
	public static IList? Themes { get; set; }

	/// <summary>
	/// Gets or sets the selected theme name
	/// </summary>
	[Separator, BindList(nameof(Themes))]
	public string? Theme { get; set; }

	/// <summary>
	/// Gets or sets whether to enable custom title bar (requires restart when changed)
	/// </summary>
	[ToolTip("Restart Required")]
	public bool? EnableCustomTitleBar { get; set; }

	/// <summary>
	/// Gets or sets the data storage settings
	/// </summary>
	[Hidden]
	public DataSettings DataSettings { get; set; } = new();

	public override string ToString() => DataSettings.ToString();
}

/// <summary>
/// Data storage settings including paths for app data, cache, and history configuration
/// </summary>
public class DataSettings
{
	/// <summary>
	/// Gets or sets the application data directory path (read-only in UI)
	/// </summary>
	[ReadOnly(true), WordWrap]
	public string? AppDataPath { get; set; }

	/// <summary>
	/// Gets or sets the local data directory path for cache and temporary files (read-only in UI)
	/// </summary>
	[ReadOnly(true), WordWrap]
	public string? LocalDataPath { get; set; }

	/// <summary>
	/// Gets or sets the link identifier for bookmark-specific data storage
	/// </summary>
	[Hidden]
	public string? LinkId { get; set; }

	/// <summary>
	/// Gets the full path to the settings file
	/// </summary>
	[Hidden]
	public string SettingsPath => Paths.Combine(AppDataPath, "Settings.atlas");

	/// <summary>
	/// Gets or sets the number of days to retain cached data (range: 1-1000, default: 30)
	/// </summary>
	[Range(1, 1000)]
	public int CacheDurationDays { get; set; } = 30;

	/// <summary>
	/// Gets or sets the maximum number of navigation history items to retain (range: 1-100, default: 20)
	/// </summary>
	[Range(1, 100)]
	public int MaxHistory { get; set; } = 20;

	public override string ToString() => SettingsPath;
}
