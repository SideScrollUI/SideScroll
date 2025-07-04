using SideScroll.Resources;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Toolbar;

public class ToolButton
{
	public string Tooltip { get; set; }
	public string? Label { get; set; }

	public IResourceView ImageResource { get; set; }

	public bool ShowTask { get; set; }
	public bool Default { get; set; } // Use Enter as HotKey, add more complex keymapping later?

	public object? HotKey { get; set; } // Only AvaloniaUI KeyGesture currently supported

	public PropertyBinding? IsEnabledBinding { get; set; }

	public IFlyoutConfig? Flyout { get; set; }

	public CallAction? Action { get; set; }
	public CallActionAsync? ActionAsync { get; set; }

	public ToolButton(string tooltip, IResourceView imageResource, CallAction? action = null, bool isDefault = false, bool showTask = false)
	{
		Tooltip = tooltip;
		ImageResource = imageResource;
		Action = action;
		Default = isDefault;
		ShowTask = showTask;
	}

	public ToolButton(string tooltip, IResourceView imageResource, CallActionAsync? actionAsync, bool isDefault = false, bool showTask = false)
	{
		Tooltip = tooltip;
		ImageResource = imageResource;
		ActionAsync = actionAsync;
		Default = isDefault;
		ShowTask = showTask;
	}
}

public class PropertyBinding(string path, object? obj)
{
	public string Path => path;
	public object? Object => obj;
}
