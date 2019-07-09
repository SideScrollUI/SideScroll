using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Tabs
{
	// Display Class
	public class TabBookmarkItem
	{
		//[ButtonColumn("-")]
		public event EventHandler<EventArgs> OnDelete;

		[ButtonColumn("-")]
		public void Delete()
		{
			OnDelete?.Invoke(this, null);
		}

		[Name("Bookmark")]
		public string Name => Bookmark.Name;
		[HiddenColumn]
		public Bookmark Bookmark { get; set; }

		public TabBookmarkItem(Bookmark bookmark)
		{
			this.Bookmark = bookmark;
		}
	}
}
