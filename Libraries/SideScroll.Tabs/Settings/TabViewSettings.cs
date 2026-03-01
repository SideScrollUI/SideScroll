using SideScroll.Tabs.Bookmarks.Models;
using System.Text.Json.Serialization;

namespace SideScroll.Tabs.Settings;

/// <summary>
/// Represents view settings for a tab including width, data settings, and selected row state
/// </summary>
public class TabViewSettings
{
	/// <summary>
	/// Gets the navigation address for all selected rows across all tab data
	/// </summary>
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
			{
				address = '[' + address + ']';
			}

			return address;
		}
	}

	/// <summary>
	/// Gets or sets the tab width
	/// </summary>
	public double? Width { get; set; }

	/// <summary>
	/// Gets or sets the list of tab data settings
	/// </summary>
	public List<TabDataSettings> TabDataSettings { get; set; } = [];

	/// <summary>
	/// Gets all selected rows from all tab data settings
	/// </summary>
	[JsonIgnore]
	public List<SelectedRow> SelectedRows => TabDataSettings.SelectMany(d => d.SelectedRows).ToList();

	/// <summary>
	/// Gets the selection type from the first non-None tab data settings
	/// </summary>
	public SelectionType SelectionType => TabDataSettings
				.FirstOrDefault(dataSettings => dataSettings.SelectionType != SelectionType.None)
				?.SelectionType ?? SelectionType.None;

	public override string? ToString() => Address;

	/// <summary>
	/// Gets or creates tab data settings at the specified index
	/// </summary>
	public TabDataSettings GetData(int index)
	{
		// Creates new Settings if necessary
		while (TabDataSettings.Count <= index)
		{
			TabDataSettings.Add(new TabDataSettings());
		}
		return TabDataSettings[index];
	}
}
