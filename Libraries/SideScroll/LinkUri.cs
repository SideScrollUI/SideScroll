using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SideScroll;

// <prefix>://<type>/[v<version>/]<path>[?<query>]
[PublicData]
public class LinkUri
{
	public string? Prefix { get; set; }
	public string? Type { get; set; }
	public Version? Version { get; set; }
	public string? Path { get; set; }
	public string? Query { get; set; }

	public string? Url { get; set; }

	private static readonly Regex _regex = new(@"^(?<prefix>[a-zA-Z]+)\:\/\/(?<type>[-0-9a-zA-Z\.]+)\/(v(?<version>[\d\.]+)\/)?(?<path>[^\?]+)(\?(?<query>.+))?$");

	public LinkUri() { }

	public LinkUri(string prefix, string type, Version version, string path, string? query = null)
	{
		Prefix = prefix;
		Type = type;
		Version = version;
		Path = path;
		Query = query;

		Url = ToUri();
	}

	public override string ToString() => Url ?? ToUri();

	public virtual bool IsValid() =>
		!Prefix.IsNullOrEmpty() &&
		!Type.IsNullOrEmpty() &&
		!Path.IsNullOrEmpty();

	public string ToUri()
	{
		string uri = $"{Prefix}://{Type}/";

		if (Version != null)
		{
			uri += $"v{Version?.Formatted()}/";
		}

		uri += Path;

		if (Query != null)
		{
			uri += '?' + Query;
		}

		return uri;
	}

	public static LinkUri Parse(string url)
	{
		if (TryParse(url, out LinkUri? linkUri)) return linkUri;

		throw new ArgumentException($"Invalid LinkUri {url}");
	}

	public static bool TryParse(string? url, [NotNullWhen(true)] out LinkUri? linkUri)
	{
		linkUri = null;
		if (url == null) return false;

		Match match = _regex.Match(url);
		if (!match.Success) return false;

		linkUri = new LinkUri
		{
			Url = url,
			Prefix = match.Groups["prefix"].Value.ToLower(),
			Type = match.Groups["type"].Value.ToLower(),
			Version = ParseVersion(match.Groups["version"].Value),
			Path = match.Groups["path"].Value,
			Query = match.Groups["query"].Value,
		};
		return true;
	}

	private static Version? ParseVersion(string? version)
	{
		if (version.IsNullOrEmpty()) return null;

		List<string> parts = version.Split('.').ToList();
		while (parts.Count < 2)
		{
			parts.Add("0");
		}

		return new Version(string.Join(".", parts));
	}
}
