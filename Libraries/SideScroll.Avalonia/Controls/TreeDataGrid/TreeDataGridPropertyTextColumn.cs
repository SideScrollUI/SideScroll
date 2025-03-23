using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Data;
using Avalonia.Media;
using SideScroll.Attributes;
using SideScroll.Avalonia.Controls.Converters;
using SideScroll.Avalonia.Utilities;
using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
namespace SideScroll.Avalonia.Controls.DataGrids;

public class TreeDataGridPropertyTextColumn<TModel, TProperty> : TextColumn<TModel, TProperty?>
	where TModel : class
{
	public static int EnableWordWrapMinStringLength { get; set; } = 64; // Don't enable wordwrap unless we have to (expensive and not always wanted)
	public static int MaxRowScanProperties { get; set; } = 30;

	public TreeDataGrid DataGrid { get; init; }
	public PropertyInfo PropertyInfo { get; set; }

	public int MinDesiredWidth { get; set; } = 25;
	public int MaxDesiredWidth { get; set; } = 500;
	public int MaxDesiredHeight { get; set; } = 100;

	public bool AutoSize { get; set; }
	public bool WordWrap { get; set; }
	//public bool Editable { get; set; } = false;
	public bool StyleCells { get; set; } // True if any column has a Style applied, so we can manually draw the horizontal lines

	public Binding FormattedBinding { get; set; }
	//private Binding unformattedBinding;
	//public FormatValueConverter FormatConverter { get; set; } = new();

	public override string ToString() => PropertyInfo.Name;

	public TreeDataGridPropertyTextColumn(
		TreeDataGrid dataGrid, 
		string label, 
		PropertyInfo propertyInfo,
		bool isReadOnly, 
		int maxDesiredWidth, 
		PropertyTextConverter<TProperty> textConverter,
		GridLength? gridLength = null)
		: base(label, x => textConverter.GetValue(x), gridLength) 
	{
		DataGrid = dataGrid;
		PropertyInfo = propertyInfo;
		//IsReadOnly = isReadOnly;
		MaxDesiredWidth = maxDesiredWidth;
		Options.MaxWidth = new(MaxDesiredWidth);

		if (typeof(TProperty).IsNumeric())
		{
			Options.TextAlignment = TextAlignment.Right;
		}

		//Binding = GetFormattedTextBinding();

		var maxHeightAttribute = propertyInfo.GetCustomAttribute<MaxHeightAttribute>();
		if (maxHeightAttribute != null && typeof(IListItem).IsAssignableFrom(PropertyInfo.PropertyType))
		{
			MaxDesiredHeight = maxHeightAttribute.MaxHeight;
			//FormatConverter.MaxLength = MaxDesiredHeight * 10;
		}
		//FormatConverter.IsFormatted = (propertyInfo.GetCustomAttribute<FormattedAttribute>() != null);

		/*var formatterAttribute = propertyInfo.GetCustomAttribute<FormatterAttribute>();
		if (formatterAttribute != null)
		{
			FormatConverter.Formatter = (ICustomFormatter)Activator.CreateInstance(formatterAttribute.Type)!;
		}*/

		if (DataGridUtils.IsTypeAutoSize(propertyInfo.PropertyType))
		{
			AutoSize = true;
		}

		Options.CanUserSortColumn = DataGridUtils.IsTypeSortable(propertyInfo.PropertyType);

		//if (PropertyInfo.GetCustomAttribute<WordWrapAttribute>() != null)
		{
			Options.TextWrapping = TextWrapping.Wrap;
		}

		//CellStyleClasses = new Classes()
	}

	/*private static string? GetValue(TreeDataGridPropertyTextColumn<TModel> column, TModel model)
	{
		return (string)column.FormatConverter.Convert(model, typeof(string), null, CultureInfo.CurrentCulture)!;
	}*/

	// Check first x rows for [Hide()] and apply WordWrap to strings/objects automatically
	public void ScanItemAttributes(IList List)
	{
		bool checkWordWrap = (!WordWrap && (PropertyInfo.PropertyType == typeof(string) || PropertyInfo.PropertyType == typeof(object)));

		var hideAttribute = PropertyInfo.GetCustomAttribute<HideAttribute>();

		if (checkWordWrap || hideAttribute != null)
		{
			/*if (hideAttribute != null && List.Count > 0)
			{
				IsVisible = false;
			}*/

			for (int i = 0; i < MaxRowScanProperties && i < List.Count; i++)
			{
				object? obj = List[i];
				if (obj == null)
					continue;

				try
				{
					object? value = PropertyInfo.GetValue(obj); // Can throw exception
					/*if (hideAttribute != null)
					{
						if (!hideAttribute.Values.Contains(value))
						{
							IsVisible = true;
						}
					}*/

					if (checkWordWrap && value != null)
					{
						string? text = value.ToString();
						if (text != null && text.Length > EnableWordWrapMinStringLength)
						{
							WordWrap = true;
						}
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());
				}
			}
		}
	}

	// never gets triggered, can't override since it's internal?
	// Owning Grid also internal so can't add our own handler
	// _owningGrid.LoadingRow += OwningGrid_LoadingRow;
	/*protected override void RefreshCellContent(IControl element, string propertyName)
	{
		base.RefreshCellContent(element, propertyName);
	}*/

	//protected override Control GenerateElement(DataGridCell cell, object dataItem)
	//{
		//cell.MaxHeight = MaxDesiredHeight; // don't let them have more than a few lines each

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
		/*{
			TextBlock textBlock = CreateTextBlock(cell);

			if (StyleCells)
			{
				return AddStyling(cell, textBlock);
			}

			return textBlock;
		}*/
	//}

	// Styled columns have a different line color, so we have to draw them manually
	// They also use different background colors, with different shades for links vs non-links
	/*private TextBlock AddStyling(DataGridCell cell, TextBlock textBlock)
	{
		if (PropertyInfo.IsDefined(typeof(StyleValueAttribute)) ||
			(DisplayIndex == 1 && typeof(DictionaryEntry).IsAssignableFrom(PropertyInfo.DeclaringType)))
		{
			// Update the cell color based on the object
			var binding = new Binding
			{
				Converter = new ValueToBackgroundBrushConverter(PropertyInfo),
				Mode = BindingMode.OneWay,
			};
			cell.Bind(DataGridCell.BackgroundProperty, binding);

			var foregroundBinding = new Binding
			{
				Converter = new ValueToForegroundBrushConverter(PropertyInfo),
				Mode = BindingMode.OneWay,
			};
			textBlock.Bind(TextBlock.ForegroundProperty, foregroundBinding);

			cell.BorderBrush = SideScrollTheme.DataGridStyledBorder;
		}

		if (DisplayIndex > 0)
		{
			//border.BorderThickness = new Thickness(1, 0, 0, 1); // Left and Bottom
			cell.BorderThickness = new Thickness(1, 0, 0, 1); // Left and Bottom
			//cell.BorderThickness = new Thickness(0, 0, 1, 1); // Right and Bottom
		}
		else
		{
			//border.BorderThickness = new Thickness(0, 0, 1, 1); // Right and Bottom
			cell.BorderThickness = new Thickness(0, 0, 1, 1); // Right and Bottom
		}

		return textBlock;
	}*/

	/*protected TextBlock CreateTextBlock(DataGridCell cell)
	{
		var textBlockElement = new TextBlockElement(this, PropertyInfo);

		cell.IsHitTestVisible = true;
		cell.Focusable = true;
		cell.FontSize = SideScrollTheme.DataGridFontSize;
		cell.BorderThickness = new Thickness(0, 0, 1, 1); // Right and Bottom

		if (Binding != null)
		{
			textBlockElement.Bind(TextBlock.TextProperty, Binding);
		}
		return textBlockElement;
	}

	[MemberNotNull(nameof(FormattedBinding))]
	private Binding GetFormattedTextBinding()
	{
		Binding binding = Binding as Binding ?? new Binding(PropertyInfo.Name);

		if (FormattedBinding == null)
		{
			FormattedBinding = new Binding
			{
				Path = binding.Path,
				Mode = BindingMode.Default,
			};
			if (IsReadOnly)
			{
				FormattedBinding.Converter = FormatConverter;
			}
		}

		return FormattedBinding;
	}*/

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

	/*private Type GetBindingType(object dataItem)
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
	}*/
}
