using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Settings;
using System.Collections;

namespace SideScroll.Tabs;

// ITab Interfaces

/// <summary>
/// Primary interface for creating tabs. Implement this interface to define a new tab type.
/// Example: public class MyTab : ITab { public TabInstance Create() => new Instance(); }
/// </summary>
public interface ITab
{
	/// <summary>
	/// Creates a new TabInstance for this tab
	/// </summary>
	TabInstance Create();
}

/// <summary>
/// Wraps another tab for nested tab scenarios.
/// Example: TabLinkView implements this to wrap bookmarked tabs
/// </summary>
public interface ITabContainer : ITab
{
	/// <summary>
	/// The wrapped inner tab to use for bookmarking purposes
	/// </summary>
	ITab? Tab { get; }
}

/// <summary>
/// Marks a tab as reloadable, typically used when viewing links.
/// Example: Implement this to support reloading tab content when viewing links
/// </summary>
public interface ITabReloadable : ITab
{
	/// <summary>
	/// Reloads the tab content. Called when viewing a link
	/// </summary>
	void Reload();
}

// TabInstance and Tab Control Interfaces

/// <summary>
/// Generates an event when the selected items change.
/// Can be used with TabInstance and any objects added to the TabModel.
/// Example: TabDataGrid implements this to notify when grid selection changes
/// </summary>
public interface ITabSelector
{
	/// <summary>
	/// The currently selected items
	/// </summary>
	IList? SelectedItems { get; }

	/// <summary>
	/// Event raised when the selection changes
	/// </summary>
	event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;
}

/// <summary>
/// Event arguments for tab selection changed events
/// </summary>
public class TabSelectionChangedEventArgs(bool recreate = false) : EventArgs
{
	/// <summary>
	/// Whether the controls should be recreated after the selection change
	/// </summary>
	public bool Recreate => recreate;
}

/// <summary>
/// Interface for custom tab controls that support item selection.
/// Example: Custom controls can implement this to support selection via SelectedItems property
/// </summary>
public interface ITabItemSelector
{
	/// <summary>
	/// Gets or sets the selected items
	/// </summary>
	IList SelectedItems { get; set; }
}

/// <summary>
/// Enables TabInstance or controls to create child controls dynamically.
/// Example: TabSampleGridDataTable implements this to create custom views for DataRow objects
/// </summary>
public interface ITabCreator
{
	/// <summary>
	/// Creates a control for the specified value
	/// </summary>
	/// <param name="label">Outputs the label for the created control</param>
	/// <returns>The created control object</returns>
	object CreateControl(object value, out string? label);
}

/// <summary>
/// Allows objects to lazily create tabs asynchronously when viewed.
/// Example: Implement this on model objects to defer expensive tab creation until the object is selected.
/// The framework will call CreateAsync() and display the resulting tab
/// </summary>
public interface ITabCreatorAsync
{
	/// <summary>
	/// Creates a tab asynchronously
	/// </summary>
	/// <returns>The created tab, or null if creation failed</returns>
	Task<ITab?> CreateAsync(Call call);
}

/// <summary>
/// Base interface for tab data controls with settings support.
/// Example: Implement this for custom data display controls that need to save/load settings
/// </summary>
public interface ITabDataControl : IDisposable
{
	/// <summary>
	/// The data settings for this tab control
	/// </summary>
	public TabDataSettings TabDataSettings { get; set; }

	/// <summary>
	/// Loads the control's settings
	/// </summary>
	public void LoadSettings();
}

/// <summary>
/// Interface for tab data controls that support item selection, typically implemented by data grids.
/// Example: TabDataGrid implements this to provide full data selection and navigation support
/// </summary>
public interface ITabDataSelector : ITabDataControl
{
	/// <summary>
	/// The data items displayed in the control
	/// </summary>
	public IList? Items { get; set; }

	/// <summary>
	/// The currently selected items
	/// </summary>
	public IList SelectedItems { get; set; }

	/// <summary>
	/// The currently selected item
	/// </summary>
	public object? SelectedItem { get; set; }

	/// <summary>
	/// The set of selected rows with their unique identifiers
	/// </summary>
	public HashSet<SelectedRow> SelectedRows { get; }

	/// <summary>
	/// Event raised when the selection changes
	/// </summary>
	public event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;
}
