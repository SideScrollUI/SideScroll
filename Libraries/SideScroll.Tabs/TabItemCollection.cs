using SideScroll.Core.Utilities;
using System.Collections;

namespace SideScroll.Tabs;

public class TabItemCollection(IList list, IEnumerable? filtered = null)
{
	public IList List = list;
	public IEnumerable? Filtered = filtered; // CollectionView takes filters into account

	private HashSet<object> _objects = [];
	private Dictionary<string, object> _keys = [];

	public override string? ToString() => List.ToString();

	public List<object> GetSelectedObjects(HashSet<SelectedRow> selectedRows)
	{
		var rowObjects = new List<object>();
		var items = Filtered ?? List;
		if (selectedRows.Count == 0 || items == null)
			return rowObjects;

		foreach (object obj in items)
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
