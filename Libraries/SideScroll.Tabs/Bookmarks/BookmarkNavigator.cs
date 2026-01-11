using SideScroll.Attributes;
using SideScroll.Serialize;
using SideScroll.Tabs.Bookmarks.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SideScroll.Tabs.Bookmarks;

public class BookmarkNavigator : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	//public event EventHandler<EventArgs> OnSelectionChanged;

	public int MaxHistorySize { get; set; } = 20;

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

	public int NextId { get; set; }

	public List<Bookmark> History { get; set; } = [];

	public Bookmark? Current
	{
		get
		{
			if (CurrentIndex >= History.Count)
				return null;
			return History[CurrentIndex];
		}
	}

	public bool CanSeekBackward => CurrentIndex > 0;
	public bool CanSeekForward => CurrentIndex + 1 < History.Count;

	[Hidden]
	public SynchronizationContext? Context { get; set; }

	public override string ToString() => $"{CurrentIndex} / {History.Count}";

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

	public void Append(Bookmark bookmark, bool makeCurrent)
	{
		// Current isn't visible, so doing this here gets the right count
		TrimHistory();

		if (makeCurrent)
		{
			CurrentIndex = History.Count;
		}
		bookmark.Name = NextId++.ToString();// + " - " + bookmark.Address;
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

	public void Update(Bookmark bookmark)
	{
		if (bookmark == null)
			return;

		Bookmark currentBookmark = History[CurrentIndex];
		//bookmark = Serialize.SerializerMemory.Clone<Bookmark>(new Core.Log(), bookmark); // sanitize
		currentBookmark.TabBookmark = bookmark.TabBookmark;
	}

	public Bookmark? SeekBackward()
	{
		if (CurrentIndex <= 0) return null;
		
		CurrentIndex--;
		Bookmark oldBookmark = History[CurrentIndex];
		Bookmark newBookmark = oldBookmark.DeepClone(); // Sanitize
		Append(newBookmark, false); // Fork instead?
		return newBookmark;
	}

	public Bookmark? SeekForward()
	{
		if (CurrentIndex >= History.Count - 1) return null;
		
		CurrentIndex++;
		return History[CurrentIndex];
	}

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
