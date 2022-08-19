using Atlas.Core;
using System.Collections;

namespace Atlas.Tabs;

public class TabItemCollection
{
	public IList List;
	public IEnumerable? Filtered; // CollectionView takes filters into account

	private HashSet<object> _objects = new();
	private Dictionary<string, object> _keys = new();

	public override string? ToString() => List.ToString();

	public TabItemCollection(IList list, IEnumerable? filtered = null)
	{
		List = list;
		Filtered = filtered;
	}

	public List<object> GetSelectedObjects(HashSet<SelectedRow> selectedRows)
	{
		var rowObjects = new List<object>();
		if (selectedRows.Count == 0)
			return rowObjects;

		foreach (object obj in Filtered ?? List)
		{
			if (obj == null)
				continue;

			_objects.Add(obj);

			string? id = ObjectUtils.GetObjectId(obj);
			if (id != null)
				_keys.TryAdd(id, obj);
		}

		foreach (SelectedRow selectedRow in selectedRows)
		{
			object? selectedObject = GetMatchingObject(selectedRow);
			if (selectedObject == null)
				continue;

			rowObjects.Add(selectedObject);
		}

		return rowObjects;
	}

	private object? GetMatchingObject(SelectedRow selectedRow)
	{
		if (selectedRow.Object != null && _objects.Contains(selectedRow.Object))
			return selectedRow.Object;

		// Try to find a matching Row Index and Key first
		int rowIndex = selectedRow.RowIndex;
		if (rowIndex >= 0 && rowIndex < List.Count)
		{
			object rowObject = List[rowIndex]!;
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
