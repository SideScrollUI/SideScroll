using SideScroll.Attributes;
using SideScroll.Serialize;
using SideScroll.Serialize.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SideScroll.Tabs.Bookmarks.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
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
	public string? Name { get; set; }

	public string? Changed { get; set; } // what was just selected, used for naming, find better default name

	[HiddenColumn]
	public Type? TabType { get; set; } // Must be ITab

	[HiddenColumn]
	public BookmarkType BookmarkType { get; set; }

	[HiddenColumn]
	public bool Imported { get; set; }

	public string Address => TabBookmark.GetAddress();

	[HiddenColumn]
	public string Label => (Name != null ? (Name + ":\n") : "") + Address;

	public DateTime? CreatedTime { get; set; }

	[HiddenColumn]
	public TabBookmark TabBookmark { get; set; } = new();

	public override string ToString() => Label;

	public Bookmark()
	{
		TabBookmark.IsRoot = true;
	}

	public static Bookmark Create(Call call, string base64, bool publicOnly)
	{
		var serializer = SerializerMemory.Create();
		serializer.PublicOnly = publicOnly;
		serializer.LoadBase64String(base64);

		Bookmark bookmark = serializer.Load<Bookmark>(call);
		bookmark.Imported = true;
		return bookmark;
	}

	public static Bookmark Create(params string[] labels)
	{
		Bookmark bookmark = new()
		{
			TabBookmark = TabBookmark.Create(labels),
			CreatedTime = DateTime.Now,
		};
		return bookmark;
	}

	public void Import(Project project)
	{
		TabBookmark.Import(project);
	}

	public string ToBase64String(Call call, bool publicOnly)
	{
		return SerializerMemory.ToBase64String(call, this, publicOnly);
	}

	public string ToJson()
	{
		return JsonSerializer.Serialize(this, JsonConverters.PublicSerializerOptions);
	}

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
