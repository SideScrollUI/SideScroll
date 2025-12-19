namespace SideScroll.Tabs.Tools.FileViewer;

public static class FileDataRepos
{
	public static FileNodeDataRepoView Favorites { get; set; } = new("Favorites");
	public static FileNodeDataRepoView Recent { get; set; } = new("Recent", true, 30);
}
