using Avalonia;
using Avalonia.Styling;
using SideScroll.Avalonia.Themes.Tabs;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using System.Text.Json;

namespace SideScroll.Avalonia.Themes;

/// <summary>Manages application theme settings, loading and saving themes to data repos, and applying them to the Avalonia application.</summary>
public class ThemeManager
{
	/// <summary>Gets the data repo group ID used to store the built-in default themes.</summary>
	public const string DefaultGroupId = "DefaultThemes";

	/// <summary>Gets the data repo group ID used to store user-customizable themes.</summary>
	public const string GroupId = "Themes";

	/// <summary>Gets or sets the current singleton <see cref="ThemeManager"/> instance.</summary>
	public static ThemeManager? Instance { get; set; }

	/// <summary>Gets the theme settings that are currently applied to the application.</summary>
	public static AvaloniaThemeSettings? CurrentTheme { get; protected set; }

	/// <summary>Gets the JSON serializer options used when deserializing theme JSON, including the color converter.</summary>
	public static JsonSerializerOptions JsonSerializerOptions { get; } = CreateJsonSerializerOptions();

	/// <summary>Gets the project whose data stores are used to persist themes.</summary>
	public Project Project { get; }

	/// <summary>Gets the current Avalonia application.</summary>
	public static Application Application => Application.Current!;

	/// <summary>Gets the display names of all user themes in the themes repo.</summary>
	public List<string> Names => DataRepoThemes.Items
		.Select(i => i.Value.Name!)
		.ToList();

	/// <summary>Gets the data repo view for built-in default themes, used as the reset baseline.</summary>
	public DataRepoView<AvaloniaThemeSettings> DataRepoDefaultThemes { get; }

	/// <summary>Gets the data repo view for user-customizable themes.</summary>
	public DataRepoView<AvaloniaThemeSettings> DataRepoThemes { get; }

	/// <summary>Initializes a new <see cref="ThemeManager"/> for the given project and loads all saved themes into memory.</summary>
	public ThemeManager(Project project)
	{
		Project = project;

		DataRepoDefaultThemes = Project.Data.App.LoadView<AvaloniaThemeSettings>(new(), DefaultGroupId, nameof(AvaloniaThemeSettings.Name));

		DataRepoThemes = Project.Data.App.LoadView<AvaloniaThemeSettings>(new(), GroupId, nameof(AvaloniaThemeSettings.Name));
		foreach (AvaloniaThemeSettings theme in DataRepoThemes.Values)
		{
			UpdateTheme(theme);
		}
		UserSettings.Themes = DataRepoThemes.Items;
	}

	/// <summary>Returns the user theme with the given name after filling any missing values, or <c>null</c> if not found.</summary>
	public AvaloniaThemeSettings? GetUpdatedTheme(string? themeName)
	{
		if (themeName == null) return null;

		var theme = DataRepoThemes.Values.FirstOrDefault(theme => theme.Name == themeName);
		UpdateTheme(theme);
		return theme;
	}

	/// <summary>If <paramref name="themeSettings"/> has any null resource values, fills them from defaults and re-saves the theme.</summary>
	public void UpdateTheme(AvaloniaThemeSettings? themeSettings)
	{
		if (themeSettings?.HasNullValue() == true)
		{
			themeSettings.FillMissingValues();
			themeSettings.Version = Project.Version;
			DataRepoThemes.Save(null, themeSettings);
		}
	}

	/// <summary>Loads and applies the theme selected in the project's user settings, if one is configured.</summary>
	public void LoadCurrentTheme()
	{
		var theme = GetUpdatedTheme(Project.UserSettings.Theme);
		if (theme == null) return;

		LoadTheme(theme);
	}

	/// <summary>Ensures the named theme variant exists in both the default and user repos, creating or updating it as needed for the current version.</summary>
	public void AddThemeVariant(Call call, string variant)
	{
		// Overwrite previous if not found since the change is so large. Todo: Remove eventually
		bool isHybridFound = DataRepoDefaultThemes.Keys.Contains("Hybrid");

		// Always overwrite default themes when the version changes
		var defaultTheme = DataRepoDefaultThemes.Values.FirstOrDefault(theme => theme.Name == variant);
		if (defaultTheme == null || defaultTheme.Version != Project.Version || defaultTheme.HasNullValue() || !isHybridFound)
		{
			defaultTheme = Create(variant, variant);
			DataRepoDefaultThemes.Save(call, defaultTheme);
		}

		// Don't replace user modified themes, but update them to add new resources
		if (!isHybridFound ||
			GetUpdatedTheme(variant) is not { } existingThemeSettings ||
			(existingThemeSettings.ModifiedAt == null && existingThemeSettings.Version != Project.Version))
		{
			DataRepoThemes.Save(call, defaultTheme);
		}
	}

	/// <summary>Deserializes a theme from JSON and saves it to the default and/or user repo if it does not yet exist or the version has changed.</summary>
	public void AddJson(Call call, string json, bool isDefault = false)
	{
		var themeSettings = JsonSerializer.Deserialize<AvaloniaThemeSettings>(json, JsonSerializerOptions)!;
		themeSettings.Version = Project.Version;

		if (isDefault)
		{
			var defaultTheme = DataRepoDefaultThemes.Values.FirstOrDefault(theme => theme.Name == themeSettings.Name);
			if (defaultTheme == null || defaultTheme.Version != Project.Version)
			{
				themeSettings.FillMissingValues();
				DataRepoDefaultThemes.Save(call, themeSettings);
			}
		}

		if (GetUpdatedTheme(themeSettings.Name) is not { } existingThemeSettings ||
			(existingThemeSettings.ModifiedAt == null && existingThemeSettings.Version != Project.Version))
		{
			DataRepoThemes.Save(call, themeSettings);
		}
	}

	/// <summary>Creates a new <see cref="AvaloniaThemeSettings"/> for the given variant by temporarily switching the application theme and reading the resulting resource values.</summary>
	public AvaloniaThemeSettings Create(string name, string variant)
	{
		AvaloniaThemeSettings themeSettings = new()
		{
			Name = name,
			Variant = variant,
			Version = Project.Version,
		};

		var original = Application.RequestedThemeVariant;
		Application.RequestedThemeVariant = themeSettings.GetVariant();
		themeSettings.LoadFromCurrent();
		Application.RequestedThemeVariant = original;

		return themeSettings;
	}

	/// <summary>Saves <paramref name="themeSettings"/> to the user themes repo, and optionally also to the default themes repo.</summary>
	public void Add(Call call, AvaloniaThemeSettings themeSettings, bool isDefault = false)
	{
		DataRepoThemes.Save(call, themeSettings);

		if (isDefault)
		{
			DataRepoDefaultThemes.Save(call, themeSettings);
		}
	}

	/// <summary>Creates the singleton <see cref="ThemeManager"/>, ensures Light, Dark, and Hybrid themes exist, and applies the current user theme.</summary>
	public static void Initialize(Project project)
	{
		Instance = new ThemeManager(project);

		Call call = new();
		Instance.AddThemeVariant(call, "Light");
		Instance.AddThemeVariant(call, "Dark");

		Instance.AddJson(call, AvaloniaAssets.Themes.Hybrid, true);

		Instance.LoadCurrentTheme();
	}

	/// <summary>Creates a named theme variant from the settings, registers it with the application, and switches the application to use it.</summary>
	public static void LoadTheme(AvaloniaThemeSettings themeSettings)
	{
		CurrentTheme = themeSettings;
		var themeVariant = new ThemeVariant(themeSettings.Name!, themeSettings.GetVariant());

		Application.Resources.ThemeDictionaries[themeVariant] = themeSettings.CreateDictionary();

		Application.RequestedThemeVariant = null;
		Application.RequestedThemeVariant = themeVariant;
	}

	/// <summary>Resets <paramref name="themeSettings"/> to the saved default values (or the base variant if no default was stored) and re-applies the theme.</summary>
	public static AvaloniaThemeSettings Reset(AvaloniaThemeSettings themeSettings)
	{
		if (Instance!.DataRepoDefaultThemes.Items.TryGetValue(themeSettings.Name!, out AvaloniaThemeSettings? defaultSettings))
		{
			themeSettings.Update(defaultSettings);
		}
		else
		{
			// Resets to Light / Dark variant
			Application.RequestedThemeVariant = themeSettings.GetVariant();
			themeSettings.LoadFromCurrent();
		}
		LoadTheme(themeSettings);

		return themeSettings;
	}

	private static JsonSerializerOptions CreateJsonSerializerOptions()
	{
		var options = new JsonSerializerOptions { WriteIndented = true };
		options.Converters.Add(new JsonColorConverter());
		return options;
	}
}
