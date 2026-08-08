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
	public static int DefaultPageSize
	{
		get => _defaultPageSize;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, nameof(DefaultPageSize));
			_defaultPageSize = value;
		}
	}
	private static int _defaultPageSize = 100;

	/// <summary>
	/// Gets the associated data repository instance
	/// </summary>
	public DataRepoInstance<T> DataRepoInstance => dataRepoInstance;

	private List<string>? _allPaths;

	/// <summary>
	/// Gets or sets whether items are displayed in ascending order
	/// </summary>
	public bool Ascending { get; set; } = ascending;

	/// <summary>
	/// Gets or sets the number of items per page
	/// </summary>
	public int PageSize
	{
		get => _pageSize;
		set
		{
			ValidatePageSize(value, nameof(PageSize));
			if (_pageSize == value) return;

			_pageSize = value;
			NotifyPropertyChanged();
			NotifyPropertyChanged(nameof(PageCount));

			// Resizing the pages can leave the current index past the last one
			int lastPageIndex = PageCount - 1;
			if (lastPageIndex >= 0 && _pageIndex > lastPageIndex)
			{
				PageIndex = lastPageIndex;
			}
			else
			{
				NotifyPropertyChanged(nameof(HasNext));
			}
		}
	}
	private int _pageSize = ValidatePageSize(pageSize ?? DefaultPageSize, nameof(pageSize));

	/// <summary>
	/// Gets the total number of pages
	/// </summary>
	public int PageCount
	{
		get
		{
			int count = _allPaths?.Count ?? 0;
			return count / PageSize + (count % PageSize > 0 ? 1 : 0);
		}
	}

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

	private static int ValidatePageSize(int value, string paramName)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, paramName);
		return value;
	}

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
	/// Gets the enumerable collection of file paths
	/// </summary>
	public IEnumerable<string>? GetPathEnumerable(Call call) => DataRepoInstance.GetPathEnumerable(call, Ascending);

	/// <summary>
	/// Loads and returns the items for the specified page
	/// </summary>
	public List<DataItem<T>> GetPage(int page, Call? call = null)
	{
		call ??= new();
		_allPaths ??= GetPathEnumerable(call)?.ToList();

		if (DataRepoInstance.Index != null)
		{
			IEnumerable<DataRepoIndex<T>.Item> indexItems = DataRepoInstance.Index.Load(call).Items;
			if (!Ascending)
			{
				indexItems = indexItems.Reverse();
			}

			return indexItems
				.Skip(PageSize * page)
				.Take(PageSize)
				.Select(item => DataRepoInstance.LoadDataItem(
					call,
					DataRepoInstance.DataRepo.GetDataPath(
						DataRepoInstance.DataType,
						DataRepoInstance.GroupId,
						item.Key),
					item.Key))
				.OfType<DataItem<T>>()
				.ToList();
		}

		if (_allPaths == null) return [];

		return _allPaths
			.Skip(PageSize * page)
			.Take(PageSize)
			.Select(path => DataRepoInstance.LoadDataItem(call, path))
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
