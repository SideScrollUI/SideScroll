using Atlas.Serialize;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Tabs
{
	public class BookmarkNavigator
	{
		public int CurrentIndex { get; set; } = -1;

		public List<Bookmark> History { get; set; } = new List<Bookmark>();

		public Bookmark Current
		{
			get
			{
				if (CurrentIndex >= History.Count)
					return null;
				return History[CurrentIndex];
			}
		}

		//public IObservable<bool> CanSeekBackwardObservable => (CurrentIndex > 0);
		public bool CanSeekBackward => (CurrentIndex > 0);
		public bool CanSeekForward => (CurrentIndex + 1 < History.Count);

		//public event EventHandler<EventArgs> OnSelectionChanged;

		public BookmarkNavigator()
		{
			var bookmark = new Bookmark()
			{
				Name = "Start",
			};
			Append(bookmark, true);
		}

		public override string ToString()
		{
			return CurrentIndex.ToString() + " / " + History.Count;
		}

		public void Append(Bookmark bookmark, bool makeCurrent)
		{
			if (bookmark == null)
				return;
			// trim Past?
			//bookmark = Serialize.SerializerMemory.Clone<Bookmark>(new Core.Log(), bookmark); // sanitize
			//int trimAt = currentIndex + 1;
			//if (trimAt < History.Count)
			//	History.RemoveRange(trimAt, History.Count - trimAt);
			if (makeCurrent)
				CurrentIndex = History.Count;
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
			//bookmark.tabBookmark = 
			//bookmark = bookmark.Clone();
			/*bookmark.Name = prevBookmark.Name;
			bookmark.Changed = prevBookmark.Changed;
			History[currentIndex] = bookmark;*/
		}

		public Bookmark SeekBackward()
		{
			if (CurrentIndex > 0)
			{
				CurrentIndex--;
				Bookmark oldBookmark = History[CurrentIndex];
				Bookmark newBookmark = oldBookmark.Clone<Bookmark>(); // sanitize
				Append(newBookmark, false); // Fork instead?
				return newBookmark;
			}
			return null; // throw exception?
		}

		public Bookmark SeekForward()
		{
			if (CurrentIndex < History.Count - 1)
			{
				CurrentIndex++;
				return History[CurrentIndex];
			}
			return null; // throw exception?
		}
	}
}

/*
Use this?
https://stackoverflow.com/questions/6816436/efficient-way-to-implement-an-indexed-queue-where-elements-can-be-retrieved-by
*/
