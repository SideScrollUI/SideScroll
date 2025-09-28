using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SideScroll.Avalonia.Controls.Flyouts;

public class MessageFlyout : Flyout
{
	public MessageFlyout(string text)
	{
		Placement = PlacementMode.Bottom;
		Content = new StackPanel
		{
			Margin = new Thickness(10, 10, 10, 5),
			Spacing = 8,
			MaxWidth = 350,
			Children =
			{
				new TextBlock
				{
					Text = text,
					TextWrapping = TextWrapping.Wrap,
					Margin = new Thickness(0, 0, 0, 10)
				},
			}
		};
	}
}
