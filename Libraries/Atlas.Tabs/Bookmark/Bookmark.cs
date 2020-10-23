using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs
{
	[PublicData]
	public class Bookmark
	{
		[Name("Bookmark")]
		public string Name { get; set; }
		public string Changed { get; set; } // what was just selected, used for naming, find better default name
		public Type Type { get; set; }
		public string Address => TabBookmark.Address;
		public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
		public TabBookmark TabBookmark { get; set; } = new TabBookmark();
		public bool Imported { get; set; }

		public Bookmark()
		{
			TabBookmark.Bookmark = this;
			TabBookmark.IsRoot = true;
		}

		public override string ToString() => Address;

		public void MergeBookmarks(List<Bookmark> bookmarks)
		{
			/*var nodes = new List<Node>();
			foreach (Bookmark bookmark in bookmarks)
				nodes.Add(bookmark.node);
			node.MergeNodes(nodes);*/
			foreach (Bookmark bookmark in bookmarks)
				TabBookmark.MergeNode(bookmark.TabBookmark);
		}

		public string ToBase64String(Call call, bool publicOnly)
		{
			return SerializerMemory.ToBase64String(call, this, publicOnly);
		}

		public static Bookmark Create(Call call, string encoded, bool publicOnly)
		{
			var serializer = SerializerMemory.Create();
			serializer.PublicOnly = publicOnly;
			serializer.LoadBase64String(encoded);
			Bookmark bookmark = serializer.Load<Bookmark>(call);
			bookmark.Imported = true;
			return bookmark;
		}

		public static Bookmark Create(params string[] labels)
		{
			var bookmark = new Bookmark();
			bookmark.TabBookmark = TabBookmark.Create(labels);
			bookmark.Imported = true;
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
			Bookmark = bookmark;
		}
	}
}
