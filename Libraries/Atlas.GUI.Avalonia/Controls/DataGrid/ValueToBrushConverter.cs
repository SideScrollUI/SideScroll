using Atlas.Core;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;

namespace Atlas.GUI.Avalonia
{
	/*public class ValueToBrushConverter : IValueConverter
	{
		public const string ColorHasChildren = "#f4c68d";

		public SolidColorBrush HasChildrenBrush { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorHasChildren));
		public SolidColorBrush EditableBrush { get; set; } = new SolidColorBrush(Theme.EditableColor);

		public bool Editable { get; set; } = false;

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			DataGridCell dataGridCell = (DataGridCell)value;
			object obj = dataGridCell.DataContext;
			try
			{
				if (dataGridCell.DisplayIndex == 1 && (obj is ListItem || obj is ListMember))
				{
					bool hasChildren = TabModel.ObjectHasChildren(obj);
					if (hasChildren)
						return HasChildrenBrush;
						//return Brushes.Moccasin;
					else if (Editable && (obj is ListMember) && ((ListMember)obj).Editable)
						return EditableBrush;
					else
						return Brushes.LightGray;
				}
			catch (InvalidCastException)
			{
			}

			//if (dataGridCell.OwningColumn.IsReadOnly)
			//	return Brushes.White; // checkbox column requires a valid value
			//else
				return EditableBrush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}*/
}
/*
Not used
FormattedTextColumn used instead for now
Need to hook this into Cell.OnPaint for hover?
*/
