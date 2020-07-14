using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs
{
	public class Bookmark
	{
		[Name("Bookmark")]
		public string Name { get; set; }
		public string Changed { get; set; } // used for naming, find better default name
		public Type Type { get; set; }
		public string Address => TabBookmark.Address;
		public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
		public TabBookmark TabBookmark { get; set; } = new TabBookmark();

		public Bookmark()
		{
			TabBookmark.Bookmark = this;
			TabBookmark.IsRoot = true;
		}

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
				TabBookmark.MergeNode(bookmark.TabBookmark);
		}

		public string GetEncodedString()
		{
			var serializer = new SerializerMemory();
			serializer.Save(new Call(), this);
			string data = serializer.GetEncodedString();
			return data;
		}

		public static Bookmark Create(string encoded)
		{
			var serializer = new SerializerMemory();
			serializer.LoadEncodedString(encoded);
			Bookmark bookmark = serializer.Load<Bookmark>();
			return bookmark;
		}

		public Bookmark GetSecure()
		{
			return this; // todo: implement?
			// clone first?

			/*foreach (var item in tabBookmark.tabChildBookmarks.Values)
			{
				item.
			}*/
		}
	}

	// Display Class
	public class ViewBookmark
	{
		/*public event EventHandler<EventArgs> OnDelete;

		[ButtonColumn("-")]
		public void Delete()
		{

		}*/

		[Name("Bookmark")]
		public string Name => Bookmark.Name;
		[HiddenColumn]
		public Bookmark Bookmark { get; set; }

		public ViewBookmark(Bookmark bookmark)
		{
			Bookmark = bookmark;
		}
	}
}
