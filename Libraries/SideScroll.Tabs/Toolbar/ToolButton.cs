using SideScroll.Resources;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Toolbar;

/// <summary>
/// Represents a toolbar button with associated action, icon, and configuration options
/// </summary>
public class ToolButton
{
	/// <summary>
	/// Gets the tooltip text displayed when hovering over the button
	/// </summary>
	public string Tooltip { get; }

	/// <summary>
	/// Gets or sets the optional text label displayed on the button
	/// </summary>
	public string? Label { get; set; }

	/// <summary>
	/// Gets the image resource to display as the button icon
	/// </summary>
	public IResourceView ImageResource { get; }

	/// <summary>
	/// Gets or sets whether task progress should be displayed when the action executes
	/// </summary>
	public bool ShowTask { get; set; }

	/// <summary>
	/// Gets or sets whether the action should execute on a background thread
	/// </summary>
	public bool UseBackgroundThread { get; set; }

	/// <summary>
	/// Gets or sets whether this is the default button
	/// When true, the button can be activated using the Enter key
	/// </summary>
	public bool IsDefault { get; set; }

	/// <summary>
	/// Gets or sets the keyboard shortcut for this button
	/// Currently only AvaloniaUI KeyGesture is supported
	/// </summary>
	public object? HotKey { get; set; }

	/// <summary>
	/// Gets or sets the property binding that controls whether the button is enabled
	/// </summary>
	public PropertyBinding? IsEnabledBinding { get; set; }

	/// <summary>
	/// Gets or sets the flyout configuration for displaying a popup when the button is clicked
	/// </summary>
	public IFlyoutConfig? Flyout { get; set; }

	/// <summary>
	/// Gets or sets the synchronous action to execute when the button is clicked
	/// </summary>
	public CallAction? Action { get; set; }

	/// <summary>
	/// Gets or sets the asynchronous action to execute when the button is clicked
	/// </summary>
	public CallActionAsync? ActionAsync { get; set; }

	public override string ToString() => Tooltip;

	/// <summary>
	/// Initializes a new toolbar button with a synchronous action
	/// </summary>
	/// <param name="tooltip">The tooltip text to display on hover</param>
	/// <param name="imageResource">The image resource for the button icon</param>
	/// <param name="action">The synchronous action to execute when clicked</param>
	/// <param name="isDefault">Whether this is the default button (activated with Enter key)</param>
	/// <param name="showTask">Whether to show task progress during execution</param>
	/// <param name="backgroundThread">Whether to execute the action on a background thread</param>
	public ToolButton(string tooltip, IResourceView imageResource, CallAction? action = null, bool isDefault = false, bool showTask = false, bool backgroundThread = false)
	{
		Tooltip = tooltip;
		ImageResource = imageResource;
		Action = action;
		IsDefault = isDefault;
		ShowTask = showTask;
		UseBackgroundThread = backgroundThread;
	}

	/// <summary>
	/// Initializes a new toolbar button with an asynchronous action
	/// </summary>
	/// <param name="tooltip">The tooltip text to display on hover</param>
	/// <param name="imageResource">The image resource for the button icon</param>
	/// <param name="actionAsync">The asynchronous action to execute when clicked</param>
	/// <param name="isDefault">Whether this is the default button (activated with Enter key)</param>
	/// <param name="showTask">Whether to show task progress during execution</param>
	/// <param name="backgroundThread">Whether to execute the action on a background thread</param>
	public ToolButton(string tooltip, IResourceView imageResource, CallActionAsync? actionAsync, bool isDefault = false, bool showTask = false, bool backgroundThread = false)
	{
		Tooltip = tooltip;
		ImageResource = imageResource;
		ActionAsync = actionAsync;
		IsDefault = isDefault;
		ShowTask = showTask;
		UseBackgroundThread = backgroundThread;
	}
}

/// <summary>
/// Represents a property binding configuration for controlling UI element properties
/// </summary>
public class PropertyBinding(string path, object? obj)
{
	/// <summary>
	/// Gets the property path to bind to
	/// </summary>
	public string Path => path;

	/// <summary>
	/// Gets the object containing the property to bind to
	/// </summary>
	public object? Object => obj;
}
