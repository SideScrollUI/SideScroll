namespace SideScroll.Tabs.Bookmarks;

/// <summary>
/// Manages collections of created and imported bookmark links for a project
/// </summary>
public class LinkManager(Project project)
{
	/// <summary>
	/// Gets or sets the singleton instance of the link manager
	/// </summary>
	public static LinkManager? Instance { get; set; }

	/// <summary>
	/// Gets the collection of links created by the user
	/// </summary>
	public LinkCollection Created { get; } = new(project, "Created");

	/// <summary>
	/// Gets the collection of links imported from external sources
	/// </summary>
	public LinkCollection Imported { get; } = new(project, "Imported");

	/// <summary>
	/// Reloads both created and imported link collections
	/// </summary>
	public void Reload(Call call)
	{
		Created.Load(call, true);
		Imported.Load(call, true);
	}
}
