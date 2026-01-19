using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SideScroll;

/// <summary>
/// Represents a structured URI with format: &lt;prefix&gt;://&lt;type&gt;/[v&lt;version&gt;/]&lt;path&gt;[?&lt;query&gt;]
/// </summary>
[PublicData]
public class LinkUri
{
	/// <summary>
	/// Gets or sets the URI prefix
	/// </summary>
	public string? Prefix { get; set; }

	/// <summary>
	/// Gets or sets the URI type
	/// </summary>
	public string? Type { get; set; }

	/// <summary>
	/// Gets or sets the optional version number
	/// </summary>
	public Version? Version { get; set; }

	/// <summary>
	/// Gets or sets the URI path
	/// </summary>
	public string? Path { get; set; }

	/// <summary>
	/// Gets or sets the optional query string
	/// </summary>
	public string? Query { get; set; }

	/// <summary>
	/// Gets or sets the complete URL string
	/// </summary>
	public string? Url { get; set; }

	private static readonly Regex _regex = new(@"^(?<prefix>[a-zA-Z]+)\:\/\/(?<type>[-0-9a-zA-Z\.]+)\/(v(?<version>[\d\.]+)\/)?(?<path>[^\?]+)(\?(?<query>.+))?$");

	/// <summary>
	/// Initializes a new empty instance of the LinkUri class
	/// </summary>
	public LinkUri() { }

	/// <summary>
	/// Initializes a new instance of the LinkUri class with the specified components
	/// </summary>
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

	/// <summary>
	/// Validates that required URI components (Prefix, Type, and Path) are present
	/// </summary>
	public virtual bool IsValid() =>
		!Prefix.IsNullOrEmpty() &&
		!Type.IsNullOrEmpty() &&
		!Path.IsNullOrEmpty();

	/// <summary>
	/// Converts the LinkUri components into a complete URI string
	/// </summary>
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

	/// <summary>
	/// Parses a URL string into a LinkUri object
	/// </summary>
	public static LinkUri Parse(string url)
	{
		if (TryParse(url, out LinkUri? linkUri)) return linkUri;

		throw new ArgumentException($"Invalid LinkUri {url}");
	}

	/// <summary>
	/// Attempts to parse a URL string into a LinkUri object
	/// </summary>
	/// <param name="url">The URL string to parse</param>
	/// <param name="linkUri">When successful, contains the parsed LinkUri; otherwise null</param>
	/// <returns>True if the URL was successfully parsed; otherwise false</returns>
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
