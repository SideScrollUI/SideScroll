using System;
using System.Windows;
using System.Windows.Controls;

namespace Atlas.UI.Wpf
{
	public class BaseDataGrid : DataGrid
	{
		protected override Size MeasureOverride(Size availableSize)
		{
			var desiredSize = base.MeasureOverride(availableSize);
			ClearBindingGroup();
			return desiredSize;
		}

		// Fix Cell edits not taking effect until Row is changed
		private void ClearBindingGroup()
		{
			// Clear ItemBindingGroup so it isn't applied to new rows
			ItemBindingGroup = null;
			// Clear BindingGroup on already created rows
			foreach (var item in Items)
			{
				var row = ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
				if (row != null)
					row.BindingGroup = null;
			}
		}
	}
}
