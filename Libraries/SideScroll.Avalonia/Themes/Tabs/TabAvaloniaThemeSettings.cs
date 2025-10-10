using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SideScroll.Attributes;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Utilities;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SideScroll.Avalonia.Themes.Tabs;

public class TabAvaloniaThemeSettings : ITab, IDataView
{
	[DataValue]
	public AvaloniaThemeSettings? ThemeSettings;

	public DataViewCollection<AvaloniaThemeSettings, TabAvaloniaThemeSettings>? DataViewCollection;

	[DataKey]
	public string? Name => ThemeSettings?.Name;

	public event EventHandler<EventArgs>? OnDelete;

	[ButtonColumn("-")]
	public void Delete()
	{
		OnDelete?.Invoke(this, EventArgs.Empty);
	}

	public override string? ToString() => ThemeSettings?.ToString();

	public TabAvaloniaThemeSettings() { }

	public TabAvaloniaThemeSettings(AvaloniaThemeSettings themeSettings)
	{
		ThemeSettings = themeSettings;
	}

	public void Load(object sender, object obj, object?[] tabParams)
	{
		DataViewCollection = (DataViewCollection<AvaloniaThemeSettings, TabAvaloniaThemeSettings>)sender;
		ThemeSettings = (AvaloniaThemeSettings)obj;
	}

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonReset { get; set; } = new("Reset", Icons.Svg.Reset)
		{
			Flyout = new ConfirmationFlyoutConfig("Are you sure you want to reset this theme?", "Reset"),
		};

		[Separator]
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save, isDefault: true);

		[Separator]
		public ToolButton ButtonCopy { get; set; } = new("Copy to Clipboard", Icons.Svg.Copy);
		public ToolButton ButtonImport { get; set; } = new("Import from Clipboard", Icons.Svg.Import);
	}

	public class Instance(TabAvaloniaThemeSettings tab) : TabInstance
	{
		public AvaloniaThemeSettings ThemeSettings = tab.ThemeSettings!.DeepClone();

		private TabForm? _themeForm;

		public ThemeHistory History { get; protected set; } = new();
		private bool _lastHistoryUpdatable;

		private bool _ignoreColorChange;

		public override void Load(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonReset.Action = Reset;
			toolbar.ButtonSave.Action = Save;
			toolbar.ButtonCopy.Action = CopyToClipboard;
			toolbar.ButtonImport.Action = ImportFromClipboard;
			model.AddObject(toolbar);
		}

		public override void LoadUI(Call call, TabModel model)
		{
			_themeForm = new TabForm(ThemeSettings, false);
			_themeForm.AddPropertyControl(nameof(ThemeSettings.Name));
			_themeForm.AddPropertyControl(nameof(ThemeSettings.Variant));
			model.AddObject(_themeForm);

			var sectionTabs = ThemeSettings.GetSections()
				.Select(obj => new TabAvaloniaThemeSection(this, obj))
				.ToList();
			model.AddData(sectionTabs);

			History.Add(ThemeSettings);
		}

		public void Undo(Call call)
		{
			_lastHistoryUpdatable = false;
			if (History.TryGetPrevious(out var previous))
			{
				LoadTheme(previous);
			}
		}

		public void Redo(Call call)
		{
			if (History.TryGetNext(out var next))
			{
				LoadTheme(next);
			}
		}

		private void LoadTheme(AvaloniaThemeSettings newSettings)
		{
			// Wait until finished to update
			_ignoreColorChange = true;
			ThemeSettings.Update(newSettings);
			_ignoreColorChange = false;

			UpdateTheme();
		}

		// Focus is lost when opening the ColorPicker
		public void ColorPicker_LostFocus(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			_lastHistoryUpdatable = false;
		}

		public void ColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
		{
			if (_ignoreColorChange) return;

			bool isColorPickerOpen = sender is ColorPicker colorPicker &&
				colorPicker.GetVisualChildren().FirstOrDefault() is DropDownButton button &&
				button.Flyout?.IsOpen == true;

			Dispatcher.UIThread.Post(() => AddColorChange(isColorPickerOpen));
		}

		private void AddColorChange(bool isColorPickerOpen)
		{
			UpdateTheme();
			AddHistory(isColorPickerOpen);
		}

		private void Reset(Call call)
		{
			ThemeSettings = ThemeManager.Reset(ThemeSettings);
			Reload();
		}

		private void Save(Call call)
		{
			Validate();

			ThemeSettings.ModifiedAt = DateTime.Now;
			tab.DataViewCollection!.DataRepoView.Save(call, ThemeSettings);
			UpdateTheme();

			call.TaskInstance!.ShowMessage("Saved");
		}

		private void UpdateTheme()
		{
			ThemeManager.LoadTheme(ThemeSettings);
		}

		private void AddHistory(bool isUpdatable)
		{
			if (_ignoreColorChange) return;

			if (_lastHistoryUpdatable && isUpdatable)
			{
				History.Replace(ThemeSettings);
			}
			else
			{
				History.Add(ThemeSettings);
			}
			_lastHistoryUpdatable = isUpdatable;
		}

		private static JsonSerializerOptions CreateJsonSerializerOptions()
		{
			var options = new JsonSerializerOptions { WriteIndented = true };
			options.Converters.Add(new JsonColorConverter());
			return options;
		}

		private static JsonSerializerOptions _jsonSerializerOptions = CreateJsonSerializerOptions();

		private void CopyToClipboard(Call call)
		{
			string json = JsonSerializer.Serialize(ThemeSettings, _jsonSerializerOptions);
			CopyToClipboard(json);
			call.TaskInstance!.ShowMessage("Copied to Clipboard");
		}

		private void ImportFromClipboard(Call call)
		{
			Dispatcher.UIThread.Post(async () => await ImportFromClipboardAsync(call));
		}

		private async Task ImportFromClipboardAsync(Call call)
		{
			try
			{
				string? json = await ClipboardUtils.TryGetTextAsync(_themeForm)!;
				if (json == null)
				{
					call.TaskInstance!.ShowMessage("No clipboard content found");
					return;
				}
				var theme = JsonSerializer.Deserialize<AvaloniaThemeSettings>(json, _jsonSerializerOptions)!;
				LoadTheme(theme);

				call.TaskInstance!.ShowMessage("Imported Theme");
			}
			catch (Exception e)
			{
				call.TaskInstance!.ShowMessage("Import Failed: " + e.Message);
			}
		}
	}
}

public class JsonColorConverter : JsonConverter<Color>
{
	public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string value = reader.GetString()!;
		return Color.Parse(value);
	}

	public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
