using Atlas.Core;
using Atlas.Serialize;

namespace Atlas.Tabs.Tools;

public class FileNodeDataRepoView(string groupId, bool indexed = false, int? maxItems = null)
{
	public string GroupId = groupId;

	private DataRepoView<NodeView>? _dataRepoNodes;

	private readonly SemaphoreSlim _semaphore = new(1);

	public async Task<DataRepoView<NodeView>> OpenViewAsync(Project project)
	{
		await _semaphore.WaitAsync();

		try
		{
			return _dataRepoNodes ??= project.DataApp.OpenView<NodeView>(GroupId, indexed);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public async Task<DataRepoView<NodeView>> LoadViewAsync(Call call, Project project)
	{
		DataRepoView<NodeView> fileFavorites = await FileDataRepos.Favorites.OpenViewAsync(project);

		await _semaphore.WaitAsync();

		try
		{
			if (_dataRepoNodes?.Loaded == true) return _dataRepoNodes;

			// DataRepo might only have been opened and not loaded before
			_dataRepoNodes ??= project.DataApp.OpenView<NodeView>(GroupId, indexed, maxItems);
			_dataRepoNodes.LoadAllIndexed(call);
			foreach (NodeView nodeView in _dataRepoNodes.Items.Values)
			{
				nodeView.DataRepoFavorites = fileFavorites;
			}
			return _dataRepoNodes;
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public async Task SaveAsync(Call call, Project project, NodeView nodeView)
	{
		DataRepoView<NodeView> dataRepoView = await FileDataRepos.Recent.OpenViewAsync(project);
		dataRepoView.Save(call, nodeView);
	}
}
