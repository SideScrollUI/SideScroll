using Atlas.Core;

namespace Atlas.Serialize.DataRepos;

public class DataPageView<T>(DataRepoInstance<T> dataRepoInstance, bool ascending, int pageSize = 100) : object()
{
	public DataRepoInstance<T> DataRepoInstance = dataRepoInstance;
	public bool Ascending { get; set; } = ascending;
	public int PageSize { get; set; } = pageSize;
	public int PageIndex { get; set; }
	public int Pages => ((_allPaths?.Count + PageSize - 1) ?? 0) / PageSize;

	private List<string>? _allPaths;

	public IEnumerable<string>? Paths => DataRepoInstance.GetPathEnumerable(Ascending);

	public List<DataItem<T>> GetPage(int page, Call? call = null)
	{
		_allPaths ??= Paths?.ToList();
		if (_allPaths == null) return [];

		call ??= new();
		return _allPaths
			.Skip(PageSize * page)
			.Take(PageSize)
			.Select(path => DataRepoInstance.DataRepo.LoadPath<T>(call, path))
			.OfType<DataItem<T>>()
			.Select(dataItem => new DataItem<T>(dataItem.Key, dataItem.Value))
			.ToList();
	}

	public List<DataItem<T>> Next(Call? call = null)
	{
		PageIndex = Math.Min(Math.Max(0, Pages - 1), PageIndex + 1);
		return GetPage(PageIndex, call);
	}

	public List<DataItem<T>> Previous(Call? call = null)
	{
		PageIndex = Math.Max(0, PageIndex - 1);
		return GetPage(PageIndex, call);
	}
}
