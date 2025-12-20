using Avalonia;
using Avalonia.Styling;
using SideScroll.Avalonia.Themes.Tabs;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using System.Text.Json;

namespace SideScroll.Avalonia.Themes;

public class ThemeManager
{
	public const string DefaultGroupId = "DefaultThemes";
	public const string GroupId = "Themes";

	public static ThemeManager? Instance { get; set; }
	public static AvaloniaThemeSettings? CurrentTheme { get; protected set; }

	public static JsonSerializerOptions JsonSerializerOptions { get; } = CreateJsonSerializerOptions();

	public Project Project { get; }

	public static Application Application => Application.Current!;

	public List<string> Names => DataRepoThemes.Items
		.Select(i => i.Value.Name!)
		.ToList();

	public DataRepoView<AvaloniaThemeSettings> DataRepoDefaultThemes { get; }

	public DataRepoView<AvaloniaThemeSettings> DataRepoThemes { get; }

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

	public AvaloniaThemeSettings? GetUpdatedTheme(string? themeName)
	{
		if (themeName == null) return null;

		var theme = DataRepoThemes.Values.FirstOrDefault(theme => theme.Name == themeName);
		UpdateTheme(theme);
		return theme;
	}

	public void UpdateTheme(AvaloniaThemeSettings? themeSettings)
	{
		if (themeSettings?.HasNullValue() == true)
		{
			themeSettings.FillMissingValues();
			themeSettings.Version = Project.Version;
			DataRepoThemes.Save(null, themeSettings);
		}
	}

	public void LoadCurrentTheme()
	{
		var theme = GetUpdatedTheme(Project.UserSettings.Theme);
		if (theme == null) return;

		LoadTheme(theme);
	}

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
			GetUpdatedTheme(variant) is not AvaloniaThemeSettings existingThemeSettings ||
			(existingThemeSettings.ModifiedAt == null && existingThemeSettings.Version != Project.Version))
		{
			DataRepoThemes.Save(call, defaultTheme);
		}
	}

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

		if (GetUpdatedTheme(themeSettings.Name) is not AvaloniaThemeSettings existingThemeSettings ||
			(existingThemeSettings.ModifiedAt == null && existingThemeSettings.Version != Project.Version))
		{
			DataRepoThemes.Save(call, themeSettings);
		}
	}

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

	public void Add(Call call, AvaloniaThemeSettings themeSettings, bool isDefault = false)
	{
		DataRepoThemes.Save(call, themeSettings);

		if (isDefault)
		{
			DataRepoDefaultThemes.Save(call, themeSettings);
		}
	}

	public static void Initialize(Project project)
	{
		Instance = new ThemeManager(project);

		Call call = new();
		Instance.AddThemeVariant(call, "Light");
		Instance.AddThemeVariant(call, "Dark");

		Instance.AddJson(call, AvaloniaAssets.Themes.Hybrid, true);

		Instance.LoadCurrentTheme();
	}

	public static void LoadTheme(AvaloniaThemeSettings themeSettings)
	{
		CurrentTheme = themeSettings;
		var themeVariant = new ThemeVariant(themeSettings.Name!, themeSettings.GetVariant());

		Application.Resources.ThemeDictionaries[themeVariant] = themeSettings.CreateDictionary();

		Application.RequestedThemeVariant = null;
		Application.RequestedThemeVariant = themeVariant;
	}

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
