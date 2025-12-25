using SideScroll.Attributes;
using System.Text.Json.Serialization;

namespace SideScroll.Tabs.Settings;

[PublicData]
public class TabViewSettings
{
	public string? Address
	{
		get
		{
			if (TabDataSettings == null) return null;

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
			{
				address += "[" + address + "] ";
			}

			return address;
		}
	}

	public double? SplitterDistance { get; set; }

	public List<TabDataSettings> TabDataSettings { get; set; } = [];

	[JsonIgnore]
	public List<SelectedRow> SelectedRows => TabDataSettings?.SelectMany(d => d.SelectedRows).ToList() ?? [];

	// Store Skipped bool instead?
	public SelectionType SelectionType => TabDataSettings?
				.FirstOrDefault(dataSettings => dataSettings.SelectionType != SelectionType.None)
				?.SelectionType ?? SelectionType.None;

	public override string? ToString() => Address;

	// change to string id?
	public TabDataSettings GetData(int index)
	{
		TabDataSettings ??= [];

		// Creates new Settings if necessary
		while (TabDataSettings.Count <= index)
		{
			TabDataSettings.Add(new TabDataSettings());
		}
		return TabDataSettings[index];
	}
}
