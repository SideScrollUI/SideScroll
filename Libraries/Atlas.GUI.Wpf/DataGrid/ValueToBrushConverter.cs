using Atlas.Core;
using Atlas.Tabs;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Atlas.GUI.Wpf
{
	public class ValueToBrushConverter : IValueConverter
	{
		public const string ColorHasChildren = "#f4c68d";
		public const string ColorEditable = "#9fe16b";

		public SolidColorBrush HasChildrenBrush { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorHasChildren));
		public SolidColorBrush EditableBrush { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorEditable));

		public bool Editable { get; set; } = false;

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			DataGridCell dataGridCell = (DataGridCell)value;
			object obj = dataGridCell.DataContext;
			try
			{
				if (dataGridCell.Column.DisplayIndex == 1 && (obj is ListItem || obj is ListMember))
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
			}
			catch (InvalidCastException)
			{
			}

			if (dataGridCell.Column.IsReadOnly)
				return Brushes.White; // checkbox column requires a valid value
			else
				return EditableBrush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}