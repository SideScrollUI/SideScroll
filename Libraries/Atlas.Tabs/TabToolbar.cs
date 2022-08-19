using Atlas.Core;

namespace Atlas.Tabs;

public class ToolButton
{
	public string Tooltip { get; set; }
	public string? Label { get; set; }
	public Stream Icon { get; set; }
	public bool ShowTask { get; set; }
	public bool Default { get; set; } // Use Enter as HotKey, add more complex keymapping later?

	public TaskDelegate.CallAction? Action { get; set; }
	public TaskDelegateAsync.CallActionAsync? ActionAsync { get; set; }

	public ToolButton(string tooltip, Stream icon, TaskDelegate.CallAction? action = null, bool isDefault = false)
	{
		Tooltip = tooltip;
		Icon = icon;
		Action = action;
		Default = isDefault;
	}

	public ToolButton(string tooltip, Stream icon, TaskDelegateAsync.CallActionAsync? actionAsync, bool isDefault = false)
	{
		Tooltip = tooltip;
		Icon = icon;
		ActionAsync = actionAsync;
		Default = isDefault;
	}
}

public class TabToolbar
{
	public List<ToolButton> Buttons { get; set; } = new();
}
