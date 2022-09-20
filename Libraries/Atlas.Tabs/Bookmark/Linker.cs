using Atlas.Core;

namespace Atlas.Tabs;

public class Linker
{
	private const string AtlasPrefix = @"atlas://";

	public bool PublicOnly { get; set; }
	public long MaxLength { get; set; } = 65500; // Uri.EscapeDataString limit

#pragma warning disable CS1998 // subclasses can be async
	public virtual async Task<string?> GetLinkUriAsync(Call call, Bookmark bookmark)
#pragma warning restore CS1998
	{
#if DEBUG
		call = call.DebugLogAll();
#endif
		string base64 = bookmark.ToBase64String(call, PublicOnly);
		if (base64.Length > MaxLength)
			return "Bookmark size " + base64.Length + " > " + MaxLength;

		return AtlasPrefix + base64;
	}

#pragma warning disable CS1998 // subclasses can be async
	public virtual async Task<Bookmark?> GetBookmarkAsync(Call call, string uri, bool checkVersion)
#pragma warning restore CS1998
	{
		if (uri == null)
			return null;
#if DEBUG
		call = call.DebugLogAll();
#endif

		if (!uri.StartsWith(AtlasPrefix))
		{
			call.Log.AddError("Invalid prefix");
			return null;
		}

		string base64 = uri[AtlasPrefix.Length..];

		if (uri.Length > MaxLength)
		{
			call.Log.AddError("Bookmark too large",
				new Tag("Length", uri.Length),
				new Tag("MaxLength", MaxLength));
			return null;
		}

		Bookmark bookmark = Bookmark.Create(call, base64, PublicOnly);
		return bookmark;
	}
}
