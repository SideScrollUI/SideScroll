using Atlas.Resources;
using Atlas.Tabs;
using Atlas.Tabs.Toolbar;
using System.ComponentModel;
using System.Windows.Input;

namespace Atlas.UI.Avalonia.Controls.Toolbar;

public class ToolbarToggleButton : ToolbarButton, IDisposable
{
	public IResourceView OnImageResource { get; set; }
	public IResourceView OffImageResource { get; set; }

	public ListProperty? ListProperty;
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

	public ToolbarToggleButton(TabControlToolbar toolbar, string tooltip, IResourceView onImageResource, IResourceView offImageResource, bool isChecked, string? label = null, ICommand? command = null) :
		base(toolbar, tooltip, isChecked ? onImageResource : offImageResource, label, command)
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

	public override void Invoke(bool canDelay = true)
	{
		if (!IsEnabled || IsActive) return;

		IsChecked = !IsChecked;
		if (ListProperty != null)
		{
			ListProperty.Value = IsChecked;
		}
		SetImage();

		base.Invoke(false);
	}
}