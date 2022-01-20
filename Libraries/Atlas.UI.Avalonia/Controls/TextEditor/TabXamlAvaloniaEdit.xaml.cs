using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;
using Avalonia.Media;

namespace Atlas.UI.Avalonia.Tabs;

public class TabXamlAvaloniaEdit : UserControl //, IDisposable
{

	public TabXamlAvaloniaEdit()
	{
		InitializeControls();
		//DataContext = new TestNode().Children;
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		return base.MeasureOverride(availableSize);
	}

	private void Initialize()
	{
		InitializeControls();
	}

	protected override void OnMeasureInvalidated()
	{
		base.OnMeasureInvalidated();
	}

	private void InitializeControls()
	{
		AvaloniaXamlLoader.Load(this);

		//Background = new SolidColorBrush(Theme.BackgroundColor);
		Background = new SolidColorBrush(Colors.Purple);
		//HorizontalAlignment = HorizontalAlignment.Stretch;
		//VerticalAlignment = VerticalAlignment.Stretch;
		Width = 1000;
		Height = 1000;
	}
}
