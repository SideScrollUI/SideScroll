using Atlas.Core.Tasks;
using Atlas.Resources;

namespace Atlas.Tabs.Toolbar;

public class ToolButton
{
	public string Tooltip { get; set; }
	public string? Label { get; set; }
	public IResourceView ImageResource { get; set; }
	public bool ShowTask { get; set; }
	public bool Default { get; set; } // Use Enter as HotKey, add more complex keymapping later?
	public object? HotKey { get; set; } // Only AvaloniaUI KeyGesture currently supported

	public CallAction? Action { get; set; }
	public CallActionAsync? ActionAsync { get; set; }

	public ToolButton(string tooltip, IResourceView imageResource, CallAction? action = null, bool isDefault = false)
	{
		Tooltip = tooltip;
		ImageResource = imageResource;
		Action = action;
		Default = isDefault;
	}

	public ToolButton(string tooltip, IResourceView imageResource, CallActionAsync? actionAsync, bool isDefault = false)
	{
		Tooltip = tooltip;
		ImageResource = imageResource;
		ActionAsync = actionAsync;
		Default = isDefault;
	}
}
