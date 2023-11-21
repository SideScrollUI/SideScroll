using Atlas.Tabs;
using Atlas.UI.Avalonia.View;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using System.ComponentModel.DataAnnotations;

namespace Atlas.UI.Avalonia;

public static class AvaloniaUtils
{
	// TextBlock control doesn't allow selecting text, so add a Copy command to the context menu
	public static void AddContextMenu(TextBlock textBlock)
	{
		var list = new AvaloniaList<object>();

		var menuItemCopy = new TabMenuItem()
		{
			Header = "_Copy",
		};
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
			menuItemCut.Click += delegate { SendTextBoxKey(textBox, keymap.Cut); };
			list.Add(menuItemCut);
		}

		var menuItemCopy = new TabMenuItem("_Copy");
		menuItemCopy.Click += delegate { SendTextBoxKey(textBox, keymap.Copy); };
		list.Add(menuItemCopy);

		if (!textBox.IsReadOnly)
		{
			var menuItemPaste = new TabMenuItem("Paste");
			menuItemPaste.Click += delegate { SendTextBoxKey(textBox, keymap.Paste); };
			list.Add(menuItemPaste);
		}

		//list.Add(new Separator());

		ContextMenu contextMenu = new()
		{
			ItemsSource = list,
		};

		textBox.ContextMenu = contextMenu;
	}

	private static void SendTextBoxKey(TextBox textBox, List<KeyGesture> keyGestures)
	{
		foreach (var key in keyGestures)
		{
			var args = new KeyEventArgs()
			{
				Key = key.Key,
				KeyModifiers = key.KeyModifiers,
				RoutedEvent = TextBox.KeyDownEvent,
			};

			textBox.RaiseEvent(args);
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
}
