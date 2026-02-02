using SideScroll.Tabs.Bookmarks.Models;

namespace SideScroll.Tabs.Bookmarks;

/// <summary>
/// Manages the creation and retrieval of bookmark links for tab navigation
/// </summary>
public class Linker(Project project)
{
	/// <summary>
	/// The URI prefix for SideScroll links
	/// </summary>
	public const string SideScrollPrefix = "sidescroll";
	
	/// <summary>
	/// The link type identifier
	/// </summary>
	public const string LinkType = "link";

	/// <summary>
	/// Only allow exporting classes and members with PublicOnly
	/// </summary>
	public bool PublicOnly { get; set; } = true;
	
	/// <summary>
	/// Maximum length for encoded link URIs. Default is 65,500 (Uri.EscapeDataString limit)
	/// </summary>
	public long MaxLength { get; set; } = 65_500;

	/// <summary>
	/// Creates a link URI from a bookmark by encoding it as a base64 string
	/// Override this method to replace save the base64 elsewhere and return a different LinkUri instead
	/// GetLinkAsync should be modified to retrieve that base64 if required 
	/// </summary>
	/// <param name="bookmark">The bookmark to encode. Must not exceed MaxLength when encoded</param>
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

	/// <summary>
	/// Retrieves a bookmark from a link URI string
	/// </summary>
	public Task<Bookmark> GetLinkAsync(Call call, string uri, bool checkVersion)
	{
		return GetLinkAsync(call, LinkUri.Parse(uri), checkVersion);
	}

	/// <summary>
	/// Retrieves a bookmark from a parsed LinkUri by decoding the base64 encoded bookmark data
	/// Override this method to return the base64 elsewhere based on the LinkUri
	/// </summary>
	/// <param name="linkUri">The parsed link URI. Must have a valid SideScroll prefix and not exceed MaxLength</param>
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
