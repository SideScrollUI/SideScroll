using Atlas.UI.Avalonia.View;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace Atlas.UI.Avalonia;

public static class AvaloniaUtils
{
	// TextBlock control doesn't allow selecting text, so add a Copy command to the context menu
	public static void AddContextMenu(TextBlock textBlock)
	{
		var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

		var list = new AvaloniaList<object>();

		var menuItemCopy = new TabMenuItem()
		{
			Header = "_Copy",
		};
		menuItemCopy.Click += delegate
		{
			ClipBoardUtils.SetText(textBlock.Text);
		};
		list.Add(menuItemCopy);

		ContextMenu contextMenu = new()
		{
			Items = list,
		};

		textBlock.ContextMenu = contextMenu;
	}

	public static void AddContextMenu(TextBox textBox)
	{
		var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>()!;

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
			Items = list,
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
	public static bool IsControlVisible(IControl control)
	{
		Point controlTopLeftPoint = new(0, 0);
		Point controlBottomRight = new(control.Bounds.Width, control.Bounds.Height);
		IControl? parentControl = control?.Parent;
		while (parentControl != null)
		{
			// sometimes controls don't update their bounds correctly, so only use the Window for now
			//if (parentControl is Window)
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

			parentControl = parentControl.Parent;
		}
		return true;
	}
}
