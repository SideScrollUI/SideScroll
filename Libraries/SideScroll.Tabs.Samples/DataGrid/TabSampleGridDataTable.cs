using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using System.Data;

namespace SideScroll.Tabs.Samples.DataGrid;

// DataTable's can be used to show dynamic columns for a DataGrid
// For structured data with known schema's, it's recommended to DataBind directly to the items instead of using a DataTable
public class TabSampleGridDataTable : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance, ITabCreator
	{
		private DataTable? _dataTable;

		public override void Load(Call call, TabModel model)
		{
			_dataTable = new();
			_dataTable.Columns.Add(new DataColumn("Id", typeof(int)));
			_dataTable.Columns.Add(new DataColumn("Name", typeof(string)));
			AddEntries();
			model.AddData(_dataTable);
		}

		private void AddEntries()
		{
			for (int i = 0; i < 10; i++)
			{
				_dataTable!.Rows.Add(i, $"Name {i}");
			}
		}

		public object CreateControl(object obj, out string? label)
		{
			label = obj.ToString();
			if (obj is DataRowView dataRowView)
			{
				return dataRowView.Row.ItemArray
					.WithIndex()
					.Select(x => new ListPair(dataRowView.DataView.Table!.Columns[x.index], x.item))
					.ToList();
			}

			return obj.ToString()!;
		}
	}
}
