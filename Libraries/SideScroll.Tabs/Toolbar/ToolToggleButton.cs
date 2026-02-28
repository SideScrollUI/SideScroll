using SideScroll.Resources;
using SideScroll.Tabs.Lists;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Toolbar;

/// <summary>
/// Represents a toolbar toggle button that switches between checked and unchecked states
/// </summary>
public class ToolToggleButton : ToolButton
{
	/// <summary>
	/// The image resource to display when the button is checked/on
	/// </summary>
	public IResourceView OnImageResource { get; }

	/// <summary>
	/// The image resource to display when the button is unchecked/off
	/// </summary>
	public IResourceView OffImageResource { get; }

	/// <summary>
	/// Optional property binding that synchronizes the toggle state with a property
	/// </summary>
	public ListProperty? ListProperty { get; set; }

	/// <summary>
	/// Whether the button is currently checked
	/// </summary>
	public bool IsChecked { get; set; }

	/// <summary>
	/// Initializes a new toggle button with a synchronous action
	/// </summary>
	public ToolToggleButton(string tooltip, IResourceView onImageResource, IResourceView offImageResource, bool isChecked, CallAction? action = null, bool isDefault = false)
		: base(tooltip, offImageResource, action, isDefault)
	{
		OnImageResource = onImageResource;
		OffImageResource = offImageResource;
		IsChecked = isChecked;
	}

	/// <summary>
	/// Initializes a new toggle button with an asynchronous action
	/// </summary>
	public ToolToggleButton(string tooltip, IResourceView onImageResource, IResourceView offImageResource, bool isChecked, CallActionAsync? actionAsync, bool isDefault = false)
		: base(tooltip, offImageResource, actionAsync, isDefault)
	{
		OnImageResource = onImageResource;
		OffImageResource = offImageResource;
		IsChecked = isChecked;
	}

	/// <summary>
	/// Initializes a new toggle button bound to a property
	/// </summary>
	/// <param name="tooltip">Tooltip text for the button</param>
	/// <param name="onImageResource">Image to display when checked</param>
	/// <param name="offImageResource">Image to display when unchecked</param>
	/// <param name="listProperty">The property to bind the toggle state to</param>
	/// <param name="action">Optional action to execute when clicked</param>
	/// <param name="isDefault">Whether this is the default button</param>
	public ToolToggleButton(string tooltip, IResourceView onImageResource, IResourceView offImageResource, ListProperty listProperty, CallAction? action = null, bool isDefault = false)
		: base(tooltip, offImageResource, action, isDefault)
	{
		OnImageResource = onImageResource;
		OffImageResource = offImageResource;
		ListProperty = listProperty;
		IsChecked = listProperty.Value?.Equals(true) ?? false;
	}
}
