using SideScroll;
using SideScroll.Resources;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;
using SideScroll.UI.Avalonia.Controls;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.UI.Avalonia.Themes.Tabs;

public class TabAvaloniaThemes : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonNew { get; set; } = new("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save);
	}

	public class Instance : TabInstance
	{
		private ThemeId _themeId = new();
		private DataRepoView<AvaloniaThemeSettings>? _dataRepoThemes;
		private TabControlParams? _themeParams;

		public override void Load(Call call, TabModel model)
		{
			model.Editing = true;
			model.MinDesiredWidth = 200;

			LoadSavedItems(call, model);
		}

		public override void LoadUI(Call call, TabModel model)
		{
			_themeId = new();
			_themeParams = new TabControlParams(_themeId);
			model.AddObject(_themeParams);

			var toolbar = new Toolbar();
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoThemes = ThemeManager.Current!.DataRepoThemes;
			DataRepoInstance = _dataRepoThemes;

			var dataCollection = new DataViewCollection<AvaloniaThemeSettings, TabAvaloniaThemeSettings>(_dataRepoThemes);
			//dataCollection.DataRepoSecondary = DataShared.LoadView<AvaloniaThemeSettings>(call, "Themes", nameof(AvaloniaThemeSettings.Name));
			model.Items = dataCollection.Items;
		}

		private void Save(Call call)
		{
			Validate();

			AvaloniaThemeSettings themeSettings = new()
			{
				Name = _themeId.Name,
				Variant = _themeId.Variant,
			};
			ThemeManager.Current!.Add(call, themeSettings);

			_themeId = new();
			_themeParams!.LoadObject(_themeId);
		}
	}

	[Params]
	public class ThemeId
	{
		[Required, StringLength(50)]
		public string? Name { get; set; }

		public static List<string> Variants =>
		[
			"Light",
			"Dark",
		];

		[BindList(nameof(Variants))]
		public string? Variant { get; set; }
	}
}
