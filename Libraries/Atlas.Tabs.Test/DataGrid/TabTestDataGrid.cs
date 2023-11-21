using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid;

[ListItem]
public class TabTestDataGrid
{
	public static TabTestGridCollectionSize CollectionSize => new();
	public static TabTestGridHashSet Enumerable => new();
	public static TabTestGridDictionary Dictionary => new();
	public static TabTestArray Array => new();
	public static TabTestNullableArray NullableArray => new();
	public static TabTestMemory Memory => new();
	public static TabTestObjectProperties ObjectProperties => new();
	public static TabTestInstanceListItems InstanceListItems => new();
	public static TabTestWideColumns WideColumns => new();
	public static TabTestGridColumnTypes ColumnTypes => new();
	public static TabTestGridColumnSizing ColumnSizing => new();
	public static TabTestGridColumnOrdering ColumnOrdering => new();
	public static TabTestGridColumnCount ColumnCount => new();
	public static TabTestFilter Filter => new();
}
