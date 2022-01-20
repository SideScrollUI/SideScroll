using Atlas.Core;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Tabs;

[PublicData]
public class TabViewSettings
{
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
					address += selectedRow.Label;
					comma = ", ";
					count++;
				}
			}
			if (count > 1)
				address += "[" + address + "] ";

			return address;
		}
	}

	public bool NotesVisible { get; set; }

	public double? SplitterDistance { get; set; }

	public List<TabDataSettings> TabDataSettings { get; set; } = new();

	public List<TabDataSettings> ChartDataSettings { get; set; } = new(); // for the Chart's internal Data List

	public List<SelectedRow> SelectedRows => TabDataSettings?.SelectMany(d => d.SelectedRows).ToList() ?? new();

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

	public override string ToString() => Address;

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

	/*public override string ToString()
	{
		var strings = new List<string>();
		TabDataSettings.ForEach(p => strings.Add(p.Formatted()));
		return string.Join(",", strings);
	}*/
}
/*
Type of control
Name of control
	Usually a reference
*/
