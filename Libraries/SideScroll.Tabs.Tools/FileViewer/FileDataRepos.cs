namespace SideScroll.Tabs.Tools.FileViewer;

/// <summary>
/// Provides shared data repository views for tracking favorite and recently accessed file nodes.
/// </summary>
public static class FileDataRepos
{
	/// <summary>Gets or sets the data repository view for favorited file nodes.</summary>
	public static FileNodeDataRepoView Favorites { get; set; } = new("Favorites");

	/// <summary>Gets or sets the data repository view for recently accessed file nodes, limited to 30 entries.</summary>
	public static FileNodeDataRepoView Recent { get; set; } = new("Recent", true, 30);
}
