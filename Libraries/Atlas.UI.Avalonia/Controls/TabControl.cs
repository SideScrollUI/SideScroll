using Atlas.Tabs;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;

namespace Atlas.UI.Avalonia.Controls;

public class TabControl : Grid
{
	public TabInstance TabInstance;

	public TabControl(TabInstance tabInstance)
	{
		TabInstance = tabInstance;

		InitializeControls();
	}

	private void InitializeControls()
	{
		//VerticalAlignment = VerticalAlignment.Stretch;
		HorizontalAlignment = HorizontalAlignment.Stretch;

		ColumnDefinitions = new ColumnDefinitions("Auto,*");

		Margin = new Thickness(15);

		MinWidth = 100;
		MaxWidth = 1000;
	}

	/*public IList SelectedItemsOrdered
	{
		get
		{
			var orderedItems = new SortedDictionary<int, object>();
			foreach (DataGridViewCell cell in dataGridView.SelectedCells)
			{
				orderedItems.Add(cell.RowIndex, cell.OwningRow.DataBoundItem);
			}
			return orderedItems.Values.ToList();
		}
	}

	private void dataGridView_SelectionChanged(object sender, EventArgs e)
	{
		OnSelectionChanged?.Invoke(this, null);
	}

	*/
}

/*
Derive most controls from this?
*/
