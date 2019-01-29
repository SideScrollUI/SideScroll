using Avalonia.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/*namespace Atlas.GUI.Avalonia.Controls.DataGrid
{
	public class CustomSortDescription : AvaloniaSortDescription
	{
		private readonly bool _descending;
		private readonly string _propertyPath;
		private readonly Lazy<CultureSensitiveComparer> _cultureSensitiveComparer;
		private readonly Lazy<IComparer<object>> _comparer;
		private Type _propertyType;
		private IComparer _internalComparer;
		private IComparer<object> _internalComparerTyped;
		private IComparer<object> InternalComparer
		{
			get
			{
				if (_internalComparerTyped == null && _internalComparer != null)
				{
					if (_internalComparerTyped is IComparer<object> c)
						_internalComparerTyped = c;
					else
						_internalComparerTyped = Comparer<object>.Create((x, y) => _internalComparer.Compare(x, y));
				}

				return _internalComparerTyped;
			}
		}

		public override string PropertyPath => _propertyPath;
		public override IComparer<object> Comparer => _comparer.Value;
		public override bool Descending => _descending;

		public CustomSortDescription(string propertyPath, bool descending, CultureInfo culture)
		{
			_propertyPath = propertyPath;
			_descending = descending;
			_cultureSensitiveComparer = new Lazy<CultureSensitiveComparer>(() => new CultureSensitiveComparer(culture ?? CultureInfo.CurrentCulture));
			_comparer = new Lazy<IComparer<object>>(() => Comparer<object>.Create((x, y) => Compare(x, y)));
		}
		private CustomSortDescription(CustomSortDescription inner, bool descending)
		{
			_propertyPath = inner._propertyPath;
			_descending = descending;
			_propertyType = inner._propertyType;
			_cultureSensitiveComparer = inner._cultureSensitiveComparer;
			_internalComparer = inner._internalComparer;
			_internalComparerTyped = inner._internalComparerTyped;

			_comparer = new Lazy<IComparer<object>>(() => Comparer<object>.Create((x, y) => Compare(x, y)));
		}

		private object GetValue(object o)
		{
			if (o == null)
				return null;

			if (HasPropertyPath)
				return InvokePath(o, _propertyPath, _propertyType);

			if (_propertyType == o.GetType())
				return o;
			else
				return null;
		}

		private IComparer GetComparerForType(Type type)
		{
			if (type == typeof(string))
				return _cultureSensitiveComparer.Value;
			else
				return (typeof(Comparer<>).MakeGenericType(type).GetProperty("Default")).GetValue(null, null) as IComparer;
		}
		private Type GetPropertyType(object o)
		{
			return o.GetType().GetNestedPropertyType(_propertyPath);
		}

		private int Compare(object x, object y)
		{
			int result = 0;

			if (_propertyType == null)
			{
				if (x != null)
				{
					_propertyType = GetPropertyType(x);
				}
				if (_propertyType == null && y != null)
				{
					_propertyType = GetPropertyType(y);
				}
			}

			object v1 = GetValue(x);
			object v2 = GetValue(y);

			if (_propertyType != null && _internalComparer == null)
				_internalComparer = GetComparerForType(_propertyType);

			result = _internalComparer?.Compare(v1, v2) ?? 0;

			if (_descending)
				return -result;
			else
				return result;
		}

		internal override void Initialize(Type itemType)
		{
			base.Initialize(itemType);

			if (_propertyType == null)
				_propertyType = itemType.GetNestedPropertyType(_propertyPath);
			if (_internalComparer == null && _propertyType != null)
				_internalComparer = GetComparerForType(_propertyType);
		}
		public override IOrderedEnumerable<object> OrderBy(IEnumerable<object> seq)
		{
			if (_descending)
			{
				return seq.OrderByDescending(o => GetValue(o), InternalComparer);
			}
			else
			{
				return seq.OrderBy(o => GetValue(o), InternalComparer);
			}
		}
		public override IOrderedEnumerable<object> ThenBy(IOrderedEnumerable<object> seq)
		{
			if (_descending)
			{
				return seq.ThenByDescending(o => GetValue(o), InternalComparer);
			}
			else
			{
				return seq.ThenByDescending(o => GetValue(o), InternalComparer);
			}
		}

		internal override AvaloniaSortDescription SwitchSortDirection()
		{
			return new CustomSortDescription(this, !_descending);
		}
	}
}*/
