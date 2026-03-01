using SideScroll.Attributes;
using SideScroll.Serialize;
using SideScroll.Tabs.Bookmarks.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SideScroll.Tabs.Bookmarks;

/// <summary>
/// Manages navigation history for bookmarks, supporting forward and backward navigation with a configurable history size
/// </summary>
public class BookmarkNavigator : INotifyPropertyChanged
{
	/// <summary>
	/// Event raised when a property value changes
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	//public event EventHandler<EventArgs> OnSelectionChanged;

	/// <summary>
	/// Gets or sets the maximum number of bookmarks to keep in history (default: 20)
	/// </summary>
	public int MaxHistorySize { get; set; } = 20;

	/// <summary>
	/// Gets or sets the current position in the history
	/// </summary>
	public int CurrentIndex
	{
		get => _currentIndex;
		set
		{
			_currentIndex = value;
			NotifyPropertyChanged();
			NotifyPropertyChanged(nameof(CanSeekBackward));
			NotifyPropertyChanged(nameof(CanSeekForward));
		}
	}
	private int _currentIndex = -1;

	/// <summary>
	/// Gets or sets the next unique ID to assign to bookmarks
	/// </summary>
	public int NextId { get; set; }

	/// <summary>
	/// Gets or sets the bookmark history list
	/// </summary>
	public List<Bookmark> History { get; set; } = [];

	/// <summary>
	/// Gets the current bookmark in the history
	/// </summary>
	public Bookmark? Current
	{
		get
		{
			if (CurrentIndex >= History.Count)
				return null;
			return History[CurrentIndex];
		}
	}

	/// <summary>
	/// Gets whether backward navigation is possible
	/// </summary>
	public bool CanSeekBackward => CurrentIndex > 0;

	/// <summary>
	/// Gets whether forward navigation is possible
	/// </summary>
	public bool CanSeekForward => CurrentIndex + 1 < History.Count;

	/// <summary>
	/// Gets or sets the synchronization context for property change notifications
	/// </summary>
	[Hidden]
	public SynchronizationContext? Context { get; set; }

	public override string ToString() => $"{CurrentIndex} / {History.Count}";

	/// <summary>
	/// Initializes a new bookmark navigator with a default start bookmark
	/// </summary>
	public BookmarkNavigator()
	{
		Context = SynchronizationContext.Current ?? new();

		Bookmark bookmark = new()
		{
			Name = "Start",
			CreatedTime = DateTime.Now,
		};
		Append(bookmark, true);
	}

	/// <summary>
	/// Appends a bookmark to the history and optionally makes it current
	/// </summary>
	/// <param name="bookmark">The bookmark to append</param>
	/// <param name="makeCurrent">Whether to make this bookmark the current one</param>
	public void Append(Bookmark bookmark, bool makeCurrent)
	{
		// Current isn't visible, so doing this here gets the right count
		TrimHistory();

		if (makeCurrent)
		{
			CurrentIndex = History.Count;
		}
		bookmark.Name = NextId++.ToString();
		History.Add(bookmark);
	}

	private void TrimHistory()
	{
		if (History.Count < MaxHistorySize) return;
		
		int removeCount = History.Count - MaxHistorySize;
		History.RemoveRange(0, removeCount);

		if (CurrentIndex < removeCount)
		{
			CurrentIndex = 0;
		}
		else
		{
			CurrentIndex -= removeCount;
		}
	}

	/// <summary>
	/// Updates the current bookmark with new bookmark data
	/// </summary>
	public void Update(Bookmark bookmark)
	{
		if (bookmark == null)
			return;

		Bookmark currentBookmark = History[CurrentIndex];
		currentBookmark.TabBookmark = bookmark.TabBookmark;
	}

	/// <summary>
	/// Navigates backward in history and creates a new bookmark copy
	/// </summary>
	/// <returns>The previous bookmark, or null if at the beginning</returns>
	public Bookmark? SeekBackward()
	{
		if (CurrentIndex <= 0) return null;
		
		CurrentIndex--;
		Bookmark oldBookmark = History[CurrentIndex];
		Bookmark newBookmark = oldBookmark.DeepClone(); // Sanitize
		Append(newBookmark, false); // Fork instead?
		return newBookmark;
	}

	/// <summary>
	/// Navigates forward in history
	/// </summary>
	/// <returns>The next bookmark, or null if at the end</returns>
	public Bookmark? SeekForward()
	{
		if (CurrentIndex >= History.Count - 1) return null;
		
		CurrentIndex++;
		return History[CurrentIndex];
	}

	/// <summary>
	/// Raises the PropertyChanged event on the appropriate synchronization context
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
