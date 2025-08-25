using SideScroll.Attributes;
using SideScroll.Tabs.Samples.Forms.Todo;

namespace SideScroll.Tabs.Samples.Forms;

[ListItem]
public class TabSampleForms
{
	public static TabSampleFormDataTabs DataTabs => new();
	public static TabSampleFormCollection Collection => new();
	public static TabSampleFormTasks Tasks => new();
	public static TabSampleFormUpdating Updating => new();
	public static TabSampleTodos Todo => new();
}
