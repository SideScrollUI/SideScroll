using System;
using System.Collections.Generic;
using System.Text;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestDataGrid : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load()
			{
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Collection Size", new TabTestGridCollectionSize()),
					new ListItem("Enumerable", new TabTestGridHashSet()),
					new ListItem("Array", new TabTestArray()),
					new ListItem("Nullable Array", new TabTestNullableArray()),
					new ListItem("Memory", new TabTestMemory()),
					new ListItem("Object Properties", new TabTestObjectProperties()),
					new ListItem("Wide Columns", new TabTestWideColumns()),
					new ListItem("Column Types", new TabTestColumnTypes()),
					new ListItem("Column Ordering", new TabTestColumnOrdering()),
					new ListItem("Filter", new TabTestFilter()),
				};
			}
		}
	}
}
/*

*/
