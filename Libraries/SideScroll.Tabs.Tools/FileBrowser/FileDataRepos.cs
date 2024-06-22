namespace SideScroll.Tabs.Tools;

public static class FileDataRepos
{
	public static FileNodeDataRepoView Favorites = new("Favorites");
	public static FileNodeDataRepoView Recent = new("Recent", true, 30);
}
