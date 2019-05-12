using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Tabs
{
	public class Bookmark
	{
		[Name("Bookmark")]
		public string Name { get; set; }
		public string Changed { get; set; }
		public string Address => tabBookmark.Address;
		public TabBookmark tabBookmark { get; set; } = new TabBookmark();

		public override string ToString()
		{
			//return Name;
			return Address;
		}

		public void MergeBookmarks(List<Bookmark> bookmarks)
		{
			/*List<Node> nodes = new List<Node>();
			foreach (Bookmark bookmark in bookmarks)
				nodes.Add(bookmark.node);
			node.MergeNodes(nodes);*/
			foreach (Bookmark bookmark in bookmarks)
				tabBookmark.MergeNode(bookmark.tabBookmark);
		}
	}

	// Display Class
	public class ViewBookmarkName
	{
		[Name("Bookmark")]
		public string Name => Bookmark.Name;
		public Bookmark Bookmark { get; set; }

		public ViewBookmarkName(Bookmark bookmark)
		{
			this.Bookmark = bookmark;
		}
	}
}
