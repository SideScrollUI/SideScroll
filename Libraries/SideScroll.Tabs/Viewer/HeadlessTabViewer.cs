namespace SideScroll.Tabs.Viewer;

/// <summary>
/// A headless tab viewer that loads and traverses a tab hierarchy without creating any UI controls.
/// Modelled after <c>TabViewer</c> and <c>TabView</c>, but operates purely on data models.
/// Actions and tasks are not processed.
/// </summary>
/// <remarks>Initializes a new <see cref="HeadlessTabViewer"/> for the given project.</remarks>
public class HeadlessTabViewer(Project project)
{
	/// <summary>Gets the project this viewer is associated with.</summary>
	public Project Project => project;

	/// <summary>Gets the root tab view loaded by <see cref="LoadTabAsync"/>, or <c>null</c> if no tab has been loaded yet.</summary>
	public HeadlessTabView? RootView { get; private set; }

	/// <summary>
	/// Creates a tab instance from <paramref name="tab"/>, loads its model asynchronously, and returns
	/// the root <see cref="HeadlessTabView"/>. Child views are not selected automatically; call
	/// <see cref="HeadlessTabView.SelectAllItemsAsync"/> or <see cref="HeadlessTabView.SelectItemsAsync"/>
	/// to traverse children.
	/// </summary>
	public async Task<HeadlessTabView> LoadTabAsync(Call call, ITab tab)
	{
		TabInstance tabInstance = tab.Create();
		tabInstance.iTab = tab;
		tabInstance.Project = Project;

		RootView = new HeadlessTabView(tabInstance, tabInstance.Model.Name);
		await RootView.LoadAsync(call);
		return RootView;
	}
}
