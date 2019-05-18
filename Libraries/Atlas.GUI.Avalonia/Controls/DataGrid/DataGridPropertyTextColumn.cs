using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Atlas.GUI.Avalonia
{
	public class DataGridPropertyTextColumn : DataGridTextColumn
	{
		public SolidColorBrush BrushHasChildren { get; set; } = new SolidColorBrush(Theme.HasChildrenColor);
		public SolidColorBrush BrushEditable { get; set; } = new SolidColorBrush(Theme.EditableColor);
		public SolidColorBrush BrushValue { get; set; } = new SolidColorBrush(Colors.LightGray);

		//public bool Editable { get; set; } = false;

		private Binding formattedBinding;
		private Binding unformattedBinding;
		private FieldValueConverter formatConverter = new FieldValueConverter();
		private PropertyInfo propertyInfo;
		
		public DataGridPropertyTextColumn(PropertyInfo propertyInfo, bool isReadOnly)
		{
			this.propertyInfo = propertyInfo;
			IsReadOnly = isReadOnly;
			Binding = GetFormattedTextBinding();
			//Binding = GetTextBinding();

			CanUserSort = IsSortable(propertyInfo.PropertyType);
		}

		public override string ToString()
		{
			return propertyInfo.Name;
		}

		private bool IsSortable(Type type)
		{
			type = type.GetNonNullableType();
			if (type.IsPrimitive ||
				type.IsEnum ||
				type == typeof(string) ||
				type == typeof(DateTime) ||
				type == typeof(TimeSpan))
				return true;

			return false;
		}

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			cell.Background = GetCellBrush(cell, dataItem);
			cell.MaxHeight = 100; // don't let them have more than a few lines each

			TextBlock textBlock = (TextBlock)base.GenerateElement(cell, dataItem);
			//TextBlock textBlock = GetTextBlock(cell, dataItem);
			textBlock.DoubleTapped += delegate
			{
				((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(textBlock.Text);
			};
			AddTextBoxContextMenu(textBlock);
			return textBlock;
		}

		// so we don't load slow templates?
		/*public class SubTextBlock : TextBlock
		{
		}

		protected TextBlock GetTextBlock(DataGridCell cell, object dataItem)
		{
			SubTextBlock textBlockElement = new SubTextBlock
			{
				Margin = new Thickness(4),
				VerticalAlignment = VerticalAlignment.Center,
				//FontFamily
				//FontSize
				//FontStyle
				//FontWeight
				//Foreground
			};

			if (Binding != null)
			{
				textBlockElement.Bind(TextBlock.TextProperty, Binding);
				
				/*var directBindingsField = textBlockElement.GetType().GetField("_directBindings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (directBindingsField.GetValue(textBlockElement) == null)
				{
					directBindingsField.SetValue(textBlockElement, new List<AvaloniaObject.DirectBindingSubscription>();
				}

				return new AvaloniaObject.DirectBindingSubscription(textBlockElement, TextBlock.TextProperty, Binding);*//*
			}
			return textBlockElement;
		}*/

		private void AddTextBoxContextMenu(TextBlock textBlock)
		{
			ContextMenu contextMenu = new ContextMenu();

			var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

			var list = new AvaloniaList<object>();

			MenuItem menuItemCopy = new MenuItem() { Header = "_Copy - Cell Contents" };
			menuItemCopy.Click += delegate
			{
				((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(textBlock.Text);
			};
			list.Add(menuItemCopy);

			//list.Add(new Separator());

			contextMenu.Items = list;

			textBlock.ContextMenu = contextMenu;
		}

		private IBrush GetCellBrush(DataGridCell dataGridCell, object dataItem)
		{
			object obj = dataGridCell.DataContext;
			try
			{
				if (propertyInfo.IsDefined(typeof(StyleValueAttribute)))
				//if (this.DisplayIndex == 1 && (dataItem is ListItem || dataItem is ListMember))
				{
					bool hasChildren = TabModel.ObjectHasChildren(dataItem);
					if (hasChildren)
						return BrushHasChildren;
					//return Brushes.Moccasin;
					else if (!IsReadOnly && (dataItem is ListMember) && ((ListMember)dataItem).Editable)
						return BrushEditable;
					else
						return BrushValue;
				}
			}
			catch (InvalidCastException)
			{
			}

			if (IsReadOnly)
				return null; // checkbox column requires a valid value
			else
				return BrushEditable;
		}

		/*protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
		{
			if (GetBindingType(dataItem) == typeof(bool))
			{
				CheckBox checkbox = new CheckBox()
				{
					Margin = new Thickness(10, 0, 0, 0)
				};
				GetTextBinding();
				unformattedBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				checkbox.SetBinding(CheckBox.IsCheckedProperty, unformattedBinding);
				if (IsReadOnly)
					checkbox.IsHitTestVisible = false; // disable changing
													   //formattedBinding = unformattedBinding;
													   //Binding = unformattedBinding;
				return checkbox;
			}
			else
			{
				TextBlock element = base.GenerateElement(cell, dataItem) as TextBlock;
				element.SetBinding(TextBlock.TextProperty, GetFormattedTextBinding());
				return element;
			}
		}

		protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
		{
			if (GetBindingType(dataItem) == typeof(bool))
			{
				CheckBox checkbox = new CheckBox()
				{
					HorizontalAlignment = HorizontalAlignment.Center
				};
				checkbox.SetBinding(CheckBox.IsCheckedProperty, GetTextBinding());
				return checkbox;
			}
			else
			{
				TextBox element = base.GenerateEditingElement(cell, dataItem) as TextBox;
				element.SetBinding(TextBox.TextProperty, GetTextBinding());
				return element;
			}
		}*/

		private Type GetBindingType(object dataItem)
		{
			if (propertyInfo.GetIndexParameters().Length > 0)
				return null;

			if (propertyInfo.DeclaringType.IsAssignableFrom(dataItem.GetType()))
			{
				object obj = propertyInfo.GetValue(dataItem);
				if (obj == null)
					return null;
				return obj.GetType();
			}
			return null;
		}

		Binding GetTextBinding()
		{
			Binding binding = (Binding)Binding;
			if (binding == null)
				return new Binding(propertyInfo.Name);

			//if (unformattedBinding == null)
			{
				unformattedBinding = new Binding
				{
					Path = binding.Path,
					Mode = BindingMode.OneWay, // copying a value to the clipboard triggers an infinite loop without this?
				};
				//if (!IsReadOnly)
				//	unformattedBinding.Mode = BindingMode.TwoWay;
				//else
				//unformattedBinding.Mode = binding.Mode;
				//unformattedBinding.BindsDirectlyToSource = true;
			}

			return unformattedBinding;
		}

		Binding GetFormattedTextBinding()
		{
			Binding binding = (Binding)Binding;
			if (binding == null)
				binding = new Binding(propertyInfo.Name);

			if (formattedBinding == null)
			{
				formattedBinding = new Binding
				{
					Path = binding.Path,
					//Mode = binding.Mode,
					Mode = BindingMode.OneWay, // copying a value to the clipboard triggers an infinite loop without this?
				};
				if (IsReadOnly)
					formattedBinding.Converter = formatConverter;
			}

			return formattedBinding;
		}
	}
}