using System.Collections.Generic;

namespace Atlas.Tabs
{
	public class TabViewSettings
	{
		public string Name { get; set; }
		public string Address
		{
			get
			{
				string address = "";
				string comma = "";
				int count = 0;
				foreach (TabDataSettings tabDataSettings in TabDataSettings)
				{
					foreach (SelectedRow selectedRow in tabDataSettings.SelectedRows)
					{
						address += comma;
						address += selectedRow.label;
						comma = ", ";
						count++;
					}
				}
				if (count > 1)
					address += "[" + address + "] ";

				return address;
			}
		}
		public bool NotesVisible { get; set; } = false;
		public double? SplitterDistance { get; set; }

		public List<TabDataSettings> TabDataSettings { get; set; } = new List<TabDataSettings>();

		// change to string id?
		public TabDataSettings GetData(int index)
		{
			// Creates new Settings if necessary
			while (TabDataSettings.Count <= index)
			{
				TabDataSettings.Add(new TabDataSettings());
			}
			return TabDataSettings[index];
		}

		public List<TabDataSettings> ChartDataSettings { get; set; } = new List<TabDataSettings>(); // for the Chart's internal Data List

		// Is this useful? Remove?
		public List<SelectedRow> SelectedRows
		{
			get
			{
				var selectedRows = new List<SelectedRow>();
				if (TabDataSettings == null)
					return selectedRows;

				foreach (TabDataSettings dataSettings in TabDataSettings)
					selectedRows.AddRange(dataSettings.SelectedRows);

				return selectedRows;
			}
		}

		// Store Skipped bool instead?
		public SelectionType SelectionType
		{
			get
			{
				if (TabDataSettings == null)
					return SelectionType.None;

				foreach (TabDataSettings dataSettings in TabDataSettings)
				{
					if (dataSettings.SelectionType != SelectionType.None)
						return dataSettings.SelectionType;
				}

				return SelectionType.None;
			}
		}

		/*public override string ToString()
		{
			var strings = new List<string>();
			TabDataSettings.ForEach(p => strings.Add(p.Formatted()));
			return string.Join(",", strings);
		}*/
	}
}
/*
Type of control
Name of control
	Usually a reference
*/