using Atlas.Core;
using Atlas.Serialize;

namespace Atlas.Tabs.Tools;

public class FileNodeDataRepoView(string groupId, bool indexed = false, int? maxItems = null)
{
	public string GroupId => groupId;
	public bool Indexed => indexed;
	public int? MaxItems => maxItems;

	private DataRepoView<NodeView>? _dataRepoNodes;

	private readonly SemaphoreSlim _semaphore = new(1);

	public async Task<DataRepoView<NodeView>> OpenViewAsync(Project project)
	{
		await _semaphore.WaitAsync();

		try
		{
			return _dataRepoNodes ??= project.DataApp.OpenView<NodeView>(GroupId, Indexed, MaxItems);
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
			_dataRepoNodes ??= project.DataApp.OpenView<NodeView>(GroupId, Indexed, MaxItems);
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
		DataRepoView<NodeView> dataRepoView = await OpenViewAsync(project);
		dataRepoView.Save(call, nodeView);
	}
}