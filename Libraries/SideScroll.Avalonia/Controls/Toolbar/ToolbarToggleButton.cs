using SideScroll.Resources;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using System.ComponentModel;

namespace SideScroll.Avalonia.Controls.Toolbar;

/// <summary>
/// A two-state icon toggle button for use in a <see cref="TabControlToolbar"/>, with separate on/off images and optional model binding.
/// </summary>
public class ToolbarToggleButton : ToolbarButton
{
	protected override Type StyleKeyOverride => typeof(ToolbarButton);

	/// <summary>Gets or sets the image resource used when the toggle is in the on/checked state.</summary>
	public IResourceView OnImageResource { get; set; }

	/// <summary>Gets or sets the image resource used when the toggle is in the off/unchecked state.</summary>
	public IResourceView OffImageResource { get; set; }

	/// <summary>Gets the optional list property this toggle is bound to for two-way state sync.</summary>
	public ListProperty? ListProperty { get; }

	/// <summary>Gets or sets whether the toggle button is currently in the checked state.</summary>
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

	private void ListProperty_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ListProperty.Value))
		{
			IsChecked = ListProperty?.Value?.Equals(true) ?? false;
			SetImage();
		}
	}

	/// <summary>Toggles the check state, updates the bound property value, and invokes the base action.</summary>
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
