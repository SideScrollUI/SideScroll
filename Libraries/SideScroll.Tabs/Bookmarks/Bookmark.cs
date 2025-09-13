using SideScroll.Attributes;
using SideScroll.Serialize;

namespace SideScroll.Tabs.Bookmarks;

public enum BookmarkType
{
	Default = 0,
	Full = 1, // Full path from Start
	Leaf = 2, // Can replace Full if a single leaf is found
	Tab = 3, // Clicking the Tab Link
}

[PublicData]
public class Bookmark
{
	[Name("Bookmark")]
	public string? Name { get; set; }

	public string? Changed { get; set; } // what was just selected, used for naming, find better default name

	[HiddenColumn]
	public Type? Type { get; set; } // Must be ITab

	public string Address => TabBookmark?.GetAddress() ?? "";

	[HiddenColumn]
	public string Path => (Name != null ? (Name + ":\n") : "") + Address;

	public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

	[HiddenColumn]
	public TabBookmark TabBookmark { get; set; } = new();

	[HiddenColumn]
	public BookmarkType BookmarkType { get; set; }

	[HiddenColumn]
	public bool Imported { get; set; }

	public override string ToString() => Path;

	public Bookmark()
	{
		TabBookmark.Bookmark = this;
		TabBookmark.IsRoot = true;
	}

	public void MergeBookmarks(List<Bookmark> bookmarks)
	{
		foreach (Bookmark bookmark in bookmarks)
		{
			TabBookmark.MergeNode(bookmark.TabBookmark);
		}
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
		Bookmark bookmark = new()
		{
			TabBookmark = TabBookmark.Create(labels)
		};
		return bookmark;
	}

	public void Reinitialize()
	{
		TabBookmark.Reinitialize(this);
	}
}
