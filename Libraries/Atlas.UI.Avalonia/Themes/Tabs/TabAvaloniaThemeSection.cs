using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;
using Atlas.Tabs.Test.Actions;
using Atlas.Tabs.Test.Chart;
using Atlas.Tabs.Test.DataGrid;
using Atlas.Tabs.Test.Objects;
using Atlas.Tabs.Test.Params;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.Samples.Controls.CustomControl;
using Avalonia.Controls;
using Avalonia.Input;
using System.Collections;

namespace Atlas.UI.Avalonia.Themes.Tabs;

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
			model.AddObject(paramControl);

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
					new ListItem("Samples", obj)
				};
			}
		}

		private object? GetSamples()
		{
			return tab.Object switch
			{
				TabTheme => new List<ListItem>
				{
					new("Forms", new TabTestParamsDataTabs()),
					new("Loading", new TabTestLoadAsync()),
				},
				ToolbarTheme => new TabCustomControl(),
				ToolTipTheme => new TabAvaloniaToolTipSample(),
				DataGridTheme => new List<ListItem>
				{
					new("Collections", new TabTestDataGrid()),
					new("Objects", new TabTestObjects()),
				},
				ButtonTheme => new TabActions(),
				TextControlTheme => new TabTestParamsDataTabs(),
				TextEditorTheme => new List<ListItem>
				{
					new("Text", Resources.Samples.Text.Plain),
					new("Json", Resources.Samples.Text.Json),
					new("Xml", Resources.Samples.Text.Xml),
				},
				ChartTheme => new TabTestChart(),
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
