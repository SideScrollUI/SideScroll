using SideScroll.Resources;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using System.ComponentModel;

namespace SideScroll.Avalonia.Controls.Toolbar;

public class ToolbarToggleButton : ToolbarButton
{
	protected override Type StyleKeyOverride => typeof(ToolbarButton);

	public IResourceView OnImageResource { get; set; }
	public IResourceView OffImageResource { get; set; }

	public ListProperty? ListProperty { get; set; }
	public bool IsChecked { get; set; }

	public ToolbarToggleButton(TabControlToolbar toolbar, ToolToggleButton toolButton) :
		base(toolbar, toolButton)
	{
		OnImageResource = toolButton.OnImageResource;
		OffImageResource = toolButton.OffImageResource;
		ListProperty = toolButton.ListProperty;
		IsChecked = toolButton.IsChecked;

		SetImage();

		if (ListProperty != null)
		{
			ListProperty.PropertyChanged += ListProperty_PropertyChanged;
		}
	}

	private void ListProperty_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ListProperty.Value))
		{
			IsChecked = ListProperty?.Value?.Equals(true) ?? false;
			SetImage();
		}
	}

	public ToolbarToggleButton(TabControlToolbar toolbar, string tooltip, IResourceView onImageResource, IResourceView offImageResource, bool isChecked, string? label = null) :
		base(toolbar, tooltip, isChecked ? onImageResource : offImageResource, null, label)
	{
		OnImageResource = onImageResource;
		OffImageResource = offImageResource;
		IsChecked = isChecked;

		SetImage();
	}

	protected void SetImage()
	{
		SetImage(IsChecked ? OnImageResource : OffImageResource);
	}

	public override async Task InvokeAsync(bool canDelay = true)
	{
		if (!IsEnabled || IsActive) return;

		IsChecked = !IsChecked;
		if (ListProperty != null)
		{
			ListProperty.Value = IsChecked;
		}
		SetImage();

		await base.InvokeAsync(false);
	}
}
