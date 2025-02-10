namespace SideScroll.Tabs.Bookmarks;

public class Linker(Project project)
{
	public const string SideScrollPrefix = "sidescroll";
	public const string LinkType = "link";

	public bool PublicOnly { get; set; } = true; // Only allow exporting classes and members with PublicOnly
	public long MaxLength { get; set; } = 65_500; // Uri.EscapeDataString limit

#pragma warning disable CS1998 // subclasses can be async
	public virtual async Task<LinkUri> AddLinkAsync(Call call, Bookmark bookmark)
#pragma warning restore CS1998
	{
#if DEBUG
		call = call.DebugLogAll();
#endif
		string base64 = bookmark.ToBase64String(call, PublicOnly);
		if (base64.Length > MaxLength)
		{
			call.Log.Throw("Link too large",
				new Tag("Length", base64.Length),
				new Tag("MaxLength", MaxLength));
		}

		return new LinkUri(SideScrollPrefix, LinkType, project.Version, base64);
	}

	public Task<Bookmark> GetLinkAsync(Call call, string uri, bool checkVersion)
	{
		return GetLinkAsync(call, LinkUri.Parse(uri), checkVersion);
	}

#pragma warning disable CS1998 // subclasses can be async
	public virtual async Task<Bookmark> GetLinkAsync(Call call, LinkUri linkUri, bool checkVersion)
#pragma warning restore CS1998
	{
		ArgumentNullException.ThrowIfNull(linkUri);

#if DEBUG
		call = call.DebugLogAll();
#endif

		if (linkUri.Prefix != SideScrollPrefix)
		{
			call.Log.Throw<ArgumentException>("Invalid prefix", new Tag("Prefix", linkUri.Prefix));
		}

		string base64 = linkUri.Path!;

		int length = linkUri.ToUri().Length;
		if (length > MaxLength)
		{
			call.Log.Throw<ArgumentException>("Link too large",
				new Tag("Length", length),
				new Tag("MaxLength", MaxLength));
		}

		Bookmark bookmark = Bookmark.Create(call, base64, PublicOnly);
		return bookmark;
	}
}
