using Atlas.Core;

namespace Atlas.Tabs.Tools;

public class TabFileBrowser : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance, ITabAsync
	{
		public async Task LoadAsync(Call call, TabModel model)
		{
			var dataRepoView = await FileFavoritesDataRepo.GetViewAsync(call, Project);

			model.Items = new List<ListItem>
			{
				new("Current", new TabDirectory(Directory.GetCurrentDirectory(), dataRepoView)),
				new("Downloads", new TabDirectory(Paths.DownloadPath, dataRepoView)),
				new("Drives", new TabDrives(dataRepoView)),
				new("Favorites", new TabFileFavorites(dataRepoView)),
			};
		}
	}
}
