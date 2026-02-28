using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Settings;
using System.Collections;

namespace SideScroll.Tabs;

// Generates an event when the SelectedItems change
// Can be used with TabInstance and any objects added to the TabModel
public interface ITabSelector
{
	IList? SelectedItems { get; }

	event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;
}

public class TabSelectionChangedEventArgs(bool recreate = false) : EventArgs
{
	public bool Recreate => recreate;
}

// For CustomTabControls
public interface ITabItemSelector
{
	IList SelectedItems { get; set; }
}

// TabInstance or Controls can specify this to create child controls dynamically
public interface ITabCreator
{
	object CreateControl(object value, out string? label);
}

// When this object is viewed, it will call CreateAsync() to generate an iTab to show instead
public interface ITabCreatorAsync
{
	Task<ITab?> CreateAsync(Call call);
}

public interface ITabDataControl : IDisposable
{
	public TabDataSettings TabDataSettings { get; set; }

	public void LoadSettings();
}

public interface ITabDataSelector : ITabDataControl
{
	public IList? Items { get; set; }

	public IList SelectedItems { get; set; }
	public object? SelectedItem { get; set; }
	public HashSet<SelectedRow> SelectedRows { get; }

	public event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;
}
