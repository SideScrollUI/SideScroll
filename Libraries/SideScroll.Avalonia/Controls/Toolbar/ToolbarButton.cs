using Avalonia.Input;
using SideScroll.Avalonia.Controls.Flyouts;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Controls.Toolbar;

/// <summary>
/// An icon button for use in a <see cref="TabControlToolbar"/> that dispatches synchronous or async actions via a <see cref="TaskInstance"/>.
/// </summary>
public class ToolbarButton : TabImageButton
{
	/// <summary>Gets the toolbar that owns this button.</summary>
	public TabControlToolbar Toolbar { get; }

	public ToolbarButton(TabControlToolbar toolbar, ToolButton toolButton) :
		base(toolButton.Tooltip, toolButton.ImageResource, toolButton.Label)
	{
		TabInstance = toolbar.TabInstance;
		Toolbar = toolbar;
		ShowTask = toolButton.ShowTask;
		UseBackgroundThread = toolButton.UseBackgroundThread;

		CallAction = toolButton.Action;
		CallActionAsync = toolButton.ActionAsync;

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

	public ToolbarButton(TabControlToolbar toolbar, string tooltip, IResourceView imageResource, double? iconSize = null, string? label = null, bool updateIconColors = true) :
		base(tooltip, imageResource, label, iconSize, updateIconColors)
	{
		TabInstance = toolbar.TabInstance;
		Toolbar = toolbar;
	}
}
