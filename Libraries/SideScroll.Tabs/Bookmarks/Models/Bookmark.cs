using SideScroll.Attributes;
using SideScroll.Serialize;
using SideScroll.Serialize.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SideScroll.Tabs.Bookmarks.Models;

/// <summary>
/// Defines the type of bookmark for navigation and linking
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookmarkType
{
	/// <summary>
	/// Default bookmark type
	/// </summary>
	Default = 0,

	/// <summary>
	/// Full navigation path from the start
	/// </summary>
	Full = 1,

	/// <summary>
	/// Single leaf bookmark that can replace a full path
	/// </summary>
	Leaf = 2,

	/// <summary>
	/// Tab link bookmark activated by clicking a tab link
	/// </summary>
	Tab = 3,
}

/// <summary>
/// Represents a bookmark for tab navigation state, supporting serialization and linking
/// </summary>
[PublicData]
public class Bookmark
{
	/// <summary>
	/// Gets or sets the bookmark name
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the description of what was just selected (used for naming)
	/// </summary>
	public string? Changed { get; set; }

	/// <summary>
	/// Gets or sets the tab type (must implement ITab)
	/// </summary>
	[HiddenColumn]
	public Type? TabType { get; set; }

	/// <summary>
	/// Gets or sets the bookmark type
	/// </summary>
	[HiddenColumn]
	public BookmarkType BookmarkType { get; set; }

	/// <summary>
	/// Gets or sets whether this bookmark was imported from an external source
	/// </summary>
	[HiddenColumn]
	public bool Imported { get; set; }

	/// <summary>
	/// Gets the navigation address for this bookmark
	/// </summary>
	public string Address => TabBookmark.GetAddress();

	/// <summary>
	/// Gets the display label combining name and address
	/// </summary>
	[HiddenColumn]
	public string Label => (Name != null ? (Name + ":\n") : "") + Address;

	/// <summary>
	/// Gets or sets the creation timestamp
	/// </summary>
	public DateTime? CreatedTime { get; set; }

	/// <summary>
	/// Gets or sets the tab bookmark containing navigation state
	/// </summary>
	[HiddenColumn]
	public TabBookmark TabBookmark { get; set; } = new();

	public override string ToString() => Label;

	/// <summary>
	/// Initializes a new bookmark with default root settings
	/// </summary>
	public Bookmark()
	{
		TabBookmark.IsRoot = true;
	}

	/// <summary>
	/// Creates a bookmark from a base64 encoded string
	/// </summary>
	/// <param name="call">The call context for logging</param>
	/// <param name="base64">The base64 encoded bookmark data</param>
	/// <param name="publicOnly">Whether to include only public data</param>
	public static Bookmark Create(Call call, string base64, bool publicOnly)
	{
		var serializer = SerializerMemory.Create();
		serializer.PublicOnly = publicOnly;
		serializer.LoadBase64String(base64);

		Bookmark bookmark = serializer.Load<Bookmark>(call);
		bookmark.Imported = true;
		return bookmark;
	}

	/// <summary>
	/// Creates a bookmark from a navigation path of labels
	/// </summary>
	public static Bookmark Create(params string[] labels)
	{
		Bookmark bookmark = new()
		{
			TabBookmark = TabBookmark.Create(labels),
			CreatedTime = DateTime.Now,
		};
		return bookmark;
	}

	/// <summary>
	/// Imports this bookmark's data into the specified project
	/// </summary>
	public void Import(Project project)
	{
		TabBookmark.Import(project);
	}

	/// <summary>
	/// Serializes this bookmark to a base64 encoded string
	/// </summary>
	/// <param name="call">The call context for logging</param>
	/// <param name="publicOnly">Whether to include only public data</param>
	public string ToBase64String(Call call, bool publicOnly)
	{
		return SerializerMemory.ToBase64String(call, this, publicOnly);
	}

	/// <summary>
	/// Serializes this bookmark to a JSON string
	/// </summary>
	public string ToJson()
	{
		return JsonSerializer.Serialize(this, JsonConverters.PublicSerializerOptions);
	}

	/// <summary>
	/// Attempts to parse a bookmark from a JSON string
	/// </summary>
	public static bool TryParseJson(string json, [NotNullWhen(true)] out Bookmark? bookmark)
	{
		try
		{
			bookmark = JsonSerializer.Deserialize<Bookmark>(json, JsonConverters.PublicSerializerOptions);
			return bookmark != null;
		}
		catch
		{
			bookmark = null;
			return false;
		}
	}
}
