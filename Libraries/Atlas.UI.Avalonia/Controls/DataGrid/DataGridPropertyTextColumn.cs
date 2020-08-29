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
using Avalonia.Styling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Atlas.UI.Avalonia
{
	public class DataGridPropertyTextColumn : DataGridTextColumn
	{
		public SolidColorBrush BrushHasLinks => Theme.HasLinksBackground;
		public SolidColorBrush BrushEditable { get; set; } = Theme.Editable;
		public SolidColorBrush BrushValue { get; set; } = new SolidColorBrush(Colors.LightGray);
		public SolidColorBrush BrushBackground { get; set; } = new SolidColorBrush(Colors.White);

		//public bool Editable { get; set; } = false;
		public DataGrid DataGrid;
		public PropertyInfo PropertyInfo;

		public int MinDesiredWidth { get; set; } = 40;
		public int MaxDesiredWidth { get; set; } = 500;
		public bool AutoSize { get; set; }

		private Binding formattedBinding;
		private Binding unformattedBinding;
		private FormatValueConverter formatConverter = new FormatValueConverter();

		public DataGridPropertyTextColumn(DataGrid dataGrid, PropertyInfo propertyInfo, bool isReadOnly, int maxDesiredWidth)
		{
			DataGrid = dataGrid;
			PropertyInfo = propertyInfo;
			IsReadOnly = isReadOnly;
			MaxDesiredWidth = maxDesiredWidth;
			Binding = GetFormattedTextBinding();
			//Binding = GetTextBinding();
			if (DataGridUtils.IsTypeAutoSize(propertyInfo.PropertyType))
				AutoSize = true;

			CanUserSort = DataGridUtils.IsTypeSortable(propertyInfo.PropertyType);

			//CellStyleClasses = new Classes()
		}

		public override string ToString() => PropertyInfo.Name;

		// never gets triggered, can't override since it's internal?
		// Owning Grid also internal so can't add our own handler
		// _owningGrid.LoadingRow += OwningGrid_LoadingRow;
		/*protected override void RefreshCellContent(IControl element, string propertyName)
		{
			base.RefreshCellContent(element, propertyName);
		}*/

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			//cell.Background = GetCellBrush(cell, dataItem);
			cell.MaxHeight = 100; // don't let them have more than a few lines each

			// Support mixed control types?
			// this needs to get set when the cell content value changes, see LoadingRow()
			/*if (GetBindingType(dataItem) == typeof(bool))
			{
				var checkbox = new CheckBox()
				{
					Margin = new Thickness(10, 0, 0, 0), // aligns with header title better than centering
				};
				GetTextBinding();
				//unformattedBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				if (Binding != null)
					checkbox.Bind(CheckBox.IsCheckedProperty, Binding);
				if (IsReadOnly)
					checkbox.IsHitTestVisible = false; // disable changing
				//formattedBinding = unformattedBinding;
				//Binding = unformattedBinding;
				return checkbox;
			}
			else*/
			{
				//TextBlock textBlock = (TextBlock)base.GenerateElement(cell, dataItem);
				TextBlock textBlock = CreateTextBlock(cell, dataItem);

				/*var style = new Style(x => x.OfType<DataGridCell>())
				{
					Setters = new[]
					{
						new Setter(DataGridCell.BackgroundProperty, BrushEditable),
					},
				};
				cell.Styles.Add(style);*/

				if (DisplayIndex == 1 || PropertyInfo.IsDefined(typeof(StyleValueAttribute)))
				{
					// Update the cell color based on the object
					var binding = new Binding()
					{
						Converter = new ValueToBrushConverter(PropertyInfo),
						Mode = BindingMode.OneWay,
					};
					cell.Bind(DataGridCell.BackgroundProperty, binding);

					var foregroundBinding = new Binding()
					{
						Converter = new ValueToForegroundBrushConverter(PropertyInfo),
						Mode = BindingMode.OneWay,
					};
					textBlock.Bind(TextBlock.ForegroundProperty, foregroundBinding);
				}

				/*if (propertyInfo.IsDefined(typeof(StyleLabelAttribute)))
				{
					var foregroundBinding = new Binding()
					{
						Converter = new ValueToForegroundBrushConverter(propertyInfo),
						Mode = BindingMode.OneWay,
					};
					cell.Bind(DataGridCell.ForegroundProperty, foregroundBinding);
				}*/

				/*if (propertyInfo.IsDefined(typeof(StyleValueAttribute)))
				{
					var foregroundBinding = new Binding()
					{
						Converter = new ValueToForegroundBrushConverter(propertyInfo),
						Mode = BindingMode.OneWay,
					};
					textBlock.Bind(TextBlock.ForegroundProperty, foregroundBinding);
				}*/

				return textBlock;
			}
		}

		public class TextBlockElement : TextBlock, IStyleable, ILayoutable
		{
			Type IStyleable.StyleKey => typeof(TextBlock);

			public double MaxDesiredWidth { get; set; } = 500;

			private DataGridPropertyTextColumn column;
			private PropertyInfo propertyInfo;

			public TextBlockElement(DataGridPropertyTextColumn column, PropertyInfo propertyInfo)
			{
				this.column = column;
				this.propertyInfo = propertyInfo;
			}

			protected Size GetMaxSize(Size size)
			{
				double maxDesiredWidth = MaxDesiredWidth;
				if (DataContext is IMaxDesiredWidth iMaxWidth && column.DisplayIndex == 1 && iMaxWidth.MaxDesiredWidth != null)
				{
					maxDesiredWidth = iMaxWidth.MaxDesiredWidth.Value;
				}

				Size maxSize = new Size(Math.Min(maxDesiredWidth, size.Width), size.Height);
				return maxSize;
			}

			// can't override DesiredSize
			protected override Size MeasureCore(Size availableSize)
			{
				availableSize = GetMaxSize(availableSize);
				Size measured = base.MeasureCore(availableSize);
				measured = GetMaxSize(measured);
				return measured;
			}
		}

		protected TextBlock CreateTextBlock(DataGridCell cell, object dataItem)
		{
			var textBlockElement = new TextBlockElement(this, PropertyInfo)
			{
				Margin = new Thickness(5),
				VerticalAlignment = VerticalAlignment.Center,
				MaxDesiredWidth = this.MaxDesiredWidth,
				//FontFamily
				//FontSize
				//FontStyle
				//FontWeight
				//Foreground
			};
			if (PropertyInfo.GetCustomAttribute<WordWrapAttribute>() != null)
				textBlockElement.TextWrapping = TextWrapping.Wrap;
			textBlockElement.TextAlignment = DataGridUtils.GetTextAlignment(PropertyInfo.PropertyType);
			AddTextBoxContextMenu(cell, textBlockElement);

			if (Binding != null)
			{
				textBlockElement.Bind(TextBlock.TextProperty, Binding);
				
				/*var directBindingsField = textBlockElement.GetType().GetField("_directBindings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (directBindingsField.GetValue(textBlockElement) == null)
				{
					directBindingsField.SetValue(textBlockElement, new List<AvaloniaObject.DirectBindingSubscription>();
				}

				return new AvaloniaObject.DirectBindingSubscription(textBlockElement, TextBlock.TextProperty, Binding);*/
			}
			return textBlockElement;
		}

		private void AddTextBoxContextMenu(DataGridCell cell, TextBlock textBlock)
		{
			var contextMenu = new ContextMenu();

			var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

			var list = new AvaloniaList<object>();

			var menuItemCopy = new MenuItem() { Header = "Copy - _Cell Contents" };
			menuItemCopy.Click += delegate
			{
				ClipBoardUtils.SetTextAsync(textBlock.Text);
			};
			list.Add(menuItemCopy);

			list.Add(new Separator());

			var menuItemCopyColumn = new MenuItem() { Header = "Copy - Co_lumn" };
			menuItemCopyColumn.Click += delegate
			{
				string text = DataGrid.ColumnToStringTable(this);
				if (text != null)
					ClipBoardUtils.SetTextAsync(text);
			};
			list.Add(menuItemCopyColumn);

			var menuItemCopyRow = new MenuItem() { Header = "Copy - _Row" };
			menuItemCopyRow.Click += delegate
			{
				string text = DataGrid.RowToString(cell.DataContext);
				if (text != null)
					ClipBoardUtils.SetTextAsync(text);
			};
			list.Add(menuItemCopyRow);

			var menuItemCopySelected = new MenuItem() { Header = "Copy - _Selected" };
			menuItemCopySelected.Click += delegate
			{
				string text = DataGrid.SelectedToString();
				if (text != null)
					ClipBoardUtils.SetTextAsync(text);
			};
			list.Add(menuItemCopySelected);

			var menuItemCopyDataGrid = new MenuItem() { Header = "Copy - _DataGrid" };
			menuItemCopyDataGrid.Click += delegate
			{
				string text = DataGrid.ToStringTable();
				if (text != null)
					ClipBoardUtils.SetTextAsync(text);
			};
			list.Add(menuItemCopyDataGrid);

			var menuItemCopyDataGridCsv = new MenuItem() { Header = "Copy - DataGrid - CS_V" };
			menuItemCopyDataGridCsv.Click += delegate
			{
				string text = DataGrid.ToCsv();
				if (text != null)
					ClipBoardUtils.SetTextAsync(text);
			};
			list.Add(menuItemCopyDataGridCsv);

			contextMenu.Items = list;

			cell.IsHitTestVisible = true;
			cell.Focusable = true;
			cell.ContextMenu = contextMenu;
		}

		// todo: set default background brush to white so context menu's work, hover breaks if it's set though
		private IBrush GetCellBrush(DataGridCell dataGridCell, object dataItem)
		{
			object obj = dataGridCell.DataContext;
			try
			{
				if (PropertyInfo.IsDefined(typeof(StyleValueAttribute)))
				//if (this.DisplayIndex == 1 && (dataItem is ListItem || dataItem is ListMember))
				{
					bool hasLinks = TabModel.ObjectHasLinks(dataItem, true);
					if (hasLinks)
						return BrushHasLinks;
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
			if (PropertyInfo.GetIndexParameters().Length > 0)
				return null;

			if (PropertyInfo.DeclaringType.IsAssignableFrom(dataItem.GetType()))
			{
				object obj = PropertyInfo.GetValue(dataItem);
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
				return new Binding(PropertyInfo.Name);

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
			Binding binding = Binding as Binding ?? new Binding(PropertyInfo.Name);

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