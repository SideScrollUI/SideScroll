using Atlas.Core;

namespace Atlas.Tabs.Tools;

public class TabFileBrowser : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance, ITabAsync
	{
		public async Task LoadAsync(Call call, TabModel model)
		{
			var dataRepoFavorites = await FileDataRepos.Favorites.LoadViewAsync(call, Project);

			model.Items = new List<ListItem>
			{
				new("Current", new TabDirectory(Directory.GetCurrentDirectory(), dataRepoFavorites)),
				new("Downloads", new TabDirectory(Paths.DownloadPath, dataRepoFavorites)),
				new("Drives", new TabDrives(dataRepoFavorites)),
				new("Favorites", new TabFileDataRepo(dataRepoFavorites)),
			};
		}
	}
}
