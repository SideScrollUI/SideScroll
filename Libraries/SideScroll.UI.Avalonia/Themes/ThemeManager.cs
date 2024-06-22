using SideScroll;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs;
using Avalonia;
using Avalonia.Styling;

namespace SideScroll.UI.Avalonia.Themes;

public class ThemeManager
{
	public const string GroupId = "Themes";

	public static ThemeManager? Current { get; set; }
	public static AvaloniaThemeSettings? CurrentTheme { get; private set; }

	public readonly Project Project;

	public List<string> Names => DataRepoThemes.Items
		.Select(i => i.Value.Name!)
		.ToList();

	public readonly DataRepoView<AvaloniaThemeSettings> DataRepoThemes;

	public ThemeManager(Project project)
	{
		Project = project;

		DataRepoThemes = Project.DataApp.LoadView<AvaloniaThemeSettings>(new(), GroupId, nameof(AvaloniaThemeSettings.Name));
		foreach (AvaloniaThemeSettings theme in DataRepoThemes.Items.Values)
		{
			UpdateTheme(theme);
		}
		UserSettings.Themes = Names;
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

	public void AddDefaultThemes(string variant)
	{
		if (GetTheme(variant) != null) return;

		Add(new Call(), new AvaloniaThemeSettings
		{
			Name = variant,
			Variant = variant,
		});
	}

	public void Add(Call call, AvaloniaThemeSettings themeSettings)
	{
		var original = Application.Current!.RequestedThemeVariant;
		Application.Current.RequestedThemeVariant = themeSettings.GetVariant();
		themeSettings.LoadFromCurrent();
		Application.Current.RequestedThemeVariant = original;

		DataRepoThemes.Save(call, themeSettings);
		UserSettings.Themes = Names;
	}

	public static void Initialize(Project project)
	{
		Current = new ThemeManager(project);

		Current.AddDefaultThemes("Light");
		Current.AddDefaultThemes("Dark");
		Current.LoadCurrentTheme();
	}

	public static void LoadTheme(AvaloniaThemeSettings themeSettings)
	{
		CurrentTheme = themeSettings;
		var themeVariant = new ThemeVariant(themeSettings.Name!, themeSettings.GetVariant());

		Application.Current!.Resources.ThemeDictionaries[themeVariant] = themeSettings.CreateDictionary();

		Application.Current.RequestedThemeVariant = null;
		Application.Current.RequestedThemeVariant = themeVariant;
	}
}
