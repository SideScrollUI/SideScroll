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

	public Project Project { get; init; }

	public List<string> Names => DataRepoThemes.Items
		.Select(i => i.Value.Name!)
		.ToList();

	public DataRepoView<AvaloniaThemeSettings> DataRepoDefaultThemes { get; protected set; }

	public DataRepoView<AvaloniaThemeSettings> DataRepoThemes { get; protected set; }

	public ThemeManager(Project project)
	{
		Project = project;

		DataRepoDefaultThemes = Project.DataApp.LoadView<AvaloniaThemeSettings>(new(), DefaultGroupId, nameof(AvaloniaThemeSettings.Name));

		DataRepoThemes = Project.DataApp.LoadView<AvaloniaThemeSettings>(new(), GroupId, nameof(AvaloniaThemeSettings.Name));
		foreach (AvaloniaThemeSettings theme in DataRepoThemes.Items.Values)
		{
			UpdateTheme(theme);
		}
		UserSettings.Themes = DataRepoThemes.Items;
	}

	public AvaloniaThemeSettings? GetTheme(string? themeName)
	{
		if (themeName == null) return null;

		var theme = DataRepoThemes.Items.Values.FirstOrDefault(theme => theme.Name == themeName);
		UpdateTheme(theme);
		return theme;
	}

	public void UpdateTheme(AvaloniaThemeSettings? themeSettings)
	{
		if (themeSettings?.HasNullValue() == true)
		{
			themeSettings.FillMissingValues();
			DataRepoThemes.Save(null, themeSettings);
		}
	}

	public void LoadCurrentTheme()
	{
		var theme = GetTheme(Project.UserSettings.Theme);
		if (theme == null) return;

		LoadTheme(theme);
	}

	public void AddDefaultTheme(string variant)
	{
		if (GetTheme(variant) != null) return;

		Add(new Call(), new AvaloniaThemeSettings
		{
			Name = variant,
			Variant = variant,
		});
	}

	public void Add(Call call, string json, bool isDefault = false)
	{
		var options = new JsonSerializerOptions();
		options.Converters.Add(new JsonColorConverter());
		var themeSettings = JsonSerializer.Deserialize<AvaloniaThemeSettings>(json, options)!;
		UpdateTheme(themeSettings);
		DataRepoThemes.Save(call, themeSettings);

		if (isDefault)
		{
			DataRepoDefaultThemes.Save(call, themeSettings);
		}
	}

	public void Add(Call call, AvaloniaThemeSettings themeSettings)
	{
		// Fill in new colors before saving
		var original = Application.Current!.RequestedThemeVariant;
		Application.Current.RequestedThemeVariant = themeSettings.GetVariant();
		themeSettings.LoadFromCurrent();
		Application.Current.RequestedThemeVariant = original;

		DataRepoThemes.Save(call, themeSettings);
	}

	public static void Initialize(Project project)
	{
		Instance = new ThemeManager(project);

		Instance.AddDefaultTheme("Light");
		Instance.AddDefaultTheme("Dark");

		Instance.Add(new(), AvaloniaAssets.Themes.LightBlue, true);

		Instance.LoadCurrentTheme();
	}

	public static void LoadTheme(AvaloniaThemeSettings themeSettings)
	{
		CurrentTheme = themeSettings;
		var themeVariant = new ThemeVariant(themeSettings.Name!, themeSettings.GetVariant());

		Application.Current!.Resources.ThemeDictionaries[themeVariant] = themeSettings.CreateDictionary();

		Application.Current.RequestedThemeVariant = null;
		Application.Current.RequestedThemeVariant = themeVariant;
	}

	public static AvaloniaThemeSettings Reset(AvaloniaThemeSettings themeSettings)
	{
		if (Instance!.DataRepoDefaultThemes.Items.TryGetValue(themeSettings.Name!, out AvaloniaThemeSettings? defaultSettings))
		{
			themeSettings = defaultSettings;
		}
		else
		{
			// Resets to Light / Dark variant
			Application.Current!.RequestedThemeVariant = themeSettings.GetVariant();
			themeSettings.LoadFromCurrent();
		}
		LoadTheme(themeSettings);

		return themeSettings;
	}
}
