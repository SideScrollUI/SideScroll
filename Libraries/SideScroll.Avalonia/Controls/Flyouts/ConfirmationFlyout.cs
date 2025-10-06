using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Avalonia.Extensions;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Controls.Flyouts;

public class ConfirmationFlyout : Flyout
{
	public ConfirmationFlyout(Action action, string text, string? confirmText = null, string? cancelText = null)
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
						new TabTextButton(cancelText ?? "Cancel")
						.Also(button =>
							{
								button.Click += (_, _) => Hide();
							}),
						new TabTextButton(confirmText ?? "Confirm", AccentType.Warning)
						.Also(button =>
							{
								button.Click += (_, _) =>
								{
									action();
									Hide();
								};
							})
					}
				}
			}
		};
	}
}
