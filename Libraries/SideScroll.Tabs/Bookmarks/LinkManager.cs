namespace SideScroll.Tabs.Bookmarks;

public class LinkManager(Project project)
{
	public static LinkManager? Instance { get; set; }

	public LinkCollection Created { get; set; } = new(project, "Created");
	public LinkCollection Imported { get; set; } = new(project, "Imported");

	public void Reload(Call call)
	{
		Created.Load(call, true);
		Imported.Load(call, true);
	}
}
