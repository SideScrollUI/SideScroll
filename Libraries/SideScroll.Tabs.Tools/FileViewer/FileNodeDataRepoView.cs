using SideScroll.Serialize.DataRepos;

namespace SideScroll.Tabs.Tools.FileViewer;

/// <summary>
/// Manages a <see cref="DataRepoView{T}"/> of <see cref="NodeView"/> items for a named group,
/// providing thread-safe open, load, and save operations.
/// </summary>
public class FileNodeDataRepoView(string groupId, bool indexed = false, int? maxItems = null)
{
	/// <summary>Gets the data repository group identifier.</summary>
	public string GroupId => groupId;

	/// <summary>Gets whether the view uses indexed storage for sorted retrieval.</summary>
	public bool Indexed => indexed;

	/// <summary>Gets the optional maximum number of items to retain in the view.</summary>
	public int? MaxItems => maxItems;

	private DataRepoView<NodeView>? _dataRepoNodes;

	private readonly SemaphoreSlim _semaphore = new(1);

	/// <summary>Opens or returns the cached data repository view without loading its items.</summary>
	public async Task<DataRepoView<NodeView>> OpenViewAsync(Project project)
	{
		await _semaphore.WaitAsync();

		try
		{
			return _dataRepoNodes ??= project.Data.App.OpenView<NodeView>(GroupId, Indexed, MaxItems);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	/// <summary>Loads all indexed items into the view and attaches file selector options to each node.</summary>
	public async Task<DataRepoView<NodeView>> LoadViewAsync(Call call, Project project)
	{
		DataRepoView<NodeView> fileFavorites = await FileDataRepos.Favorites.OpenViewAsync(project);
		FileSelectorOptions fileSelectorOptions = new()
		{
			DataRepoFavorites = fileFavorites,
		};

		await _semaphore.WaitAsync();

		try
		{
			if (_dataRepoNodes?.IsLoaded == true) return _dataRepoNodes;

			// DataRepo might only have been opened and not loaded before
			_dataRepoNodes ??= project.Data.App.OpenView<NodeView>(GroupId, Indexed, MaxItems);
			_dataRepoNodes.LoadAllIndexed(call);
			foreach (NodeView nodeView in _dataRepoNodes.Values)
			{
				nodeView.FileSelectorOptions = fileSelectorOptions;
			}
			return _dataRepoNodes;
		}
		finally
		{
			_semaphore.Release();
		}
	}

	/// <summary>Saves the given node view to the underlying data repository.</summary>
	public async Task SaveAsync(Call call, Project project, NodeView nodeView)
	{
		DataRepoView<NodeView> dataRepoView = await OpenViewAsync(project);
		dataRepoView.Save(call, nodeView);
	}
}
