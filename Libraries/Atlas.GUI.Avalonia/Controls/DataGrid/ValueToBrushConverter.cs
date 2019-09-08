using Atlas.Core;
using Atlas.Tabs;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;

namespace Atlas.GUI.Avalonia
{
	public class ValueToBrushConverter : IValueConverter
	{
		public const string ColorHasChildren = "#f4c68d";

		public SolidColorBrush HasChildrenBrush { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorHasChildren));
		public SolidColorBrush EditableBrush { get; set; } = new SolidColorBrush(Theme.EditableColor);

		public bool Editable { get; set; } = false;

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			//DataGridCell dataGridCell = (DataGridCell)value;
			try
			{
				if ((value is ListItem || value is ListMember)) // dataGridCell.DisplayIndex == 1 && 
				{
					bool hasChildren = TabModel.ObjectHasChildren(value, true);
					if (hasChildren)
						return HasChildrenBrush;
					//return Brushes.Moccasin;
					else if (Editable && (value is ListMember) && ((ListMember)value).Editable)
						return EditableBrush;
					else
						return Brushes.LightGray;
				}
			}
			catch (InvalidCastException)
			{
			}

			//if (dataGridCell.OwningColumn.IsReadOnly)
			return null;
			//	return Brushes.White; // checkbox column requires a valid value
			//else
			//	return EditableBrush;
		}

		// todo: set default background brush to white so context menu's work, hover breaks if it's set though
		/*private IBrush GetCellBrush(DataGridCell dataGridCell, object dataItem)
		{
			object obj = dataGridCell.DataContext;
			try
			{
				if (propertyInfo.IsDefined(typeof(StyleValueAttribute)))
				//if (this.DisplayIndex == 1 && (dataItem is ListItem || dataItem is ListMember))
				{
					bool hasChildren = TabModel.ObjectHasChildren(dataItem, true);
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
		}*/

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
/*
Not used
FormattedTextColumn used instead for now
Need to hook this into Cell.OnPaint for hover?
*/
