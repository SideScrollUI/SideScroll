using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;
using SideScroll.UI.Avalonia.Controls;
using SideScroll.UI.Avalonia.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SideScroll.UI.Avalonia.Themes.Tabs;

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

	public void Load(object sender, object obj, object[] tabParams)
	{
		DataViewCollection = (DataViewCollection<AvaloniaThemeSettings, TabAvaloniaThemeSettings>)sender;
		ThemeSettings = (AvaloniaThemeSettings)obj;
	}

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonReset { get; set; } = new("Reset", Icons.Svg.Reset);

		[Separator]
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save);

		[Separator]
		public ToolButton ButtonCopy { get; set; } = new("Copy to Clipboard", Icons.Svg.Copy);
		public ToolButton ButtonImport { get; set; } = new("Import from Clipboard", Icons.Svg.Import);
	}

	public class Instance(TabAvaloniaThemeSettings tab) : TabInstance
	{
		public AvaloniaThemeSettings ThemeSettings = tab.ThemeSettings.DeepClone()!;

		private TabControlParams? _paramControl;

		private ThemeHistory _history = new();
		private bool _replaceLastHistory;

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
			_paramControl = new TabControlParams(ThemeSettings, false);
			_paramControl.AddPropertyControl(nameof(ThemeSettings.Name));
			_paramControl.AddPropertyControl(nameof(ThemeSettings.Variant));
			model.AddObject(_paramControl);

			var sectionTabs = ThemeSettings.GetSections()
				.Select(obj => new TabAvaloniaThemeSection(this, obj))
				.ToList();
			model.AddData(sectionTabs);

			_history.Add(ThemeSettings);
		}

		public void Undo(Call call)
		{
			_replaceLastHistory = false;
			if (_history.TryGetPrevious(out var previous))
			{
				LoadTheme(previous);
			}
		}

		public void Redo(Call call)
		{
			if (_history.TryGetNext(out var next))
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
			_replaceLastHistory = false;
		}

		public void ColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
		{
			if (_ignoreColorChange) return;

			Dispatcher.UIThread.Post(AddColorChange);
		}

		private void AddColorChange()
		{
			UpdateTheme();
			AddHistory();
		}

		private void Reset(Call call)
		{
			Application.Current!.RequestedThemeVariant = ThemeSettings.GetVariant();
			ThemeSettings.LoadFromCurrent();
			ThemeManager.LoadTheme(ThemeSettings);
			Reload();
		}

		private void Save(Call call)
		{
			tab.DataViewCollection!.DataRepoView.Save(call, ThemeSettings);
			UpdateTheme();
		}

		private void UpdateTheme()
		{
			ThemeManager.LoadTheme(ThemeSettings);
		}

		private void AddHistory()
		{
			if (_ignoreColorChange) return;

			if (_replaceLastHistory)
			{
				_history.Replace(ThemeSettings);
			}
			else
			{
				_history.Add(ThemeSettings);
				_replaceLastHistory = true;
			}
		}

		private void CopyToClipboard(Call call)
		{
			var options = new JsonSerializerOptions { WriteIndented = true };
			options.Converters.Add(new JsonColorConverter());
			string json = JsonSerializer.Serialize(ThemeSettings, options);
			ClipboardUtils.SetText(_paramControl, json);
		}

		private void ImportFromClipboard(Call call)
		{
			string json = ClipboardUtils.GetText(_paramControl)!;

			var options = new JsonSerializerOptions();
			options.Converters.Add(new JsonColorConverter());
			var theme = JsonSerializer.Deserialize<AvaloniaThemeSettings>(json, options)!;
			LoadTheme(theme);
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
