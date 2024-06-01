using Atlas.Core;

namespace Atlas.Tabs.Samples.DataGrid;

[ListItem]
public class TabSampleDataGrid
{
	public static TabSampleGridColumnTypes ColumnTypes => new();
	public static TabSampleGridColumnSizing ColumnSizing => new();
	public static TabSampleGridColumnOrdering ColumnOrdering => new();
	public static TabSampleGridColumnCount ColumnCount => new();
	public static TabSampleWideColumns WideColumns => new();
	public static TabSampleGridCollectionSize CollectionSize => new();
	public static TabSampleGridHashSet Enumerable => new();
	public static TabSampleGridDictionary Dictionary => new();
	public static TabSampleArray Array => new();
	public static TabSampleNullableArray NullableArray => new();
	public static TabSampleMemory Memory => new();
	public static TabSampleInstanceListItems InstanceListItems => new();
	public static TabSampleFilter Filter => new();
}
