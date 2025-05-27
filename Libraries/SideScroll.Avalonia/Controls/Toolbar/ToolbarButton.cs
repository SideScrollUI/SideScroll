using Avalonia.Input;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

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

		if (toolButton.IsEnabledBinding is PropertyBinding propertyBinding)
		{
			BindIsEnabled(propertyBinding.Path, propertyBinding.Object);
		}

		if (toolButton.Flyout is ConfirmationFlyoutConfig config)
		{
			Flyout = new ConfirmationFlyout(InvokeAction, config.Text, config.ConfirmText, config.CancelText);
		}
	}

	public ToolbarButton(TabControlToolbar toolbar, string tooltip, IResourceView imageResource, double? iconSize = null, string? label = null) :
		base(tooltip, imageResource, label, iconSize)
	{
		TabInstance = toolbar.TabInstance;
		Toolbar = toolbar;
	}
}
