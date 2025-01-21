namespace SideScroll.Tabs.Bookmarks;

public class BookmarkManager(Project project)
{
	public static BookmarkManager? Instance { get; set; }

	public BookmarkCollection Created { get; set; } = new(project, "Created");
	public BookmarkCollection Imported { get; set; } = new(project, "Imported");

	public void Reload(Call call)
	{
		Created.Load(call, true);
		Imported.Load(call, true);
	}
}
