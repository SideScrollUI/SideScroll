using Avalonia.Input;
using SideScroll.Avalonia.Controls.Flyouts;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Controls.Toolbar;

public class ToolbarButton : TabImageButton
{
	public TabControlToolbar Toolbar { get; }

	public ToolbarButton(TabControlToolbar toolbar, ToolButton toolButton) :
		base(toolButton.Tooltip, toolButton.ImageResource, toolButton.Label)
	{
		TabInstance = toolbar.TabInstance;
		Toolbar = toolbar;
		ShowTask = toolButton.ShowTask;

		CallAction = toolButton.Action;
		CallActionAsync = toolButton.ActionAsync;
		UseUIThread = toolButton.UseUIThread;

		if (toolButton.IsDefault)
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
			Flyout = new ConfirmationFlyout(async () => await InvokeTaskAsync(), config.Text, config.ConfirmText, config.CancelText);
		}
	}

	public ToolbarButton(TabControlToolbar toolbar, string tooltip, IResourceView imageResource, double? iconSize = null, string? label = null) :
		base(tooltip, imageResource, label, iconSize)
	{
		TabInstance = toolbar.TabInstance;
		Toolbar = toolbar;
	}
}
