using Atlas.Core;
using Atlas.Resources;

namespace Atlas.Tabs;

public class ToolButton
{
	public string Tooltip { get; set; }
	public string? Label { get; set; }
	public ResourceView ImageResource { get; set; }
	public bool ShowTask { get; set; }
	public bool Default { get; set; } // Use Enter as HotKey, add more complex keymapping later?

	public TaskDelegate.CallAction? Action { get; set; }
	public TaskDelegateAsync.CallActionAsync? ActionAsync { get; set; }

	public ToolButton(string tooltip, ResourceView imageResource, TaskDelegate.CallAction? action = null, bool isDefault = false)
	{
		Tooltip = tooltip;
		ImageResource = imageResource;
		Action = action;
		Default = isDefault;
	}

	public ToolButton(string tooltip, ResourceView imageResource, TaskDelegateAsync.CallActionAsync? actionAsync, bool isDefault = false)
	{
		Tooltip = tooltip;
		ImageResource = imageResource;
		ActionAsync = actionAsync;
		Default = isDefault;
	}
}

public class TabToolbar
{
	public List<ToolButton> Buttons { get; set; } = new();
}
