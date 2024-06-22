using SideScroll;

namespace SideScroll.Tabs.Tools;

public class TabFileBrowser(SelectFileDelegate? selectFileDelegate = null) : ITab
{
	public SelectFileDelegate? SelectFileDelegate => selectFileDelegate;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabFileBrowser tab) : TabInstance, ITabAsync
	{
		public async Task LoadAsync(Call call, TabModel model)
		{
			var dataRepoFavorites = await FileDataRepos.Favorites.LoadViewAsync(call, Project);
			FileSelectorOptions fileSelectorOptions = new()
			{
				DataRepoFavorites = dataRepoFavorites,
				SelectFileDelegate = tab.SelectFileDelegate
			};

			model.Items = new List<ListItem>
			{
				new("Current", new TabDirectory(Directory.GetCurrentDirectory(), fileSelectorOptions)),
				new("Downloads", new TabDirectory(Paths.DownloadPath, fileSelectorOptions)),
				new("Drives", new TabDrives(fileSelectorOptions)),
				new("Favorites", new TabFileDataRepo(dataRepoFavorites, fileSelectorOptions)),
			};
		}
	}
}
