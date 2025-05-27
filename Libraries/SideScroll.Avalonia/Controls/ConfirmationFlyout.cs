using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Avalonia.Extensions;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Controls;

public class ConfirmationFlyout : Flyout
{
	public ConfirmationFlyout(CallAction callAction, string text, string confirmText, string? cancelText = null)
	{
		Placement = PlacementMode.Bottom;
		Content = new StackPanel
		{
			Margin = new Thickness(10, 10, 10, 5),
			Spacing = 8,
			Width = 200,
			Children =
			{
				new TextBlock
				{
					Text = text,
					TextWrapping = TextWrapping.Wrap,
					Margin = new Thickness(0, 0, 0, 10)
				},
				new StackPanel
				{
					Orientation = Orientation.Horizontal,
					HorizontalAlignment = HorizontalAlignment.Right,
					Spacing = 8,
					Children =
					{
						new TabControlTextButton(cancelText ?? "Cancel")
						.Also(button =>
							{
								button.Click += (_, _) => Hide();
							}),
						new TabControlTextButton(confirmText, AccentType.Warning)
						.Also(button =>
							{
								button.Click += (_, _) =>
								{
									callAction(new Call());
									Hide();
								};
							})
					}
				}
			}
		};
	}
}
