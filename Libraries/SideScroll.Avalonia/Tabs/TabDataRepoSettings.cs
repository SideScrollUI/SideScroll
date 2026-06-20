using SideScroll.Attributes;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Settings;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Tabs;

[PrivateData]
public class TabDataRepoSettings(UserSettings userSettings, CallAction save) : ITab
{
	public UserSettings UserSettings => userSettings;
	public CallAction Save => save;

	public TabInstance Create() => new Instance(this);

	private class Instance(TabDataRepoSettings tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("Settings", new TabDataSettings(tab.UserSettings, tab.Save)),
				new("Repositories", new TabDataRepositories(tab.UserSettings)),
			};
		}
	}
}
