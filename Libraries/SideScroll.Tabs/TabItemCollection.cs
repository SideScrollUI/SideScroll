using SideScroll.Tabs.Settings;
using SideScroll.Utilities;
using System.Collections;

namespace SideScroll.Tabs;

public class TabItemCollection
{
	public IList Items { get; set; }
	public IEnumerable? Filtered { get; set; } // CollectionView takes filters into account

	private HashSet<object> _objects = [];
	private Dictionary<string, object> _keys = [];

	public override string? ToString() => Items.ToString();

	public TabItemCollection(IList items, IEnumerable? filtered = null)
	{
		Items = items;
		Filtered = filtered;

		UpdateIndices();
	}

	public void UpdateIndices()
	{
		_objects = [];
		_keys = [];

		var items = Filtered ?? Items;
		if (items == null)
			return;

		foreach (object obj in items)
		{
			if (obj == null)
				continue;

			_objects.Add(obj);

			string? id = ObjectUtils.GetObjectId(obj);
			if (id != null)
			{
				_keys.TryAdd(id, obj);
			}
		}
	}

	public List<object> GetSelectedObjects(HashSet<SelectedRow> selectedRows)
	{
		if (selectedRows.Count == 0 || _objects.Count == 0)
			return [];

		return selectedRows
			.Select(GetMatchingObject)
			.OfType<object>()
			.ToList();
	}

	public object? GetMatchingObject(SelectedRow selectedRow)
	{
		if (selectedRow.Object != null && _objects.Contains(selectedRow.Object))
			return selectedRow.Object;

		// Try to find a matching Row Index and Key first
		if (selectedRow.RowIndex is int rowIndex && rowIndex >= 0 && rowIndex < Items.Count)
		{
			object rowObject = Items[rowIndex]!;
			var currentSelectedRow = new SelectedRow(rowObject);
			if (currentSelectedRow.Equals(selectedRow))
				return rowObject;
		}

		if (selectedRow.DataKey != null)
		{
			if (_keys.TryGetValue(selectedRow.DataKey, out object? matchingObject))
				return matchingObject;
		}

		if (selectedRow.Label != null)
		{
			// These can be user generated
			if (_keys.TryGetValue(selectedRow.Label, out object? matchingObject))
				return matchingObject;
		}

		return null;
	}
}
