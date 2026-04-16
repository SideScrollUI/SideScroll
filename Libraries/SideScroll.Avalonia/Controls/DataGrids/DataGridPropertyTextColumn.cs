using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using SideScroll.Attributes;
using SideScroll.Avalonia.Controls.Converters;
using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Utilities;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.DataGrids;

/// <summary>
/// A data-grid text column bound to a reflected <see cref="PropertyInfo"/>, with configurable width constraints, word-wrap,
/// auto-sizing, cell styling, foreground/background brush converters, and a context menu.
/// </summary>
public class DataGridPropertyTextColumn : DataGridTextColumn
{
	/// <summary>Gets or sets the minimum string length above which word-wrap is enabled automatically. Defaults to 64.</summary>
	public static int EnableWordWrapMinStringLength { get; set; } = 64;

	/// <summary>Gets or sets the maximum number of rows scanned when inferring column attributes from item values. Defaults to 30.</summary>
	public static int MaxRowScanProperties { get; set; } = 30;

	/// <summary>Gets the reflected property this column is bound to.</summary>
	public PropertyInfo PropertyInfo { get; }

	/// <summary>Gets or sets the minimum desired column width in pixels. Defaults to 25.</summary>
	public int MinDesiredWidth { get; set; } = 25;

	/// <summary>Gets the maximum desired column width in pixels.</summary>
	public int MaxDesiredWidth { get; }

	/// <summary>Gets the maximum desired cell height in pixels. Defaults to 100.</summary>
	public int MaxDesiredHeight { get; } = 100;

	/// <summary>Gets or sets whether the column width automatically adjusts to fit cell content.</summary>
	public bool AutoSize { get; set; }

	/// <summary>Gets or sets whether cell text wraps when it exceeds the column width.</summary>
	public bool WordWrap { get; set; }

	/// <summary>Gets or sets whether any column in the grid uses cell styling, requiring this column to manually draw horizontal lines.</summary>
	public bool StyleCells { get; set; }

	/// <summary>Gets or sets the formatted binding used to display cell values via <see cref="FormatConverter"/>.</summary>
	public Binding FormattedBinding { get; set; }
	//private Binding unformattedBinding;
	/// <summary>Gets the value converter used to format cell content for display.</summary>
	public FormatValueConverter FormatConverter { get; } = new();

	/// <summary>Returns the reflected property name.</summary>
	public override string ToString() => PropertyInfo.Name;

	public DataGridPropertyTextColumn(PropertyInfo propertyInfo, bool isReadOnly, int maxDesiredWidth)
	{
		PropertyInfo = propertyInfo;
		IsReadOnly = isReadOnly;
		MaxDesiredWidth = maxDesiredWidth;

		Binding = GetFormattedTextBinding();

		var maxHeightAttribute = propertyInfo.GetCustomAttribute<MaxHeightAttribute>();
		if (maxHeightAttribute != null)
		{
			MaxDesiredHeight = maxHeightAttribute.MaxHeight;
			FormatConverter.MaxLength = MaxDesiredHeight * 10;
		}
		FormatConverter.IsFormatted = (propertyInfo.GetCustomAttribute<FormattedAttribute>() != null);

		var formatterAttribute = propertyInfo.GetCustomAttribute<FormatterAttribute>();
		if (formatterAttribute != null)
		{
			FormatConverter.Formatter = (ICustomFormatter)Activator.CreateInstance(formatterAttribute.Type)!;
		}

		if (TableUtils.IsTypeAutoSize(propertyInfo.PropertyType))
		{
			AutoSize = true;
		}

		CanUserSort = TableUtils.IsTypeSortable(propertyInfo.PropertyType);

		WordWrap = (PropertyInfo.GetCustomAttribute<WordWrapAttribute>() != null);

		//CellStyleClasses = new Classes()
	}

	/// <summary>Scans the first <see cref="MaxRowScanProperties"/> items to infer <see cref="HideAttribute"/> visibility and enable word-wrap for long strings.</summary>
	public void ScanItemAttributes(IList list)
	{
		bool checkWordWrap = (!WordWrap && (PropertyInfo.PropertyType == typeof(string) || PropertyInfo.PropertyType == typeof(object)));

		var hideAttribute = PropertyInfo.GetCustomAttribute<HideAttribute>();

		if (checkWordWrap || hideAttribute != null)
		{
			if (hideAttribute != null && list.Count > 0)
			{
				IsVisible = false;
			}

			for (int i = 0; i < MaxRowScanProperties && i < list.Count; i++)
			{
				object? obj = list[i];
				if (obj == null)
					continue;

				try
				{
					object? value = PropertyInfo.GetValue(obj); // Can throw exception
					if (hideAttribute != null)
					{
						if (!hideAttribute.Values.Contains(value))
						{
							IsVisible = true;
						}
					}

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

	protected override Control GenerateElement(DataGridCell cell, object dataItem)
	{
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
		{
			TextBlock textBlock = CreateTextBlock(cell);

			if (StyleCells)
			{
				return AddStyling(cell, textBlock);
			}

			return textBlock;
		}
	}

	// Styled columns have a different line color, so we have to draw them manually
	// They also use different background colors, with different shades for links vs non-links
	private TextBlock AddStyling(DataGridCell cell, TextBlock textBlock)
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
			cell.Bind(TemplatedControl.BackgroundProperty, binding);

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
	}

	protected TextBlock CreateTextBlock(DataGridCell cell)
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
	}
}
