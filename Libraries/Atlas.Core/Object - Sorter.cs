using System;
using System.Collections;
using System.ComponentModel;

namespace Atlas.Core
{
	public class CustomComparer : IComparer
	{
		public virtual int Compare(object x, object y)
		{
			if (x == null && y == null)
				return 0;
			else if (x == null)
				return -1;
			else if (y == null)
				return 1;

			Type xType = x.GetType();
			Type yType = y.GetType();
			if (xType != yType)
				return 0;

			if (x is string)
			{
				return string.Compare((string)x, (string)y);// , true); // performance hit?
			}

			if (x is ICollection)
			{
				if (((ICollection)x).Count == ((ICollection)y).Count)
					return 0;
				else if (((ICollection)x).Count < ((ICollection)y).Count)
					return -1;
				else
					return 1;
			}

			if (x is IComparable)
				return ((IComparable)x).CompareTo(y);

			if (xType.IsPrimitive || xType.IsEnum)
			{
				if ((dynamic)x == (dynamic)y)
					return 0;
				else if ((dynamic)x < (dynamic)y)
					return -1;
				else
					return 1;
			}

			return string.Compare(x.ToString(), y.ToString());
		}
	}

	public class Sorter : CustomComparer
	{
		public ListSortDescriptionCollection sorts;
		public ListSortDirection SortDirection;
		
		public override int Compare(object x, object y)
		{
			PropertyDescriptor descriptor = sorts[0].PropertyDescriptor;
			object xKey = descriptor.GetValue(x);
			object yKey = descriptor.GetValue(y);

			if (object.Equals(xKey, yKey))
				return 0;

			int comparison = base.Compare(xKey, yKey);

			if (SortDirection == ListSortDirection.Descending)
				comparison = -comparison;

			return comparison;
		}
	}
}
