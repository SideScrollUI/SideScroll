using SideScroll.Serialize;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SideScroll.Tabs.Bookmarks;

public class BookmarkNavigator : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	//public event EventHandler<EventArgs> OnSelectionChanged;

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

	public SynchronizationContext? Context { get; set; }

	public override string ToString() => $"{CurrentIndex} / {History.Count}";

	public BookmarkNavigator()
	{
		Context = SynchronizationContext.Current ?? new();

		Bookmark bookmark = new()
		{
			Name = "Start",
		};
		Append(bookmark, true);
	}

	public void Append(Bookmark bookmark, bool makeCurrent)
	{
		if (bookmark == null)
			return;

		// trim Past?
		//int trimAt = currentIndex + 1;
		//if (trimAt < History.Count)
		//	History.RemoveRange(trimAt, History.Count - trimAt);
		if (makeCurrent)
		{
			CurrentIndex = History.Count;
		}
		bookmark.Name = CurrentIndex.ToString();// + " - " + bookmark.Address;
		History.Add(bookmark);
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
		if (CurrentIndex > 0)
		{
			CurrentIndex--;
			Bookmark oldBookmark = History[CurrentIndex];
			Bookmark newBookmark = oldBookmark.DeepClone()!; // sanitize
			Append(newBookmark, false); // Fork instead?
			return newBookmark;
		}
		return null; // throw exception?
	}

	public Bookmark? SeekForward()
	{
		if (CurrentIndex < History.Count - 1)
		{
			CurrentIndex++;
			return History[CurrentIndex];
		}
		return null; // throw exception?
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

/*
Use this?
https://stackoverflow.com/questions/6816436/efficient-way-to-implement-an-indexed-queue-where-elements-can-be-retrieved-by
*/
