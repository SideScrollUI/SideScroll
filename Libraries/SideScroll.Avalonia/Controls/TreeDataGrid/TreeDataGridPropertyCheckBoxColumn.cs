using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using SideScroll.Avalonia.Utilities;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.TreeDataGrids;

public class TreeDataGridPropertyCheckBoxColumn<TModel> : CheckBoxColumn<TModel>
	where TModel : class
{
	public TreeDataGrid DataGrid { get; init; }
	public PropertyInfo PropertyInfo { get; set; }

	public int MinDesiredWidth { get; set; } = 25;
	public int MaxDesiredWidth { get; set; } = 500;

	//public bool Editable { get; set; } = false;
	public bool StyleCells { get; set; } // True if any column has a Style applied, so we can manually draw the horizontal lines

	public override string ToString() => PropertyInfo.Name;

	public TreeDataGridPropertyCheckBoxColumn(
		TreeDataGrid dataGrid, 
		string label, 
		PropertyInfo propertyInfo,
		bool isReadOnly, 
		int maxDesiredWidth, 
		GridLength? gridLength = null)
		: base(label, x => (bool?)propertyInfo.GetValue(x), null, gridLength) 
	{
		DataGrid = dataGrid;
		PropertyInfo = propertyInfo;
		//IsReadOnly = isReadOnly;
		MaxDesiredWidth = maxDesiredWidth;
		Options.MaxWidth = new(MaxDesiredWidth);

		Options.CanUserSortColumn = DataGridUtils.IsTypeSortable(propertyInfo.PropertyType);
	}
}
