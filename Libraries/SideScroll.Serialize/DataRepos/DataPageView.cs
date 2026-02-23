using SideScroll.Attributes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SideScroll.Serialize.DataRepos;

/// <summary>
/// Interface for paged data views with navigation support
/// </summary>
public interface IDataPageView : INotifyPropertyChanged
{
	/// <summary>
	/// Gets or sets whether items are displayed in ascending order
	/// </summary>
	public bool Ascending { get; set; }

	/// <summary>
	/// Gets or sets the number of items per page
	/// </summary>
	public int PageSize { get; set; }
	
	/// <summary>
	/// Gets the total number of pages
	/// </summary>
	public int PageCount { get; }
	
	/// <summary>
	/// Gets or sets the current page index
	/// </summary>
	public int PageIndex { get; set; }

	/// <summary>
	/// Gets whether there is a previous page available
	/// </summary>
	public bool HasPrevious { get; }
	
	/// <summary>
	/// Gets whether there is a next page available
	/// </summary>
	public bool HasNext { get; }
}

/// <summary>
/// Provides paged access to data repository items
/// </summary>
public class DataPageView<T>(DataRepoInstance<T> dataRepoInstance, bool ascending, int? pageSize = null) : IDataPageView
{
	/// <summary>
	/// Gets or sets the default page size for new instances
	/// </summary>
	public static int DefaultPageSize { get; set; } = 100;

	/// <summary>
	/// Gets the associated data repository instance
	/// </summary>
	public DataRepoInstance<T> DataRepoInstance => dataRepoInstance;

	/// <summary>
	/// Gets the enumerable collection of file paths
	/// </summary>
	public IEnumerable<string>? Paths => DataRepoInstance.GetPathEnumerable(Ascending);

	private List<string>? _allPaths;

	/// <summary>
	/// Gets or sets whether items are displayed in ascending order
	/// </summary>
	public bool Ascending { get; set; } = ascending;

	/// <summary>
	/// Gets or sets the number of items per page
	/// </summary>
	public int PageSize { get; set; } = pageSize ?? DefaultPageSize;
	
	/// <summary>
	/// Gets the total number of pages
	/// </summary>
	public int PageCount => ((_allPaths?.Count + PageSize - 1) ?? 0) / PageSize;
	
	/// <summary>
	/// Gets or sets the current page index
	/// </summary>
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

	/// <summary>
	/// Gets whether there is a previous page available
	/// </summary>
	public bool HasPrevious => PageIndex > 0;
	
	/// <summary>
	/// Gets whether there is a next page available
	/// </summary>
	public bool HasNext => PageIndex + 1 < PageCount;

	/// <summary>
	/// Occurs when a property value changes
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Gets or sets the synchronization context for property change notifications
	/// </summary>
	[Hidden]
	public SynchronizationContext? Context { get; set; } = SynchronizationContext.Current ?? new();

	/// <summary>
	/// Loads and returns the items for the specified page
	/// </summary>
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

	/// <summary>
	/// Navigates to the next page and returns its items
	/// </summary>
	public List<DataItem<T>> Next(Call? call = null)
	{
		PageIndex = Math.Min(Math.Max(0, PageCount - 1), PageIndex + 1);
		return GetPage(PageIndex, call);
	}

	/// <summary>
	/// Navigates to the previous page and returns its items
	/// </summary>
	public List<DataItem<T>> Previous(Call? call = null)
	{
		PageIndex = Math.Max(0, PageIndex - 1);
		return GetPage(PageIndex, call);
	}

	/// <summary>
	/// Notifies listeners that a property value has changed
	/// </summary>
	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		if (Context != null)
		{
			Context.Post(NotifyPropertyChangedContext, propertyName);
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
