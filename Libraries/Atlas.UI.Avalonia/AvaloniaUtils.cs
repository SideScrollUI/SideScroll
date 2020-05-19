﻿using Avalonia;
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
	}
}
