using Atlas.Core;

namespace Atlas.Tabs;

public class ToolButton
{
	public string Tooltip { get; set; }
	public string? Label { get; set; }
	public string ImageResourceName { get; set; }
	public bool ShowTask { get; set; }
	public bool Default { get; set; } // Use Enter as HotKey, add more complex keymapping later?

	public TaskDelegate.CallAction? Action { get; set; }
	public TaskDelegateAsync.CallActionAsync? ActionAsync { get; set; }

	public ToolButton(string tooltip, string imageResourceName, TaskDelegate.CallAction? action = null, bool isDefault = false)
	{
		Tooltip = tooltip;
		ImageResourceName = imageResourceName;
		Action = action;
		Default = isDefault;
	}

	public ToolButton(string tooltip, string imageResourceName, TaskDelegateAsync.CallActionAsync? actionAsync, bool isDefault = false)
	{
		Tooltip = tooltip;
		ImageResourceName = imageResourceName;
		ActionAsync = actionAsync;
		Default = isDefault;
	}
}

public class TabToolbar
{
	public List<ToolButton> Buttons { get; set; } = new();
}
