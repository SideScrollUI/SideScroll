namespace SideScroll.Tabs.Tools.FileViewer;

public static class FileDataRepos
{
	public static FileNodeDataRepoView Favorites = new("Favorites");
	public static FileNodeDataRepoView Recent = new("Recent", true, 30);
}
