using Avalonia.Controls;
using Avalonia.Input;
using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Samples;
using SideScroll.Tabs.Samples.Actions;
using SideScroll.Tabs.Samples.Chart;
using SideScroll.Tabs.Samples.DataGrid;
using SideScroll.Tabs.Samples.Objects;
using SideScroll.Tabs.Samples.Params;
using SideScroll.Tabs.Toolbar;
using SideScroll.UI.Avalonia.Controls;
using SideScroll.UI.Avalonia.Samples.Controls;
using SideScroll.UI.Avalonia.Samples.Controls.CustomControl;

//using SideScroll.UI.Avalonia.Samples.Controls.CustomControl;
using System.Collections;

namespace SideScroll.UI.Avalonia.Themes.Tabs;

public class TabAvaloniaThemeSection(TabAvaloniaThemeSettings.Instance tabInstance, object obj) : ITab
{
	public TabAvaloniaThemeSettings.Instance TabInstance = tabInstance;
	public object Object = obj;

	public override string? ToString() => Object?.ToString();

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; set; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonUndo { get; set; } = new("Undo (Ctrl + Z)", Icons.Svg.Undo)
		{
			HotKey = new KeyGesture(Key.Z, KeyModifiers.Control),
		};
		public ToolButton ButtonRedo { get; set; } = new("Redo (Ctrl + Y)", Icons.Svg.Redo)
		{
			HotKey = new KeyGesture(Key.Y, KeyModifiers.Control),
		};
	}

	public class Instance(TabAvaloniaThemeSection tab) : TabInstance, ITabSelector
	{
		public new IList? SelectedItems { get; set; }

		public new event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;

		public override void Load(Call call, TabModel model)
		{
			model.CustomSettingsPath = tab.ToString();
		}

		public override void LoadUI(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Refresh;
			toolbar.ButtonUndo.Action = tab.TabInstance.Undo;
			toolbar.ButtonRedo.Action = tab.TabInstance.Redo;
			model.AddObject(toolbar);

			var paramControl = new TabControlParams(tab.Object);
			model.AddObject(paramControl, true, true);

			foreach (var control in paramControl.ContainerGrid.Children)
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
				//ToolbarTheme => new TabSampleToolbar(),
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

		private void Refresh(Call call)
		{
			Reload();
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
