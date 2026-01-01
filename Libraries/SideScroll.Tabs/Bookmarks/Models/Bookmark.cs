using SideScroll.Attributes;
using SideScroll.Serialize;

namespace SideScroll.Tabs.Bookmarks.Models;

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

	[HiddenColumn, DeprecatedName("Type")]
	public Type? TabType { get; set; } // Must be ITab

	public string Address => TabViewBookmark?.GetAddress() ?? TabBookmark?.GetAddress() ?? "";

	[HiddenColumn]
	public string Label => (Name != null ? (Name + ":\n") : "") + Address;

	[DeprecatedName("TimeStamp")]
	public DateTime? CreatedTime { get; set; }

	[HiddenColumn, Obsolete("Use TabViewBookmark instead")]
	public TabBookmark? TabBookmark { get; set; }

	[HiddenColumn]
	public TabViewBookmark TabViewBookmark { get; set; } = new();

	[HiddenColumn]
	public BookmarkType BookmarkType { get; set; }

	[HiddenColumn]
	public bool Imported { get; set; }

	public override string ToString() => Label;

	public Bookmark()
	{
		TabViewBookmark.IsRoot = true;
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
			TabViewBookmark = TabViewBookmark.Create(labels),
			CreatedTime = DateTime.Now,
		};
		return bookmark;
	}

	internal void Import(Project project)
	{
		//TabBookmark.Import(project);
		TabViewBookmark.Import(project);
	}
}
