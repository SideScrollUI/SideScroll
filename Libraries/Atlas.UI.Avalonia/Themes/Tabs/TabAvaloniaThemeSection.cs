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

public class TabAvaloniaThemeSection : ITab
{
	public TabAvaloniaThemeSettings.Instance TabInstance;
	public object Object;

	public override string? ToString() => Object?.ToString();

	public TabAvaloniaThemeSection(TabAvaloniaThemeSettings.Instance tabInstance, object obj)
	{
		TabInstance = tabInstance;
		Object = obj;
	}

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
		public TabAvaloniaThemeSection Tab = tab;
		public new IList? SelectedItems { get; set; }

		public new event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;

		public override void LoadUI(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Refresh;
			toolbar.ButtonUndo.Action = Tab.TabInstance.Undo;
			toolbar.ButtonRedo.Action = Tab.TabInstance.Redo;
			model.AddObject(toolbar);

			var paramControl = new TabControlParams(Tab.Object);
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
				SelectedItems = new List<ListItem>() { new ListItem("Samples", obj) };
			}
		}

		private object? GetSamples()
		{
			switch (Tab.Object)
			{
				case TabTheme:
				{
					return new List<ListItem>()
					{
						new ListItem("Forms", new TabTestParamsDataTabs()),
						new ListItem("Loading", new TabTestLoadAsync()),
					};
				}
				case ToolbarTheme:
				{
					return new TabCustomControl();
				}
				case ToolTipTheme:
				{
					return new TabAvaloniaToolTipSample();
				}
				case DataGridTheme:
				{
					return new List<ListItem>()
					{
						new ListItem("Collections", new TabTestDataGrid()),
						new ListItem("Objects", new TabTestObjects()),
					};
				}
				case ButtonTheme:
				{
					return new TabActions();
				}
				case TextControlTheme:
				{
					return new TabTestParamsDataTabs();
				}
				case TextEditorTheme:
				{
					return new List<ListItem>()
					{
						new ListItem("Text", Resources.Samples.Text.Plain),
						new ListItem("Json", Resources.Samples.Text.Json),
						new ListItem("Xml", Resources.Samples.Text.Xml),
					};
				}
				case ChartTheme:
				{
					return new TabTestChart();
				}
				default:
				{
					return null;
				}
			}
		}

		private void Refresh(Call call)
		{
			Reload();
		}

		// Focus is lost when opening the ColorPicker
		private void ColorPicker_LostFocus(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Tab.TabInstance.ColorPicker_LostFocus(sender, e);
		}

		private void ColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
		{
			Tab.TabInstance.ColorPicker_ColorChanged(sender, e);
		}
	}
}
