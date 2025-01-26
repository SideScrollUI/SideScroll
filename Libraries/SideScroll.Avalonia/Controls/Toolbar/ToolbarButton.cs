using Avalonia.Input;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;
using System.Windows.Input;

namespace SideScroll.Avalonia.Controls.Toolbar;

public class ToolbarButton : TabControlImageButton
{
	public TabControlToolbar Toolbar { get; init; }

	public ToolbarButton(TabControlToolbar toolbar, ToolButton toolButton) :
		base(toolButton.Tooltip, toolButton.ImageResource, toolButton.Label)
	{
		TabInstance = toolbar.TabInstance;
		Toolbar = toolbar;
		ShowTask = toolButton.ShowTask;

		CallAction = toolButton.Action;
		CallActionAsync = toolButton.ActionAsync;

		if (toolButton.Default)
		{
			SetDefault();
		}

		if (toolButton.HotKey is KeyGesture keyGesture)
		{
			HotKey = keyGesture;
		}
	}

	public ToolbarButton(TabControlToolbar toolbar, string tooltip, IResourceView imageResource, double? iconSize = null, string? label = null, ICommand? command = null) :
		base(tooltip, imageResource, label, iconSize, command)
	{
		TabInstance = toolbar.TabInstance;
		Toolbar = toolbar;
	}
}
