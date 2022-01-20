using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid;

public class TabTestDataGrid : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new ItemCollection<ListItem>()
				{
					new("Collection Size", new TabTestGridCollectionSize()),
					new("Enumerable", new TabTestGridHashSet()),
					new("Dictionary", new TabTestGridDictionary()),
					new("Array", new TabTestArray()),
					new("Nullable Array", new TabTestNullableArray()),
					new("Memory", new TabTestMemory()),
					new("Object Properties", new TabTestObjectProperties()),
					new("Instance List Items", new TabTestInstanceListItems()),
					new("Wide Columns", new TabTestWideColumns()),
					new("Column Types", new TabTestGridColumnTypes()),
					new("Column Ordering", new TabTestGridColumnOrdering()),
					new("Column Count", new TabTestGridColumnCount()),
					new("Filter", new TabTestFilter()),
				};
		}
	}
}
