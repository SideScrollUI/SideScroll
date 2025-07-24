using SideScroll.Attributes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SideScroll.Serialize.DataRepos;

public interface IDataPageView : INotifyPropertyChanged
{
	public bool Ascending { get; set; }

	public int PageSize { get; set; }
	public int PageCount { get; }
	public int PageIndex { get; set; }

	public bool HasPrevious { get; }
	public bool HasNext { get; }
}

public class DataPageView<T>(DataRepoInstance<T> dataRepoInstance, bool ascending, int? pageSize = null) : IDataPageView
{
	public static int DefaultPageSize { get; set; } = 100;

	public DataRepoInstance<T> DataRepoInstance => dataRepoInstance;

	private List<string>? _allPaths;

	public IEnumerable<string>? Paths => DataRepoInstance.GetPathEnumerable(Ascending);

	public bool Ascending { get; set; } = ascending;

	public int PageSize { get; set; } = pageSize ?? DefaultPageSize;
	public int PageCount => ((_allPaths?.Count + PageSize - 1) ?? 0) / PageSize;
	public int PageIndex
	{
		get => _pageIndex;
		set
		{
			_pageIndex = value;
			NotifyPropertyChanged();
			NotifyPropertyChanged(nameof(HasPrevious));
			NotifyPropertyChanged(nameof(HasNext));
		}
	}
	private int _pageIndex = -1;

	public bool HasPrevious => PageIndex > 0;
	public bool HasNext => PageIndex + 1 < PageCount;

	public event PropertyChangedEventHandler? PropertyChanged;

	[Hidden]
	public SynchronizationContext? Context { get; set; } = SynchronizationContext.Current ?? new();

	public List<DataItem<T>> GetPage(int page, Call? call = null)
	{
		_allPaths ??= Paths?.ToList();
		if (_allPaths == null) return [];

		call ??= new();
		return _allPaths
			.Skip(PageSize * page)
			.Take(PageSize)
			.Select(path => DataRepo.LoadPath<T>(call, path))
			.OfType<DataItem<T>>()
			.Select(dataItem => new DataItem<T>(dataItem.Key, dataItem.Value))
			.ToList();
	}

	public List<DataItem<T>> Next(Call? call = null)
	{
		PageIndex = Math.Min(Math.Max(0, PageCount - 1), PageIndex + 1);
		return GetPage(PageIndex, call);
	}

	public List<DataItem<T>> Previous(Call? call = null)
	{
		PageIndex = Math.Max(0, PageIndex - 1);
		return GetPage(PageIndex, call);
	}

	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		if (Context != null)
		{
			Context!.Post(NotifyPropertyChangedContext, propertyName);
		}
		else
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	private void NotifyPropertyChangedContext(object? state)
	{
		string propertyName = (string)state!;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
