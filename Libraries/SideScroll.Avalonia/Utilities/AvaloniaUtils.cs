using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using SideScroll.Avalonia.Controls;
using SideScroll.Tabs.Lists;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.Avalonia.Utilities;

/// <summary>
/// Provides utility methods for Avalonia UI controls
/// </summary>
public static class AvaloniaUtils
{
	/// <summary>
	/// Adds a context menu with Copy functionality to a TextBlock control
	/// </summary>
	public static void AddContextMenu(TextBlock textBlock)
	{
		AvaloniaList<object> list = [];

		var menuItemCopy = new TabMenuItem("_Copy");
		menuItemCopy.Click += async delegate
		{
			await ClipboardUtils.SetTextAsync(textBlock, textBlock.Text ?? "");
		};
		list.Add(menuItemCopy);

		ContextMenu contextMenu = new()
		{
			ItemsSource = list,
		};

		textBlock.ContextMenu = contextMenu;
	}

	/// <summary>
	/// Adds a context menu with Cut, Copy, and Paste functionality to a TextBox control
	/// </summary>
	public static void AddContextMenu(TextBox textBox)
	{
		var keymap = Application.Current!.PlatformSettings!.HotkeyConfiguration;

		AvaloniaList<object> list = [];

		if (!textBox.IsReadOnly)
		{
			var menuItemCut = new TabMenuItem("Cu_t");
			menuItemCut.Click += delegate { SendKeyGesture(textBox, keymap.Cut); };
			list.Add(menuItemCut);
		}

		var menuItemCopy = new TabMenuItem("_Copy");
		menuItemCopy.Click += delegate { SendKeyGesture(textBox, keymap.Copy); };
		list.Add(menuItemCopy);

		if (!textBox.IsReadOnly)
		{
			var menuItemPaste = new TabMenuItem("_Paste");
			menuItemPaste.Click += delegate { SendKeyGesture(textBox, keymap.Paste); };
			list.Add(menuItemPaste);
		}

		ContextMenu contextMenu = new()
		{
			ItemsSource = list,
		};

		textBox.ContextMenu = contextMenu;
	}

	/// <summary>
	/// Adds a context menu with Copy and Paste functionality to a ComboBox control
	/// </summary>
	public static void AddContextMenu(ComboBox comboBox)
	{
		AvaloniaList<object> list = [];

		var menuItemCopy = new TabMenuItem("_Copy");
		menuItemCopy.Click += async delegate
		{
			await ClipboardUtils.SetTextAsync(comboBox, comboBox.SelectedItem?.ToString() ?? "");
		};
		list.Add(menuItemCopy);

		var menuItemPaste = new TabMenuItem("_Paste");
		menuItemPaste.Click += async delegate
		{
			if (await ClipboardUtils.TryGetTextAsync(comboBox) is string clipboardText)
			{
				if (comboBox.Items.FirstOrDefault(i => i?.ToString() == clipboardText) is object matchingItem)
				{
					comboBox.SelectedItem = matchingItem;
				}
			}
		};
		list.Add(menuItemPaste);

		ContextMenu contextMenu = new()
		{
			ItemsSource = list,
		};

		comboBox.ContextMenu = contextMenu;
	}

	/// <summary>
	/// Adds a context menu with Copy and Paste functionality to a ColorPicker control
	/// </summary>
	public static void AddContextMenu(ColorPicker colorPicker)
	{
		AvaloniaList<object> list = [];

		var menuItemCopy = new TabMenuItem("_Copy");
		menuItemCopy.Click += async delegate
		{
			await ClipboardUtils.SetTextAsync(colorPicker, colorPicker.Color.ToString());
		};
		list.Add(menuItemCopy);

		var menuItemPaste = new TabMenuItem("_Paste");
		menuItemPaste.Click += async delegate
		{
			if (await ClipboardUtils.TryGetTextAsync(colorPicker) is string clipboardText &&
				Color.TryParse(clipboardText, out Color color))
			{
				colorPicker.Color = color;
			}
		};
		list.Add(menuItemPaste);

		ContextMenu contextMenu = new()
		{
			ItemsSource = list,
		};

		colorPicker.ContextMenu = contextMenu;
	}

	private static void SendKeyGesture(InputElement inputElement, List<KeyGesture> keyGestures)
	{
		foreach (KeyGesture keyGesture in keyGestures)
		{
			KeyEventArgs args = new()
			{
				Key = keyGesture.Key,
				KeyModifiers = keyGesture.KeyModifiers,
				RoutedEvent = InputElement.KeyDownEvent,
			};

			inputElement.RaiseEvent(args);
			break;
		}
	}

	/// <summary>
	/// Determines if a control is visible within its parent hierarchy
	/// </summary>
	public static bool IsControlVisible(Control control)
	{
		// Add padding param to load in earlier and avoid poppin effect?
		Point controlTopLeftPoint = new(0, 0);
		Point controlBottomRight = new(control.Bounds.Width, control.Bounds.Height);
		StyledElement? parentElement = control.Parent;
		while (parentElement != null)
		{
			// sometimes controls don't update their bounds correctly, so only use the Window for now
			if (parentElement is Control parentControl)
			{
				// Get control bounds in Parent control coordinates
				Point? translatedTopLeft = control.TranslatePoint(controlTopLeftPoint, parentControl);
				Point? translatedBottomRight = control.TranslatePoint(controlBottomRight, parentControl);
				if (translatedTopLeft == null || translatedBottomRight == null)
					return false;

				var parentBounds = new Rect(translatedTopLeft.Value, translatedBottomRight.Value);
				parentBounds = parentBounds.WithX(parentBounds.X + parentControl.Bounds.X);
				parentBounds = parentBounds.WithY(parentBounds.Y + parentControl.Bounds.Y);

				if (parentBounds.X > parentControl.Bounds.Right ||
					parentBounds.Y > parentControl.Bounds.Bottom ||
					parentBounds.Right < parentControl.Bounds.X ||
					parentBounds.Bottom < parentControl.Bounds.Y)
					return false;
			}

			parentElement = parentElement.Parent;
		}
		return true;
	}

	/// <summary>
	/// Validates a control against data validation attributes and displays error messages.
	/// Supports RequiredAttribute, StringLengthAttribute, and RangeAttribute validation.
	/// </summary>
	public static bool ValidateControl(ListProperty listProperty, Control control)
	{
		dynamic? value = listProperty.Value;

		if (listProperty.GetCustomAttribute<RequiredAttribute>() != null)
		{
			if (value == null || (value is string text && text.Length == 0))
			{
				DataValidationErrors.SetError(control, new DataValidationException("Required"));
				return false;
			}
		}

		if (value == null) return true;

		if (listProperty.GetCustomAttribute<StringLengthAttribute>() is StringLengthAttribute stringLengthAttribute)
		{
			if (!stringLengthAttribute.IsValid(value))
			{
				DataValidationErrors.SetError(control, new DataValidationException(stringLengthAttribute.FormatErrorMessage(listProperty.Name!)));
				return false;
			}
		}

		if (listProperty.GetCustomAttribute<RangeAttribute>() is RangeAttribute rangeAttribute)
		{
			dynamic minValue = rangeAttribute.Minimum;
			if (value < minValue)
			{
				DataValidationErrors.SetError(control, new DataValidationException("Min Value: " + minValue));
				return false;
			}

			dynamic maxValue = rangeAttribute.Maximum;
			if (value > maxValue)
			{
				DataValidationErrors.SetError(control, new DataValidationException("Max Value: " + maxValue));
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Shows a flyout with text content attached to a control
	/// </summary>
	public static void ShowFlyout(Control control, Flyout flyout, string text)
	{
		Dispatcher.UIThread.Post(() => ShowFlyoutUI(control, flyout, text));
	}

	private static void ShowFlyoutUI(Control control, Flyout flyout, string text)
	{
		flyout.Content = text;
		flyout.ShowAt(control);
	}
}
