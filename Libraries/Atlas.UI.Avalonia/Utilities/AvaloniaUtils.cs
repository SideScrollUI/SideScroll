using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using System;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia
{
	public class AvaloniaUtils
	{
		public static void AddTextBoxContextMenu(TextBox textBox)
		{
			var contextMenu = new ContextMenu();

			var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

			var list = new AvaloniaList<object>();
			var menuItemCut = new MenuItem() { Header = "Cut" };
			menuItemCut.Click += delegate { SendTextBoxKey(textBox, keymap.Cut); };
			list.Add(menuItemCut);

			var menuItemCopy = new MenuItem() { Header = "_Copy" };
			menuItemCopy.Click += delegate { SendTextBoxKey(textBox, keymap.Copy); };
			list.Add(menuItemCopy);

			var menuItemPaste = new MenuItem() { Header = "Paste" };
			menuItemPaste.Click += delegate { SendTextBoxKey(textBox, keymap.Paste); };
			list.Add(menuItemPaste);

			//list.Add(new Separator());

			contextMenu.Items = list;

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
			Point controlTopLeftPoint = new Point(0, 0);
			Point controlBottomRight = new Point(control.Bounds.Width, control.Bounds.Height);
			IControl parentControl = control?.Parent;
			while (parentControl != null)
			{
				// sometimes controls don't update their bounds correctly, so only use the Window for now
				if (parentControl is Window)
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
}
