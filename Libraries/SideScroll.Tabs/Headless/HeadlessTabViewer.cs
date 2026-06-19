using SideScroll.Tabs.Bookmarks.Models;

namespace SideScroll.Tabs.Headless;

/// <summary>
/// A headless tab viewer that loads and traverses a tab hierarchy without creating any UI controls.
/// Modelled after <c>TabViewer</c> and <c>TabView</c>, but operates purely on data models.
/// Actions and tasks are not processed.
/// </summary>
/// <remarks>Initializes a new <see cref="HeadlessTabViewer"/> for the given project.</remarks>
public class HeadlessTabViewer(Project project, HeadlessTabOptions? options = null)
{
	/// <summary>Gets the project this viewer is associated with.</summary>
	public Project Project => project;

	/// <summary>Gets the root tab view loaded by <see cref="LoadTabAsync"/>, or <c>null</c> if no tab has been loaded yet.</summary>
	public HeadlessTabView? RootView { get; private set; }

	/// <summary>
	/// Consolidated traversal options: depth limit, per-list item cap, and optional tab filter.
	/// Passed to the root <see cref="HeadlessTabView"/> and propagated automatically to all descendants.
	/// </summary>
	public HeadlessTabOptions Options { get; init; } = options ?? new();

	/// <summary>
	/// Creates a tab instance from <paramref name="tab"/>, loads its model asynchronously, and returns
	/// the root <see cref="HeadlessTabView"/>. Child views are not selected automatically; call
	/// <see cref="HeadlessTabView.SelectAllItemsAsync"/> or <see cref="HeadlessTabView.SelectItemsAsync"/>
	/// to traverse children, or use <see cref="LoadAndTraverseAsync"/> for a one-step operation.
	/// </summary>
	public async Task<HeadlessTabView> LoadTabAsync(Call call, ITab tab)
	{
		TabInstance tabInstance = tab.Create();
		tabInstance.Model.Name = "Start";
		tabInstance.iTab = tab;
		tabInstance.Project = Project;

		RootView = new HeadlessTabView(tabInstance, tabInstance.Model.Name) { Options = Options };
		await RootView.LoadAsync(call);
		return RootView;
	}

	/// <summary>
	/// Convenience method that loads the root tab and immediately traverses its hierarchy
	/// up to <see cref="HeadlessTabOptions.MaxDepth"/> levels.
	/// When <paramref name="bookmark"/> is provided, only the items it selects are followed
	/// (via <see cref="HeadlessTabView.SelectBookmarkItemsRecursiveAsync"/>), stopping at the
	/// bookmark's leaf nodes. Otherwise the full hierarchy is expanded
	/// (via <see cref="HeadlessTabView.SelectAllItemsRecursiveAsync"/>).
	/// </summary>
	public async Task<HeadlessTabView> LoadAndTraverseAsync(Call call, ITab tab, Bookmark? bookmark = null)
	{
		HeadlessTabView rootView = await LoadTabAsync(call, tab);

		// Enforce the optional time budget: cancel the task's token after MaxTime. Sub-task timers
		// created during traversal share this token source, so the whole traversal observes it.
		if (Options.MaxTime is { } maxTime)
		{
			call.TaskInstance?.TokenSource.CancelAfter(maxTime);
		}

		if (bookmark != null)
		{
			await rootView.SelectBookmarkItemsRecursiveAsync(call, bookmark.TabBookmark, Options.MaxDepth);
		}
		else
		{
			await rootView.SelectAllItemsRecursiveAsync(call, Options.MaxDepth);
		}
		return rootView;
	}
}
