namespace SideScroll.Tabs.Toolbar;

/// <summary>
/// Container for toolbar buttons and controls displayed in a tab
/// This class can be created outside the UI thread, and the UI controls will be created when loading
/// </summary>
public class TabToolbar
{
	/// <summary>
	/// Additional toolbar buttons appended after any <see cref="ToolButton"/> properties
	/// declared on a subclass. The primary way to define buttons is to add them as typed
	/// properties on a <c>TabToolbar</c> subclass (e.g. <c>public ToolButton ButtonSave { get; } = new(…)</c>);
	/// use this collection only when buttons must be added dynamically at runtime.
	/// </summary>
	public List<ToolButton> AdditionalButtons { get; set; } = [];
}
