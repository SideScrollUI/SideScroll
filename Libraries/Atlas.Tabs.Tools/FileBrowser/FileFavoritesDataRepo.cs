using Atlas.Core;
using Atlas.Serialize;

namespace Atlas.Tabs.Tools;

public static class FileFavoritesDataRepo
{
	public const string GroupId = "Favorites";

	private static DataRepoView<NodeView>? _dataRepoNodes;

	private static readonly SemaphoreSlim _semaphore = new(1);

	public static async Task<DataRepoView<NodeView>> GetViewAsync(Call call, Project project)
	{
		await _semaphore.WaitAsync();

		try
		{
			if (_dataRepoNodes != null) return _dataRepoNodes;

			_dataRepoNodes = project.DataApp.LoadView<NodeView>(call, GroupId, nameof(NodeView.Name));
			foreach (var node in _dataRepoNodes.Items.Values)
			{
				node.DataRepo = _dataRepoNodes;
			}
			return _dataRepoNodes;
		}
		finally
		{
			_semaphore.Release();
		}
	}
}
