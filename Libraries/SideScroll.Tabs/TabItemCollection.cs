using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Utilities;
using System.Collections;

namespace SideScroll.Tabs;

/// <summary>
/// Manages a collection of items for tab views, providing indexing and selection capabilities
/// </summary>
public class TabItemCollection
{
	/// <summary>
	/// The complete list of items in the collection
	/// </summary>
	public IList Items { get; set; }
	
	/// <summary>
	/// The filtered view of items. CollectionView takes filters into account
	/// </summary>
	public IEnumerable? Filtered { get; set; }

	private HashSet<object> _objects = [];
	private Dictionary<string, object> _keys = [];

	public override string? ToString() => Items.ToString();

	/// <summary>
	/// Creates a new TabItemCollection with the specified items and optional filtered view
	/// </summary>
	public TabItemCollection(IList items, IEnumerable? filtered = null)
	{
		Items = items;
		Filtered = filtered;

		UpdateIndices();
	}

	/// <summary>
	/// Updates the internal indices and key mappings for items in the collection
	/// </summary>
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

	/// <summary>
	/// Retrieves the objects corresponding to the selected rows
	/// </summary>
	public List<object> GetSelectedObjects(HashSet<SelectedRow> selectedRows)
	{
		if (selectedRows.Count == 0 || _objects.Count == 0)
			return [];

		return selectedRows
			.Select(GetMatchingObject)
			.OfType<object>()
			.ToList();
	}

	/// <summary>
	/// Finds the object that matches the specified selected row by comparing object reference, row index, data key, or label
	/// </summary>
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
