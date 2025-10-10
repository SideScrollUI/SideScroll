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

public static class AvaloniaUtils
{
	// TextBlock control doesn't allow selecting text, so add a Copy command to the context menu
	public static void AddContextMenu(TextBlock textBlock)
	{
		var list = new AvaloniaList<object>();

		var menuItemCopy = new TabMenuItem("_Copy");
		menuItemCopy.Click += delegate
		{
			ClipboardUtils.SetText(textBlock, textBlock.Text ?? "");
		};
		list.Add(menuItemCopy);

		ContextMenu contextMenu = new()
		{
			ItemsSource = list,
		};

		textBlock.ContextMenu = contextMenu;
	}

	public static void AddContextMenu(TextBox textBox)
	{
		var keymap = Application.Current!.PlatformSettings!.HotkeyConfiguration;

		var list = new AvaloniaList<object>();

		if (!textBox.IsReadOnly)
		{
			var menuItemCut = new TabMenuItem("Cut");
			menuItemCut.Click += delegate { SendKeyGesture(textBox, keymap.Cut); };
			list.Add(menuItemCut);
		}

		var menuItemCopy = new TabMenuItem("_Copy");
		menuItemCopy.Click += delegate { SendKeyGesture(textBox, keymap.Copy); };
		list.Add(menuItemCopy);

		if (!textBox.IsReadOnly)
		{
			var menuItemPaste = new TabMenuItem("Paste");
			menuItemPaste.Click += delegate { SendKeyGesture(textBox, keymap.Paste); };
			list.Add(menuItemPaste);
		}

		ContextMenu contextMenu = new()
		{
			ItemsSource = list,
		};

		textBox.ContextMenu = contextMenu;
	}

	public static void AddContextMenu(ComboBox comboBox)
	{
		var list = new AvaloniaList<object>();

		var menuItemCopy = new TabMenuItem("_Copy");
		menuItemCopy.Click += delegate
		{
			ClipboardUtils.SetText(comboBox, comboBox.SelectedItem?.ToString() ?? "");
		};
		list.Add(menuItemCopy);

		var menuItemPaste = new TabMenuItem("Paste");
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

	public static void AddContextMenu(ColorPicker colorPicker)
	{
		var list = new AvaloniaList<object>();

		var menuItemCopy = new TabMenuItem("_Copy");
		menuItemCopy.Click += delegate
		{
			ClipboardUtils.SetText(colorPicker, colorPicker.Color.ToString() ?? "");
		};
		list.Add(menuItemCopy);

		var menuItemPaste = new TabMenuItem("Paste");
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
		foreach (var key in keyGestures)
		{
			var args = new KeyEventArgs
			{
				Key = key.Key,
				KeyModifiers = key.KeyModifiers,
				RoutedEvent = InputElement.KeyDownEvent,
			};

			inputElement.RaiseEvent(args);
			break;
		}
	}

	// Add padding to avoid poppin effect?
	public static bool IsControlVisible(Control control)
	{
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
