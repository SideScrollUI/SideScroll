using Avalonia.Controls;
using Avalonia.Input;
using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Samples.Actions;
using SideScroll.Tabs.Samples.Chart;
using SideScroll.Tabs.Samples.DataGrid;
using SideScroll.Tabs.Samples.Objects;
using SideScroll.Tabs.Samples.Params;
using SideScroll.Tabs.Toolbar;
using SideScroll.UI.Avalonia.Controls;
using SideScroll.UI.Avalonia.Samples.Controls;
using SideScroll.UI.Avalonia.Samples.Controls.CustomControl;
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

		public override void LoadUI(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Refresh;
			toolbar.ButtonUndo.Action = tab.TabInstance.Undo;
			toolbar.ButtonRedo.Action = tab.TabInstance.Redo;
			model.AddObject(toolbar);

			var paramControl = new TabControlParams(tab.Object);
			model.AddObject(paramControl, true, true);

			foreach (var control in paramControl.Children)
			{
				if (control is ColorPicker colorPicker)
				{
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
			return tab.Object switch
			{
				TabTheme => new List<ListItem>
				{
					new("Forms", new TabSampleParamsDataTabs()),
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
				ButtonTheme => new TabSampleActions(),
				TextControlTheme => new TabSampleParamsDataTabs(),
				TextAreaTheme => new TabTextArea(),
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

		// Focus is lost when opening the ColorPicker
		private void ColorPicker_LostFocus(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			tab.TabInstance.ColorPicker_LostFocus(sender, e);
		}

		private void ColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
		{
			tab.TabInstance.ColorPicker_ColorChanged(sender, e);
		}
	}
}
