using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

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
			this.Bookmark = bookmark;
		}
	}
}
