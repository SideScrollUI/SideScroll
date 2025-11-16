using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using System.Data;

namespace SideScroll.Tabs.Samples.DataGrid;

// DataTable's can be used to show dynamic columns for a DataGrid
// For structured data with known schema's, it's recommended to DataBind directly to the items instead of using a DataTable
public class TabSampleGridDataTable : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance, ITabCreator
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

		public object CreateControl(object value, out string? label)
		{
			label = value.ToString();
			if (value is DataRowView dataRowView)
			{
				return dataRowView.Row.ItemArray
					.WithIndex()
					.Select(x => new ListPair(dataRowView.DataView.Table!.Columns[x.index], x.item))
					.ToList();
			}

			return value.ToString()!;
		}
	}
}
