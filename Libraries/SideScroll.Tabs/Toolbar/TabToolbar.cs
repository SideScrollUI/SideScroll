namespace SideScroll.Tabs.Toolbar;

/// <summary>
/// Container for toolbar buttons and controls displayed in a tab
/// This class can be created outside the UI thread, and the UI controls will be created when loading
/// </summary>
public class TabToolbar
{
	/// <summary>
	/// Collection of toolbar buttons
	/// </summary>
	public List<ToolButton> Buttons { get; set; } = [];
}
