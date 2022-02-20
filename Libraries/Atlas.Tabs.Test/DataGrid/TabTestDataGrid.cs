using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid;

[ListItem]
public class TabTestDataGrid
{
	public TabTestGridCollectionSize CollectionSize => new();
	public TabTestGridHashSet Enumerable => new();
	public TabTestGridDictionary Dictionary => new();
	public TabTestArray Array => new();
	public TabTestNullableArray NullableArray => new();
	public TabTestMemory Memory => new();
	public TabTestObjectProperties ObjectProperties => new();
	public TabTestInstanceListItems InstanceListItems => new();
	public TabTestWideColumns WideColumns => new();
	public TabTestGridColumnTypes ColumnTypes => new();
	public TabTestGridColumnOrdering ColumnOrdering => new();
	public TabTestGridColumnCount ColumnCount => new();
	public TabTestFilter Filter => new();
}
