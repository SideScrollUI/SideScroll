using Avalonia.Controls;
using Avalonia.Input;
using SideScroll.Attributes;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Samples.Controls;
using SideScroll.Avalonia.Samples.Controls.CustomControl;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Samples.Actions;
using SideScroll.Tabs.Samples.Chart;
using SideScroll.Tabs.Samples.DataGrid;
using SideScroll.Tabs.Samples.Objects;
using SideScroll.Tabs.Samples.Params;
using SideScroll.Tabs.Toolbar;
using System.Collections;

namespace SideScroll.Avalonia.Themes.Tabs;

public class TabAvaloniaThemeSection(TabAvaloniaThemeSettings.Instance tabInstance, object obj) : ITab
{
	public TabAvaloniaThemeSettings.Instance TabInstance = tabInstance;
	public object Object = obj;

	public override string? ToString() => Object?.ToString();

	public TabInstance Create() => new Instance(this);

	public class Toolbar(Instance tabInstance) : TabToolbar
	{
		public ToolButton ButtonRefresh { get; set; } = new("Refresh", Icons.Svg.Refresh)
		{
			Action = (_) => tabInstance.Reload()
		};

		[Separator]
		public ToolButton ButtonUndo { get; set; } = new("Undo (Ctrl + Z)", Icons.Svg.Undo)
		{
			Action = tabInstance.Tab.TabInstance.Undo,
			HotKey = new KeyGesture(Key.Z, KeyModifiers.Control),
			IsEnabledBinding = new PropertyBinding(nameof(ThemeHistory.HasPrevious), tabInstance.Tab.TabInstance.History),
		};

		public ToolButton ButtonRedo { get; set; } = new("Redo (Ctrl + Y)", Icons.Svg.Redo)
		{
			Action = tabInstance.Tab.TabInstance.Redo,
			HotKey = new KeyGesture(Key.Y, KeyModifiers.Control),
			IsEnabledBinding = new PropertyBinding(nameof(ThemeHistory.HasNext), tabInstance.Tab.TabInstance.History),
		};
	}

	public class Instance(TabAvaloniaThemeSection tab) : TabInstance, ITabSelector
	{
		public TabAvaloniaThemeSection Tab => tab;
		public new IList? SelectedItems { get; set; }

		public new event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;

		public override void Load(Call call, TabModel model)
		{
			model.CustomSettingsPath = tab.ToString();
		}

		public override void LoadUI(Call call, TabModel model)
		{
			Toolbar toolbar = new(this);
			model.AddObject(toolbar);

			var objectEditor = new TabObjectEditor(tab.Object);
			model.AddObject(objectEditor, true, true);

			foreach (var control in objectEditor.ContainerGrid.Children)
			{
				if (control is ColorPicker colorPicker)
				{
					// Avalonia could probably use Diagonal Corner Placements?
					// colorPicker.Resources.Add("ColorPickerFlyoutPlacement", PlacementMode.LeftEdgeAlignedBottom);

					colorPicker.ColorChanged += ColorPicker_ColorChanged;
					colorPicker.LostFocus += ColorPicker_LostFocus;
				}
			}

			if (GetSamples() is object obj)
			{
				SelectedItems = new List<ListItem>
				{
					new("Samples", obj)
				};
			}
		}

		private object? GetSamples()
		{
			// Using Lists provides useful spacing so drop down's don't appear on top of sample
			return tab.Object switch
			{
				FontTheme => new List<ListItem>
				{
					new("Text", TextSamples.Plain),
					new("Json", TextSamples.Json),
					new("Xml", TextSamples.Xml),
				},
				TabTheme => new List<ListItem>
				{
					new("Forms", new TabSampleParamsDataTabs()),
					new("Buttons", new TabSampleGridHashSet()),
					new("Loading", new TabSampleLoadAsync()),
				},
				ToolbarTheme => new TabCustomControl(),
				ToolTipTheme => new TabAvaloniaToolTipSample(),
				ScrollBarTheme => new TabSampleGridCollectionSize(),
				DataGridTheme => new List<ListItem>
				{
					new("Collections", new TabSampleDataGrid()),
					new("Objects", new TabSampleObjects()),
				},
				ButtonTheme => new List<ListItem>
				{
					new("Collections", new TabSampleGridCollectionSize()),
					new("Actions", new TabSampleActions()),
					new("Popups", new TabSampleFlyout()),
				},
				TextControlTheme => new TabSampleParamsDataTabs(),
				TextAreaTheme => new TabSampleTextArea(),
				TextEditorTheme => new List<ListItem>
				{
					new("Text", TextSamples.Plain),
					new("Json", TextSamples.Json),
					new("Xml", TextSamples.Xml),
				},
				ChartTheme => new TabSampleCharts(),
				_ => null
			};
		}

		private void ColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
		{
			tab.TabInstance.ColorPicker_ColorChanged(sender, e);
		}

		// Focus is lost when opening the ColorPicker
		private void ColorPicker_LostFocus(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			tab.TabInstance.ColorPicker_LostFocus(sender, e);
		}
	}
}
