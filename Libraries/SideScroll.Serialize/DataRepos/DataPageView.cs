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
	/// Gets the total number of items
	/// </summary>
	public int ItemCount { get; }

	/// <summary>
	/// Gets the total number of pages
	/// </summary>
	public int PageCount { get; }

	/// <summary>
	/// Gets or sets the current page index, or null when no page has been loaded yet
	/// </summary>
	public int? PageIndex { get; set; }

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
	private int? _indexCount;

	/// <summary>
	/// Gets or sets whether items are displayed in ascending order
	/// </summary>
	/// <remarks>
	/// Changing this discards the cached paths. They're enumerated in this order, so keeping them
	/// left unindexed pages in their original order after the direction changed
	/// </remarks>
	public bool Ascending
	{
		get => _ascending;
		set
		{
			if (_ascending == value) return;

			_ascending = value;
			Refresh();
		}
	}
	private bool _ascending = ascending;

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

			// Resizing the pages can leave the current index past the last one. Only checked once
			// something is loaded, reading PageCount here would load the whole repository from a
			// page size assignment
			if (_pageIndex is { } index && IsCountLoaded)
			{
				int lastPageIndex = PageCount - 1;
				if (lastPageIndex >= 0 && index > lastPageIndex)
				{
					PageIndex = lastPageIndex;
				}
			}
			NotifyPropertyChanged(nameof(HasNext));
		}
	}
	private int _pageSize = ValidatePageSize(pageSize ?? DefaultPageSize, nameof(pageSize));

	/// <summary>
	/// Gets the total number of items, loading them if they haven't been already
	/// </summary>
	/// <remarks>
	/// Indexed instances count the index, which is what <see cref="GetPage(int, Call?)"/> pages
	/// through. Counting the paths instead made <see cref="PageCount"/> and <see cref="HasNext"/>
	/// disagree with the pages actually returned
	/// </remarks>
	public int ItemCount
	{
		get
		{
			if (DataRepoInstance.Index != null)
			{
				return _indexCount ??= DataRepoInstance.Index.Load(new Call()).Items.Count;
			}

			// Derived rather than cached alongside the paths, GetPage() loads them too and a
			// separate count went stale against them
			_allPaths ??= GetPathEnumerable(new Call())?.ToList();
			return _allPaths?.Count ?? 0;
		}
	}

	// Whether the count is known without going back to disk
	private bool IsCountLoaded => DataRepoInstance.Index != null ? _indexCount != null : _allPaths != null;

	/// <summary>
	/// Gets the total number of pages
	/// </summary>
	public int PageCount
	{
		get
		{
			int count = ItemCount;
			return count / PageSize + (count % PageSize > 0 ? 1 : 0);
		}
	}

	/// <summary>
	/// Gets or sets the current page index, or null when no page has been loaded yet
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The value is negative</exception>
	public int? PageIndex
	{
		get => _pageIndex;
		set
		{
			if (value is { } index)
			{
				ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(PageIndex));
			}
			if (_pageIndex == value) return;

			_pageIndex = value;
			NotifyPropertyChanged();
			NotifyPropertyChanged(nameof(HasPrevious));
			NotifyPropertyChanged(nameof(HasNext));
		}
	}
	private int? _pageIndex;

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
	/// <remarks>
	/// Compared this way rather than as PageIndex + 1 &lt; PageCount, which overflows to
	/// int.MinValue at int.MaxValue and reported a next page that doesn't exist
	/// </remarks>
	public bool HasNext => _pageIndex is { } index ? index < PageCount - 1 : PageCount > 0;

	/// <summary>
	/// Discards the cached paths and item count so the next read reloads them
	/// </summary>
	public void Refresh()
	{
		_allPaths = null;
		_indexCount = null;

		NotifyPropertyChanged(nameof(ItemCount));
		NotifyPropertyChanged(nameof(PageCount));
		NotifyPropertyChanged(nameof(HasPrevious));
		NotifyPropertyChanged(nameof(HasNext));
	}

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
	/// Loads and returns the items for the current page, or the first one when none is set yet
	/// </summary>
	/// <remarks>
	/// Use this rather than <see cref="Next"/> to load the first page. Next() only reached page
	/// zero because the index started below it, which made "next" mean "first" on a new view
	/// </remarks>
	public List<DataItem<T>> GetPage(Call? call = null)
	{
		int page = _pageIndex ?? 0;
		PageIndex = page;
		return GetPage(page, call);
	}

	/// <summary>
	/// Loads and returns the items for the specified page
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is negative</exception>
	public List<DataItem<T>> GetPage(int page, Call? call = null)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(page);

		call ??= new();

		// Beyond int.MaxValue there's nothing to skip to, and the cast would wrap to a negative
		long offset = (long)PageSize * page;
		if (offset > int.MaxValue)
			return [];

		if (DataRepoInstance.Index != null)
		{
			IEnumerable<DataRepoIndex<T>.Item> indexItems = DataRepoInstance.Index.Load(call).Items;
			if (!Ascending)
			{
				indexItems = indexItems.Reverse();
			}

			return indexItems
				.Skip((int)offset)
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

		// Only the unindexed path needs these, loading them for an indexed instance enumerated the
		// whole repository for a result the index already provides
		_allPaths ??= GetPathEnumerable(call)?.ToList();
		if (_allPaths == null) return [];

		return _allPaths
			.Skip((int)offset)
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
		int lastPageIndex = Math.Max(0, PageCount - 1);

		// Compared before incrementing, PageIndex + 1 overflows to int.MinValue at int.MaxValue
		int next = _pageIndex is { } index
			? (index < lastPageIndex ? index + 1 : lastPageIndex)
			: 0;

		PageIndex = next;
		return GetPage(next, call);
	}

	/// <summary>
	/// Navigates to the previous page and returns its items
	/// </summary>
	public List<DataItem<T>> Previous(Call? call = null)
	{
		int lastPageIndex = Math.Max(0, PageCount - 1);

		// Clamped to the last page as well, so stepping back from an index past the end lands on it
		int previous = _pageIndex is { } index
			? Math.Min(lastPageIndex, Math.Max(0, index - 1))
			: 0;

		PageIndex = previous;
		return GetPage(previous, call);
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
