using Atlas.Core;
using Atlas.Tabs;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections;
using System.Reflection;

namespace Atlas.UI.Avalonia
{
	public class ValueToBrushConverter : IValueConverter
	{
		private PropertyInfo propertyInfo;

		public ValueToBrushConverter(PropertyInfo propertyInfo)
		{
			this.propertyInfo = propertyInfo;
		}

		public sealed class BrushColors
		{
			public ISolidColorBrush HasLinks => Theme.HasLinks;
			public ISolidColorBrush NoLinks => Theme.NoLinks;
			public ISolidColorBrush Editable { get; set; } = Theme.Editable;
		}
		internal static BrushColors StyleBrushes { get; set; } = new BrushColors();

		public bool Editable { get; set; } = false;

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				if (value is DictionaryEntry || propertyInfo.IsDefined(typeof(StyleValueAttribute)))
				{
					bool hasLinks = TabModel.ObjectHasLinks(value, true);
					if (hasLinks)
						return StyleBrushes.HasLinks;
					else if (Editable && value is ListMember listMember && listMember.Editable)
						return StyleBrushes.Editable;
					else
						return StyleBrushes.NoLinks;
				}
			}
			catch (InvalidCastException)
			{
			}

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
						return Editable;
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
Used by DataGridPropertyTextColumn
Need to hook this into Cell.OnPaint for hover?
*/
