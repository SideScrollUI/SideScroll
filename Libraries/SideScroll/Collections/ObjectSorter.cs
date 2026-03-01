using System.Collections;
using System.ComponentModel;

namespace SideScroll.Collections;

/// <summary>
/// A custom comparer that handles comparison of various object types including primitives, strings, collections, and IComparable objects
/// </summary>
public class CustomComparer : IComparer
{
	/// <summary>
	/// Compares two objects and returns a value indicating their relative sort order
	/// </summary>
	public virtual int Compare(object? x, object? y)
	{
		if (x == null && y == null)
			return 0;
		if (x == null)
			return -1;
		if (y == null)
			return 1;

		Type xType = x.GetType();
		Type yType = y.GetType();
		if (xType != yType)
			return 0;

		if (x is string xString && y is string yString)
		{
			return string.CompareOrdinal(xString, yString);// , true); // performance hit?
		}

		if (x is ICollection xCollection && y is ICollection yCollection)
		{
			if (xCollection.Count == yCollection.Count)
				return 0;
			if (xCollection.Count < yCollection.Count)
				return -1;
			return 1;
		}

		if (x is IComparable xComparable)
			return xComparable.CompareTo(y);

		if (xType.IsPrimitive || xType.IsEnum)
		{
			if ((dynamic)x == (dynamic)y)
				return 0;
			if ((dynamic)x < (dynamic)y)
				return -1;
			return 1;
		}

		return string.CompareOrdinal(x.ToString(), y.ToString());
	}
}

/// <summary>
/// Extends CustomComparer to support property-based sorting with sort direction
/// </summary>
public class ObjectSorter : CustomComparer
{
	/// <summary>
	/// Gets or sets the collection of sort descriptions containing the properties to sort by
	/// </summary>
	public ListSortDescriptionCollection? Sorts { get; set; }

	/// <summary>
	/// Gets or sets the sort direction (Ascending or Descending)
	/// </summary>
	public ListSortDirection SortDirection { get; set; }

	/// <summary>
	/// Compares two objects based on the specified property descriptor and sort direction
	/// </summary>
	public override int Compare(object? x, object? y)
	{
		PropertyDescriptor descriptor = Sorts![0]!.PropertyDescriptor!;
		object? xKey = descriptor.GetValue(x);
		object? yKey = descriptor.GetValue(y);

		if (Equals(xKey, yKey))
			return 0;

		int comparison = base.Compare(xKey, yKey);

		if (SortDirection == ListSortDirection.Descending)
		{
			return -comparison;
		}

		return comparison;
	}
}
