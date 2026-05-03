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

	/// <summary>Initializes the button from a <see cref="ToolButton"/> definition, wiring up its action, hotkey, and flyout.</summary>
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

		if (toolButton.IsEnabledBinding is { } propertyBinding)
		{
			BindIsEnabled(propertyBinding.Path, propertyBinding.Object);
		}

		if (toolButton.Flyout is ConfirmationFlyoutConfig config)
		{
			Flyout = new ConfirmationFlyout(async () => await InvokeTaskAsync(), config.Text, config.ConfirmText, config.CancelText);
		}
	}

	/// <summary>Initializes the button directly with an image resource and tooltip, without a <see cref="ToolButton"/> definition.</summary>
	public ToolbarButton(TabControlToolbar toolbar, string tooltip, IResourceView imageResource, double? iconSize = null, string? label = null, bool updateIconColors = true) :
		base(tooltip, imageResource, label, iconSize, updateIconColors)
	{
		TabInstance = toolbar.TabInstance;
		Toolbar = toolbar;
	}
}
