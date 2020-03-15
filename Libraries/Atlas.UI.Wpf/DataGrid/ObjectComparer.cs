using Atlas.Core;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Atlas.UI.Wpf
{
	public class DataGridSortComparer : CustomComparer
	{
		private ListSortDirection sortDirection;
		//private string propertyName;
		private PropertyInfo propertyInfo;

		public DataGridSortComparer(PropertyInfo propertyInfo, ListSortDirection sortDirection)
		{
			this.propertyInfo = propertyInfo; // does this work with derived properties? is it worth the speed tradeoff to not use this?
			this.sortDirection = sortDirection;
		}

		public override int Compare(object x, object y)
		{
			//if (propertyInfo == null)
			//	propertyInfo = x.GetType().GetProperty(propertyName);

			object value1 = propertyInfo.GetValue(x);
			object value2 = propertyInfo.GetValue(y);

			int result = base.Compare(value1, value2);
			if (sortDirection == ListSortDirection.Descending)
			{
				result = -result;
			}
			return result;
		}
	}
}
