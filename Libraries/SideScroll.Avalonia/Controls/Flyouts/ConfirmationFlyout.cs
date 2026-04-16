using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Avalonia.Extensions;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Controls.Flyouts;

/// <summary>
/// A flyout that displays a confirmation prompt with configurable confirm and cancel buttons.
/// </summary>
public class ConfirmationFlyout : Flyout
{
	/// <summary>
	/// Creates a confirmation flyout with the given message and optional button labels.
	/// </summary>
	/// <param name="action">The action invoked when the confirm button is clicked.</param>
	/// <param name="text">The message displayed in the flyout.</param>
	/// <param name="confirmText">The label for the confirm button. Defaults to "Confirm".</param>
	/// <param name="cancelText">The label for the cancel button. Defaults to "Cancel".</param>
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
