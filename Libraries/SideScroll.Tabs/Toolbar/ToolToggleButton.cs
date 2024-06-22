using SideScroll.Core.Tasks;
using SideScroll.Resources;

namespace SideScroll.Tabs.Toolbar;

public class ToolToggleButton : ToolButton
{
	public IResourceView OnImageResource { get; set; }
	public IResourceView OffImageResource { get; set; }

	public ListProperty? ListProperty;

	public bool IsChecked { get; set; }

	public ToolToggleButton(string tooltip, IResourceView onImageResource, IResourceView offImageResource, bool isChecked, CallAction? action = null, bool isDefault = false)
		: base(tooltip, offImageResource, action, isDefault)
	{
		OnImageResource = onImageResource;
		OffImageResource = offImageResource;
		IsChecked = isChecked;
	}

	public ToolToggleButton(string tooltip, IResourceView onImageResource, IResourceView offImageResource, bool isChecked, CallActionAsync? actionAsync, bool isDefault = false)
		: base(tooltip, offImageResource, actionAsync, isDefault)
	{
		OnImageResource = onImageResource;
		OffImageResource = offImageResource;
		IsChecked = isChecked;
	}

	public ToolToggleButton(string tooltip, IResourceView onImageResource, IResourceView offImageResource, ListProperty listProperty, CallAction? action = null, bool isDefault = false)
		: base(tooltip, offImageResource, action, isDefault)
	{
		OnImageResource = onImageResource;
		OffImageResource = offImageResource;
		ListProperty = listProperty;
		IsChecked = listProperty.Value?.Equals(true) ?? false;
	}
}
