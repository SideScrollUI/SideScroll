using Avalonia;
using SideScroll.Attributes;
using SideScroll.Avalonia.Controls;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.Avalonia.Themes.Tabs;

public class TabAvaloniaThemes : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar(Instance instance) : TabToolbar
	{
		public ToolButton ButtonNew { get; set; } = new("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save)
		{
			IsEnabledBinding = new PropertyBinding(nameof(ThemeId.HasName), instance.ThemeId),
		};
	}

	public class Instance : TabInstance
	{
		public ThemeId ThemeId { get; protected set; } = new();
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
			ThemeId.Reset();
			_themeParams = new TabControlParams(ThemeId);
			model.AddObject(_themeParams);

			var toolbar = new Toolbar(this);
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoThemes = ThemeManager.Instance!.DataRepoThemes;
			DataRepoInstance = _dataRepoThemes;

			var dataCollection = new DataViewCollection<AvaloniaThemeSettings, TabAvaloniaThemeSettings>(_dataRepoThemes);
			//dataCollection.DataRepoSecondary = DataShared.LoadView<AvaloniaThemeSettings>(call, "Themes", nameof(AvaloniaThemeSettings.Name));
			model.Items = dataCollection.Items;
		}

		private void Save(Call call)
		{
			Validate();

			AvaloniaThemeSettings themeSettings = ThemeManager.Instance!.Create(ThemeId.Name!, ThemeId.Variant!);

			ThemeManager.Instance!.Add(call, themeSettings);

			ThemeId.Reset();
			_themeParams!.LoadObject(ThemeId);
		}
	}

	[Params]
	public class ThemeId : AvaloniaObject
	{
		[Required, StringLength(50)]
		public string? Name
		{
			get => _name;
			set
			{
				_name = value;
				HasName = !_name.IsNullOrEmpty();
			}
		}
		public string? _name;

		public static List<string> Variants { get; } =
		[
			"Light",
			"Dark",
		];

		[BindList(nameof(Variants))]
		public string? Variant { get; set; }

		[Hidden]
		public bool HasName
		{
			get => GetValue(HasNameProperty);
			private set => SetValue(HasNameProperty, value);
		}

		public static readonly StyledProperty<bool> HasNameProperty =
			AvaloniaProperty.Register<ThemeHistory, bool>(nameof(HasName));

		public void Reset()
		{
			Name = null;
			Variant = Variants.First();
		}
	}
}
