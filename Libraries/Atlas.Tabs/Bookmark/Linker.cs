using Atlas.Core;

namespace Atlas.Tabs;

public class Linker
{
	private const string AtlasPrefix = @"atlas://";

	public bool PublicOnly { get; set; }
	public long MaxLength { get; set; } = 65500; // Uri.EscapeDataString limit

#pragma warning disable CS1998 // subclasses can be async
	public virtual async Task<string> AddLinkAsync(Call call, Bookmark bookmark)
#pragma warning restore CS1998
	{
#if DEBUG
		call = call.DebugLogAll();
#endif
		string base64 = bookmark.ToBase64String(call, PublicOnly);
		if (base64.Length > MaxLength)
		{
			call.Log.AddError("Link too large",
				new Tag("Length", base64.Length),
				new Tag("MaxLength", MaxLength));
			throw new Exception($"Link size {base64.Length} > {MaxLength}");
		}

		return AtlasPrefix + base64;
	}

#pragma warning disable CS1998 // subclasses can be async
	public virtual async Task<Bookmark> GetLinkAsync(Call call, string uri, bool checkVersion)
#pragma warning restore CS1998
	{
		if (uri == null) throw new ArgumentNullException(nameof(uri));

#if DEBUG
		call = call.DebugLogAll();
#endif

		if (!uri.StartsWith(AtlasPrefix))
		{
			call.Log.AddError("Invalid prefix");
			throw new ArgumentException("Invalid uri prefix");
		}

		string base64 = uri[AtlasPrefix.Length..];

		if (uri.Length > MaxLength)
		{
			call.Log.AddError("Link too large",
				new Tag("Length", uri.Length),
				new Tag("MaxLength", MaxLength));
			throw new ArgumentException($"Link too large: {uri.Length} / {MaxLength}");
		}

		Bookmark bookmark = Bookmark.Create(call, base64, PublicOnly);
		return bookmark;
	}
}
